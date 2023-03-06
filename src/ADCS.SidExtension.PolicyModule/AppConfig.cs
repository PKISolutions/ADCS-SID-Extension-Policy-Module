using System;
using System.Collections.Generic;
using System.Linq;
using ADCS.CertMod.Managed;
using ADCS.SidExtension.PolicyModule.Forms;
using Microsoft.Win32;

namespace ADCS.SidExtension.PolicyModule;

public class AppConfig : RegistryService {
    public const String PROP_DS_DO_NOT_USE_GC           = "DoNotUseGC";
    public const String PROP_LOG_LEVEL                  = "LogLevel";
    public const String PROP_NATIVE_POLICY_PROG_ID      = "NativeProgID";
    public const String PROP_MAP_LIST                   = "AuthorizedMap";
    public const String PROP_TRUSTED_EXTENSION_POLICY   = "TrustedSidExtensionPolicy";
    public const String PROP_UNTRUSTED_EXTENSION_POLICY = "UntrustedSidExtensionPolicy";

    readonly ILogWriter _logWriter;

    public AppConfig(String moduleName, ILogWriter logWriter) : base(moduleName, true) {
        _logWriter = logWriter;
    }

    public Boolean InitializeConfig(String config) {
        try {
            Initialize(config);
            _logWriter.LogLevel = GetLogLevel();
        } catch (Exception ex) {
            _logWriter.LogError(ex, "[AppConfig::InitializeConfig]");
            return false;
        }

        return true;
    }

    #region PROP_DS_DO_NOT_USE_GC

    public Boolean GetDsDoNotUseGC() {
        try {
            RegTriplet triplet = GetRecord(PROP_DS_DO_NOT_USE_GC);
            if (triplet is { Type: RegistryValueKind.DWord }) {
                return Convert.ToBoolean(triplet.Value);
            }
        } catch { }

        return false;
    }
    public void SetDsDoNotUseGC(Boolean doNotUseGC) {
        WriteRecord(new RegTriplet(PROP_DS_DO_NOT_USE_GC, RegistryValueKind.DWord) {
            Value = Convert.ToInt32(doNotUseGC)
        });
    }

    #endregion

    #region LogLevel

    public LogLevel GetLogLevel() {
        try {
            RegTriplet triplet = GetRecord(PROP_LOG_LEVEL);
            if (triplet is { Type: RegistryValueKind.DWord }) {
                return (LogLevel)triplet.Value;
            }
        } catch { }

        return LogLevel.None;
    }
    public void SetLogLevel(LogLevel logLevel) {
        WriteRecord(new RegTriplet(PROP_LOG_LEVEL, RegistryValueKind.DWord) {
            Value = logLevel
        });
    }

    #endregion

    #region Native policy module

    public String GetNativePolicyModule() {
        try {
            RegTriplet triplet = GetRecord(PROP_NATIVE_POLICY_PROG_ID);
            if (triplet is { Type: RegistryValueKind.String }) {
                return triplet.Value?.ToString();
            }
            
        } catch { }

        return null;
    }
    public void SetNativePolicyModule(String progID) {
        WriteRecord(new RegTriplet(PROP_NATIVE_POLICY_PROG_ID, RegistryValueKind.String) {
            Value = progID
        });
    }

    #endregion

    #region Authorization map

    public List<TemplateRequesterMap> GetAuthorizedMap() {
        var retValue = new List<TemplateRequesterMap>();
        try {
            RegTriplet triplet = GetRecord(PROP_MAP_LIST);
            if (triplet is { Type: RegistryValueKind.MultiString }) {
                foreach (String line in (String[])triplet.Value) {
                    if (String.IsNullOrWhiteSpace(line)) {
                        continue;
                    }

                    String[] tokens = line.Split(':');
                    if (tokens.Length < 2) {
                        continue;
                    }

                    retValue.Add(TemplateRequesterMap.FromMap(tokens[0], tokens[1]));
                }
            }
            
        } catch { }

        return retValue;
    }
    public void SetAuthorizedMap(IEnumerable<String> map) {
        WriteRecord(new RegTriplet(PROP_MAP_LIST, RegistryValueKind.MultiString) {
            Value = map.ToArray()
        });
    }

    #endregion

    #region Trusted SID Extension policy

    public SidExtensionAction GetTrustedSidExtensionPolicy() {
        try {
            RegTriplet triplet = GetRecord(PROP_TRUSTED_EXTENSION_POLICY);
            if (triplet is { Type: RegistryValueKind.DWord }) {
                return (SidExtensionAction)triplet.Value;
            }
            
        } catch { }

        return SidExtensionAction.PassThrough;
    }
    public void SetTrustedSidExtensionPolicy(SidExtensionAction policy) {
        WriteRecord(new RegTriplet(PROP_TRUSTED_EXTENSION_POLICY, RegistryValueKind.DWord) {
            Value = policy
        });
    }

    #endregion

    #region Untrusted SID Extension policy

    public SidExtensionAction GetUntrustedSidExtensionPolicy() {
        try {
            RegTriplet triplet = GetRecord(PROP_UNTRUSTED_EXTENSION_POLICY);
            if (triplet is { Type: RegistryValueKind.DWord }) {
                return (SidExtensionAction)triplet.Value;
            }
            
        } catch { }

        return SidExtensionAction.PassThrough;
    }
    public void SetUntrustedSidExtensionPolicy(SidExtensionAction policy) {
        WriteRecord(new RegTriplet(PROP_UNTRUSTED_EXTENSION_POLICY, RegistryValueKind.DWord) {
            Value = policy
        });
    }

    #endregion
}