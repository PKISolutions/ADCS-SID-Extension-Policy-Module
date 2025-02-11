using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ADCS.CertMod.Managed;
using Microsoft.Win32;

namespace ADCS.SidExtension.PolicyModule;

static class CertTemplateCache {
    const String REG_TMPL_CACHE_PATH              = @"SOFTWARE\Microsoft\Cryptography\CertificateTemplateCache";
    const Int32 CT_FLAG_ENROLLEE_SUPPLIES_SUBJECT = 1;

    // the purpose of this locker is to ensure that:
    // - cache is not read-accessed while it is re-building
    // - cache is not rebuilt while it is in read-access
    static readonly Object _locker = new();
    static readonly IDictionary<String, CertTemplateInfo> _templateCache = new Dictionary<String, CertTemplateInfo>();
    static ILogWriter? localLogger;
        
    static Timer? timer;
    static Int64 timerPeriod;

    public static void Start(ILogWriter logger) {
        if (timer is null) {
            localLogger = logger;
            timerPeriod = Convert.ToInt64(TimeSpan.FromMinutes(1).TotalMilliseconds);
            timer = new Timer(onTimerTrigger, null, 0, timerPeriod);
        }
    }
    public static void Stop() {

    }

    static void onTimerTrigger(Object state) {
        Int32 retryAttempts = 3;
        // stop timer.
        timer!.Change(Timeout.Infinite, Timeout.Infinite);
        
        lock (_locker) {
            // sometimes we can hit a moment when CA rebuilds the cache and registry key doesn't exist yet
            // so retry up to three times with 200ms delays.
            while (!rebuildCache() && retryAttempts-- > 0) {
                Thread.Sleep(200);
            }
        }
        // restart timer.
        timer.Change(timerPeriod, timerPeriod);
    }
    static Boolean rebuildCache() {
        _templateCache.Clear();
        try {
            using RegistryKey? key = Registry.LocalMachine.OpenSubKey(REG_TMPL_CACHE_PATH);
            if (key is null) {
                localLogger!.LogDebug("[CertTemplateCache::ReloadCache] Template cache key is null");
                return false;
            }
            foreach (String subKeyName in key.GetSubKeyNames()) {
                RegistryKey? subKey = key.OpenSubKey(subKeyName);
                if (subKey is null) {
                    localLogger!.LogDebug($"[CertTemplateCache::ReloadCache] Subkey '{subKeyName}' is null");
                    continue;
                }
                        
                // read template OID
                String[] templateOid = (String[])subKey.GetValue("msPKI-Cert-Template-OID", Array.Empty<String>());
                if (!templateOid.Any()) {
                    localLogger!.LogDebug($"[CertTemplateCache::ReloadCache] Failed to read template oid for '{subKeyName}'");
                    continue;
                }
                        
                // read display name
                String displayName = (String)subKey.GetValue("DisplayName", "UNKNOWN");
                // read schema version
                Int32 schemaVersion = (Int32)subKey.GetValue("msPKI-Template-Schema-Version", 1);
                String identifier = schemaVersion > 1
                    ? templateOid[0]
                    : subKeyName;

                // read subject type
                SubjectType subjectType = getSubjectType((Int32)subKey.GetValue("Flags", 0));

                // read template offline status
                Int32 nameFlags = (Int32)subKey.GetValue("msPKI-Certificate-Name-Flag", 0);
                Boolean isOffline = (nameFlags & CT_FLAG_ENROLLEE_SUPPLIES_SUBJECT) == CT_FLAG_ENROLLEE_SUPPLIES_SUBJECT;

                // insert into cache
                _templateCache.Add(identifier, new CertTemplateInfo(displayName, templateOid[0], subjectType, isOffline));
            }

            return true;
        } catch (Exception ex) {
            localLogger!.LogError(ex, "[CertTemplateCache::ReloadCache]");
        }

        return false;
    }

    public static CertTemplateInfo? GetTemplateInfo(String templateId) {
        lock (_locker) {
            return _templateCache.TryGetValue(templateId, out CertTemplateInfo? templateInfo)
                ? templateInfo
                : null;
        }
    }
    public static List<CertTemplateInfo> GetTemplateInfoList() {
        lock (_locker) {
            return _templateCache.Values.OrderBy(x => x.DisplayName).ToList();
        }
    }

    static SubjectType getSubjectType(Int32 flags) {
        if ((flags & (Int32)CertificateTemplateFlags.IsCA) > 0) {
            return SubjectType.CA;
        }
        if ((flags & (Int32)CertificateTemplateFlags.MachineType) > 0) {
            return SubjectType.Computer;
        }
        return (flags & (Int32)CertificateTemplateFlags.IsCrossCA) > 0
            ? SubjectType.CrossCA
            : SubjectType.User;
    }
}