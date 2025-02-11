using System;

namespace ADCS.SidExtension.PolicyModule.Forms;

/// <summary>
/// Represents template-requester map entry.
/// </summary>
public class TemplateRequesterMap {
    public TemplateRequesterMap(String templateOid, String requesterName) {
        TemplateOid = templateOid;
        RequesterName = requesterName;
    }

    /// <summary>
    /// Gets or sets the template object identifier (OID).
    /// </summary>
    public String TemplateOid { get; }
    /// <summary>
    /// Gets or sets requester name in a form 'DomainName\AccountName'.
    /// </summary>
    public String RequesterName { get; }

    /// <summary>
    /// Determines if current entry is properly configured.
    /// </summary>
    /// <returns><strong>True</strong> if entry is properly configured, otherwise <strong>False</strong>.</returns>
    public Boolean IsValid() {
        return !String.IsNullOrWhiteSpace(TemplateOid) && !String.IsNullOrWhiteSpace(RequesterName) && RequesterName.Contains("\\");
    }

    /// <inheritdoc />
    public override String ToString() {
        return $"{TemplateOid}:{RequesterName}";
    }
}