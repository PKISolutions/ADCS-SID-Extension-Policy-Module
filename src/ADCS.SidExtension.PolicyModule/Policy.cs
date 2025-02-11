using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using ADCS.CertMod.Managed;
using ADCS.CertMod.Managed.Policy;
using ADCS.SidExtension.PolicyModule.Forms;

namespace ADCS.SidExtension.PolicyModule;

[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
[ProgId("PKISolutions_SID.Policy")]
[Guid("4335db31-edc5-4277-b1ee-25b88a05192c")]
public class Policy : CertPolicyBase {

    readonly AppConfig _config;

    List<TemplateRequesterMap> map = new();
    SidExtensionAction trustedSidPolicy, untrustedSidPolicy;
    ICertManageModule? policyManage;

    public Policy() : base(new LogWriter("SID.Policy", LogLevel.Error)) {
        _config = new AppConfig("PKISolutions_SID.Policy", Logger);
    }

    /// <inheritdoc />
    public override PolicyModuleAction VerifyRequest(String strConfig, Int32 Context, Int32 bNewRequest, Int32 Flags) {
        // read native result from underlying policy module
        PolicyModuleAction nativeResult = base.VerifyRequest(strConfig, Context, bNewRequest, Flags);
        // if native policy module did not pass the request, follow its decision
        if (nativeResult != PolicyModuleAction.Issue && nativeResult != PolicyModuleAction.PutToPending) {
            return nativeResult;
        }
            
        try {
            // validate SID Policy Module pre-conditions:
            // - requested template is offline.
            if (validatePrerequisites(out CertTemplateInfo targetTemplate)) {
                Logger.LogDebug("Requested template is offline. Starting request processing.");
                // process request if all validations and prerequisites passed
                PolicyModuleAction result =  processRequest(targetTemplate, nativeResult);
                // we do not want to place request into pending state when it is already pending, because it will
                // fail to successfully resolve the request.
                if (bNewRequest == 0 && result == PolicyModuleAction.PutToPending) {
                    Logger.LogDebug("Resulting certificate action is to pend request. However, the request is already in pending state. Instructing CA to issue the certificate.");
                    return PolicyModuleAction.Issue;
                }

                return result;
            } else {
                Logger.LogDebug("Requested template is not offline. Skip processing and return native policy module result.");
            }
        } catch (Exception ex) {
            Logger.LogError(ex, "[Policy::VerifyRequest]");
        } finally {
            CertServer.FinalizeContext();
        }

        return nativeResult;
    }
    /// <inheritdoc />
    public override void Initialize(String strConfig) {
        if (!_config.InitializeConfig(strConfig)) {
            throw new Exception("Failed to initialize policy module configuration");
        }

        DefaultPolicyProgID = _config.GetNativePolicyModule();
        Logger.LogLevel     = _config.GetLogLevel();
        trustedSidPolicy    = _config.GetTrustedSidExtensionPolicy();
        untrustedSidPolicy  = _config.GetUntrustedSidExtensionPolicy();
        map                 = _config.GetAuthorizedMap();
        DsUtils.DoNotUseGC  = _config.GetDsDoNotUseGC();
        CertTemplateCache.Start(Logger);

        base.Initialize(strConfig);
    }
    /// <inheritdoc />
    public override String GetDescription() {
        return "NTDS CA Security Extension Enforcement";
    }
    /// <inheritdoc />
    public override ICertManageModule GetManageModule() {
        if (policyManage is null) {
            return policyManage = new PolicyManage(Logger, _config);
        }

        return policyManage;
    }

    /// <summary>
    /// Determines whether current request should be additionally processed by policy module.
    /// </summary>
    /// <param name="targetTemplate"></param>
    /// <returns>
    ///     <strong>True</strong> if request should be processed by policy 
    /// </returns>
    Boolean validatePrerequisites(out CertTemplateInfo? targetTemplate) {
        targetTemplate = null;
        Logger.LogTrace("[Policy::validatePrerequisites] Retrieve template name.");
        // read template name/OID from incoming request
        String templateName = CertServer.GetCertificateTemplate() ?? String.Empty;
        if (String.IsNullOrWhiteSpace(templateName)) {
            Logger.LogTrace("[Policy::validatePrerequisites] Template name is empty.");

            return false;
        }
        Logger.LogTrace("Template: {0}", templateName);
        // get requested template info from cache
        targetTemplate = CertTemplateCache.GetTemplateInfo(templateName);
        // do basic validations
        if (targetTemplate is null) {
            Logger.LogDebug("[Policy::validatePrerequisites] Requested template '{0}' is not found in local cache. Skipping.", templateName);

            return false;
        }

        // check if requested template is online or offline. If it is online, skip further processing.
        if (!targetTemplate.IsOffline) {
            Logger.LogDebug($"[Policy::validatePrerequisites] Requested template '{templateName}' is not offline. Skipping.");
            return false;
        }

        return true;
    }
    PolicyModuleAction processRequest(CertTemplateInfo templateInfo, PolicyModuleAction nativeResult) {
        // read configuration entries that contain requested template.
        // single template supports multiple configurations, so return value is a collection of
        // requested template configurations
        IEnumerable<TemplateRequesterMap> subset = map.Where(x => templateInfo.OID.Equals(x.TemplateOid)).ToList();
        // read requester name from incoming request
        String requesterName = CertServer.GetRequesterName();
        Logger.LogDebug($"[Policy::VerifyRequest] Requester name: {requesterName}");
        // check if there is a requester match in template configurations. Otherwise, return underlying policy module result
        Boolean match = subset.Any(x => requesterName.Equals(x.RequesterName, StringComparison.OrdinalIgnoreCase));
        Logger.LogDebug(match
            ? "Found matching entry in Template/Requester table."
            : "No matching entry was found in Template/Requester table.");
        // read all extensions from request
        Logger.LogDebug("Reading request extensions.");
        IReadOnlyList<RequestExtension> extensionList = CertServer.GetRequestExtensions().ToList();
        Logger.LogDebug("Found {0} extensions.", extensionList.Count);
        // first, read request extensions as follows:
        // - find location in request extensions that may contain SID extension.
        // - try to read SAN extension from request
        // - try to find subject identity in AD using corresponding alt name type (UPN for user, dnsHostName for computer)
        RequestExtensionProcessResult extProcessResult = extensionList.ProcessRequestExtensions(templateInfo.SubjectType, match, Logger);
        if (match) {
            // if we were unable to populate SID extension ourselves, follow the policy:
            // either, pass through (issue the cert without SID extension) or deny request.
            if (!addSidToCertificate(extProcessResult, templateInfo.SubjectType)) {
                return enforceSidExtensionPolicy(extProcessResult, trustedSidPolicy);
            }
        } else {
            // if there is no valid match for template and requester, then enforce untrusted SID extension policy
            // if necessary.
            PolicyModuleAction result = enforceSidExtensionPolicy(extProcessResult, untrustedSidPolicy);
            // if native policy module asks to pend request and this policy module is configured to issue
            // such request, we shall return native policy module result. In other words, when untrusted SID extension
            // policy is set to 'Issue', we are transparent to CA and forward response from native policy module.
            if (result == PolicyModuleAction.Issue && nativeResult == PolicyModuleAction.PutToPending) {
                return nativeResult;
            }

            return result;
        }

        return nativeResult;
    }
    PolicyModuleAction enforceSidExtensionPolicy(RequestExtensionProcessResult extProcessResult, SidExtensionAction policy) {
        Logger.LogDebug("Enforcing SID extension policy. Requested action: {0}", policy);
        switch (policy) {
            case SidExtensionAction.Suppress:
                // if SID value is found in SID extension, disable the extension
                if ((extProcessResult.SidValueLocation & SidValueLocation.SidExtension) == SidValueLocation.SidExtension) {
                    disableSidExtension();
                }
                // if SID value is found in SAN extension, rebuild SAN extension by excluding rogue SID names.
                if ((extProcessResult.SidValueLocation & SidValueLocation.SanExtension) == SidValueLocation.SanExtension) {
                    // re-create SAN extension
                    rebuildSanExtension(extProcessResult);
                }
                break;
            case SidExtensionAction.Deny:
                return PolicyModuleAction.Deny;
            case SidExtensionAction.Pending:
                return PolicyModuleAction.PutToPending;
        }

        return PolicyModuleAction.Issue;
    }
    void disableSidExtension() {
        Logger.LogDebug("Disabling SID extension.");
        CertServer.GetManagedPolicyModule().DisableCertificateExtension(Constants.SID_EXTENSION_OID);
    }
    void rebuildSanExtension(RequestExtensionProcessResult extProcessResult) {
        // check if SID value was found in SAN extension. If not, do nothing.
        if ((extProcessResult.SidValueLocation & SidValueLocation.SanExtension) == SidValueLocation.SanExtension) {
            Logger.LogDebug("Attempting to rebuild the SAN extension after removing SID values.");
            // try to create SAN extension
            X509Extension? sanExtension = extProcessResult.CreateSafeSanExtension();
            if (sanExtension is null) {
                Logger.LogDebug("SID value is the only alternative name in SAN extension. Disabling SAN extension completely.");
                // if no safe names found (i.e. SAN contains only rogue SID value),
                // then we effectively disable extension.
                CertServer.GetManagedPolicyModule().DisableCertificateExtension(Constants.SAN_EXTENSION_OID);
            } else {
                Logger.LogDebug("Overwriting SAN extension with excluded SID values.");
                // otherwise override SAN extension by including only safe names.
                CertServer.GetManagedPolicyModule().SetCertificateExtension(sanExtension);
            }
        }
    }

    Boolean addSidToCertificate(RequestExtensionProcessResult extensionProcessResult, SubjectType subjectType) {
        if (!extensionProcessResult.HasSanExtension) {
            Logger.LogDebug("SAN extension not found. Skipping.");

            return false;
        }
        if (!extensionProcessResult.FoundPrincipalName) {
            Logger.LogDebug("Alternative name not found in SAN extension. Skipping.");

            return false;
        }
        Logger.LogDebug("Alt principal name: {0}", extensionProcessResult.PrincipalName);
        String sid = null;
        switch (subjectType) {
            case SubjectType.User:
                sid = DsUtils.FindSidByPrincipalName(extensionProcessResult.PrincipalName);
                break;
            case SubjectType.Computer:
                sid = DsUtils.FindSidByDnsName(extensionProcessResult.PrincipalName);
                break;
        }
        Logger.LogDebug("Returned SID: {0}", sid);
        if (String.IsNullOrEmpty(sid)) {
            Logger.LogDebug("SID was not found for requested alternative name. Skipping.");
            return false;
        }
        // insert SID extension into dummy certificate
        X509Extension extension = AsnUtils.EncodeSidExtension(sid);
        CertServer.GetManagedPolicyModule().SetCertificateExtension(extension);
        // rebuild SAN extension if necessary to exclude rogue SID value from SAN extension.
        rebuildSanExtension(extensionProcessResult);

        return true;
    }
}