using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ADCS.CertMod.Managed;
using CERTENROLLLib;

namespace ADCS.SidExtension.PolicyModule;

public static class RequestExtensionListExtensions {
    const String SAN_SID_PATTERN = @"microsoft\.com,2022-09-14:sid";
    static ILogWriter logWriter;

    /// <summary>
    /// Processes incoming request extensions. This method performs multiple tasks in one run:
    /// <list type="bullet">
    ///     <item>Searches for all potential locations</item>
    ///     <item>Attempts to search requested principal name if method requests to do so</item>
    /// </list>
    /// </summary>
    /// <param name="extensions">A collection of request extensions.</param>
    /// <param name="subjectType">Subject type specified by certificate template.</param>
    /// <param name="searchPrincipal">Specifies whether target principal search must be executed.</param>
    /// <param name="logger">Logger.</param>
    /// <returns>A DTO object that contains request extension processing results.</returns>
    public static RequestExtensionProcessResult ProcessRequestExtensions(this IReadOnlyList<RequestExtension> extensions, SubjectType subjectType, Boolean searchPrincipal, ILogWriter logger) {
        logWriter = logger;
        var retValue = new RequestExtensionProcessResult();
        if (extensions.Any(x => Constants.SID_EXTENSION_OID.Equals(x.ExtensionName.Value))) {
            logger.LogDebug("Found SID extension in request.");
            retValue.SidValueLocation |= SidValueLocation.SidExtension;
        }

        Boolean isFirstSanOccurrence = true;
        Boolean isFirstSidValueInSanOccurrence = true;
        // loop over all extensions
        for (Int32 index = 0; index < extensions.Count; index++) {
            // skip non-SAN extensions
            if (!Constants.SAN_EXTENSION_OID.Equals(extensions[index].ExtensionName.Value)) {
                continue;
            }
            logger.LogDebug("Found SAN extension at index {0}.", index);
            retValue.HasSanExtension = true;
            if ((extensions[index].Flags & RequestExtensionFlags.Critical) == RequestExtensionFlags.Critical) {
                logger.LogDebug("SAN extension is critical.", index);
                retValue.SanIsCritical = true;
            }
            // once we hit SAN extension, read it
            var ext = new CX509ExtensionAlternativeNames();
            ext.InitializeDecode(EncodingType.XCN_CRYPT_STRING_BASE64, Convert.ToBase64String(extensions[index].Value));
            // we read principal name only from very first SAN extension occurrence.
            if (isFirstSanOccurrence && searchPrincipal) {
                isFirstSanOccurrence = false;
                readPrincipalName(ext, subjectType, retValue);
            }

            // and loop over all alt names of type of URL in every SAN extension
            foreach (CAlternativeName altName in ext.AlternativeNames) {
                if (altName.Type == AlternativeNameType.XCN_CERT_ALT_NAME_URL) {
                    // attempt to match URL name type value against pattern
                    if (Regex.IsMatch(altName.strValue, SAN_SID_PATTERN, RegexOptions.Compiled | RegexOptions.IgnoreCase)) {
                        // if match, add SID value location and break the loop
                        if (isFirstSidValueInSanOccurrence) {
                            logger.LogDebug("Found SID value in SAN extension in request.");
                            isFirstSidValueInSanOccurrence = false;
                        }
                        retValue.SidValueLocation |= SidValueLocation.SanExtension;
                    } else {
                        retValue.SafeAltNames.Add(altName);
                    }
                } else {
                    retValue.SafeAltNames.Add(altName);
                }
            }
        }

        return retValue;
    }

    static void readPrincipalName(IX509ExtensionAlternativeNames san, SubjectType subjectType, RequestExtensionProcessResult retValue) {
        if (san == null) {
            return;
        }
        // pick appropriate SAN name type depending on subject type in template.
        // UPN for user templates and DNS name for machine templates
        AlternativeNameType altNameType;
        switch (subjectType) {
            case SubjectType.User:
                altNameType = AlternativeNameType.XCN_CERT_ALT_NAME_USER_PRINCIPLE_NAME;
                logWriter.LogDebug("Executing principal search from UPN.");
                break;
            case SubjectType.Computer:
                altNameType = AlternativeNameType.XCN_CERT_ALT_NAME_DNS_NAME;
                logWriter.LogDebug("Executing principal search from DNS.");
                break;
            default:
                return;
        }
        // try to find desired name type. We read only first occurrence of desired name type 
        IAlternativeName altName = san.AlternativeNames.Cast<IAlternativeName>().FirstOrDefault(x => x.Type == altNameType);
        if (altName == null) {
            logWriter.LogDebug("No matching alternative name found in SAN extension.");
        } else {
            retValue.PrincipalName = altName.strValue;
        }
    }
}