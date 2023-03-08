using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using ADCS.CertMod.Managed;
using ADCS.CertMod.Managed.Policy;
using ADCS.SidExtension.PolicyModule.Forms;
using CERTENROLLLib;

namespace ADCS.SidExtension.PolicyModule;

[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
[ProgId("SysadminsLV_SID.Policy")]
[Guid("4335db31-edc5-4277-b1ee-25b88a05192c")]
public class Policy : CertPolicyBase {
    const String SID_EXTENSION_OID = "1.3.6.1.4.1.311.25.2";
    const String SAN_EXTENSION_OID = "2.5.29.17";

    readonly AppConfig _config;

    List<TemplateRequesterMap> map = new();
    SidExtensionAction trustedSidPolicy, untrustedSidPolicy;
    ICertManageModule policyManage;

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
            
        // initialize
        try {
            if (validatePrerequisites(out CertTemplateInfo targetTemplate)) {
                // process request if all validations and prerequisites passed
                return processRequest(targetTemplate, nativeResult);
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
        if (policyManage == null) {
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
    Boolean validatePrerequisites(out CertTemplateInfo targetTemplate) {
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
        if (targetTemplate == null) {
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
        // read all extensions from request
        List<RequestExtension> extensionList = CertServer.GetRequestExtensions().ToList();
        if (match) {
            // if there is a valid match for template and requester, then:
            // - try to read SAN extension from request
            // - try to find subject identity in AD using corresponding alt name type (UPN for user, dnsHostName for computer)
            // - try to insert found SID into dummy certificate
            // - return appropriate response back to CA
            RequestExtension sanExtension = extensionList.FirstOrDefault(x => SAN_EXTENSION_OID.Equals(x.ExtensionName.Value));
            // if we were unable to populate SID extension ourselves, follow the policy:
            // either, pass through (issue the cert without SID extension) or deny request.
            if (!addSidToCertificate(sanExtension, templateInfo.SubjectType)) {
                return enforceSidExtensionPolicy(trustedSidPolicy);
            }
        } else {
            // if there is no valid match for template and requester, then enforce untrusted SID extension policy
            // if necessary (only when untrusted SID extension is presented in request).
            RequestExtension sidExtension = extensionList.FirstOrDefault(x => SID_EXTENSION_OID.Equals(x.ExtensionName.Value));
            if (sidExtension != null) {
                PolicyModuleAction result = enforceSidExtensionPolicy(untrustedSidPolicy);
                // if native policy module asks to pend request and this policy module is configured to issue
                // such request, we shall return native policy module result. In other words, when untrusted SID extension
                // policy is set to 'Issue', we are transparent to CA and forward response from native policy module.
                if (result == PolicyModuleAction.Issue && nativeResult == PolicyModuleAction.PutToPending) {
                    return nativeResult;
                }

                return result;
            }
        }

        return nativeResult;
    }
    PolicyModuleAction enforceSidExtensionPolicy(SidExtensionAction policy) {
        switch (policy) {
            case SidExtensionAction.Suppress:
                CertServer.GetManagedPolicyModule().DisableCertificateExtension(SID_EXTENSION_OID);
                break;
            case SidExtensionAction.Deny:
                return PolicyModuleAction.Deny;
            case SidExtensionAction.Pending:
                return PolicyModuleAction.PutToPending;
        }

        return PolicyModuleAction.Issue;
    }
    Boolean addSidToCertificate(RequestExtension sanExtension, SubjectType subjectType) {
        if (sanExtension == null) {
            Logger.LogDebug("SAN extension not found. Skipping.");

            return false;
        }

        var ext = new CX509ExtensionAlternativeNames();
        ext.InitializeDecode(EncodingType.XCN_CRYPT_STRING_BASE64, Convert.ToBase64String(sanExtension.Value));
        // pick appropriate SAN name type depending on subject type in template.
        // UPN for user templates and DNS name for machine templates
        AlternativeNameType altNameType;
        switch (subjectType) {
            case SubjectType.User:
                altNameType = AlternativeNameType.XCN_CERT_ALT_NAME_USER_PRINCIPLE_NAME;
                break;
            case SubjectType.Computer:
                altNameType = AlternativeNameType.XCN_CERT_ALT_NAME_DNS_NAME;
                break;
            default:
                return false;
        }
        // try to find desired name type. We read only first occurrence of desired name type 
        IAlternativeName san = ext.AlternativeNames.Cast<IAlternativeName>().FirstOrDefault(x => x.Type == altNameType);
        if (san == null) {
            Logger.LogDebug("Alternative name not found in SAN extension. Skipping.");
            return false;
        }
        // read found alternative name value
        String principalName = san.strValue;
        Logger.LogDebug("Alt principal name: {0}", principalName);
        if (String.IsNullOrWhiteSpace(principalName)) {
            return false;
        }

        // invoke appropriate search method to query for identity's SID based on subject type
        String sid = null;
        switch (subjectType) {
            case SubjectType.User:
                sid = DsUtils.FindSidByPrincipalName(principalName);
                break;
            case SubjectType.Computer:
                sid = DsUtils.FindSidByDnsName(principalName);
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

        return true;
    }
}