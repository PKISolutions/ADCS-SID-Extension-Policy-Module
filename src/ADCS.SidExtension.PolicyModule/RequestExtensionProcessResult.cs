using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using CERTENROLLLib;

namespace ADCS.SidExtension.PolicyModule;

/// <summary>
/// Represents request extension processing result that contains all required information about SID extensions and
/// request principals.
/// </summary>
public class RequestExtensionProcessResult {
    /// <summary>
    /// Gets or sets SID value locations.
    /// </summary>
    public SidValueLocation SidValueLocation { get; set; }
    /// <summary>
    /// Gets or sets the value whether request contains SAN extension.
    /// </summary>
    public Boolean HasSanExtension { get; set; }
    /// <summary>
    /// Gets or sets the value whether request contains SID extension.
    /// </summary>
    public Boolean HasSidExtension => (SidValueLocation & SidValueLocation.SidExtension) == SidValueLocation.SidExtension;
    /// <summary>
    /// Gets or sets the value whether request contains SAN extension and it contains at least one
    /// name that matches requested subject type.
    /// </summary>
    public Boolean FoundPrincipalName => !String.IsNullOrWhiteSpace(PrincipalName);
    /// <summary>
    /// Gets or sets target principal name if found.
    /// </summary>
    public String? PrincipalName { get; set; }
    /// <summary>
    /// Gets or sets the value if SAN extension should be critical.
    /// </summary>
    public Boolean SanIsCritical { get; set; }
    /// <summary>
    /// Gets a collection of safe SAN names that exclude potentially rogue SID values.
    /// </summary>
    public IList<CAlternativeName> SafeAltNames { get; } = new List<CAlternativeName>();

    /// <summary>
    /// Creates a safe SAN extension out of a collection of safe names.
    /// </summary>
    /// <returns>Encoded SAN extension if at least one safe alt name is found, otherwise null.</returns>
    public X509Extension? CreateSafeSanExtension() {
        if (!SafeAltNames.Any()) {
            return null;
        }
        var altNames = new CAlternativeNamesClass();
        foreach (CAlternativeName altName in SafeAltNames) {
            altNames.Add(altName);
        }
        var sanExtension = new CX509ExtensionAlternativeNamesClass();
        sanExtension.InitializeEncode(altNames);

        return new X509Extension(
            Constants.SAN_EXTENSION_OID,
            Convert.FromBase64String(sanExtension.RawData[EncodingType.XCN_CRYPT_STRING_BASE64]),
            SanIsCritical);
    }
}