using System;

namespace ADCS.SidExtension.PolicyModule.Forms;

public class TemplateRequesterMap {
    public String TemplateOid { get; set; }
    public String RequesterName { get; set; }

    public Boolean IsValid() {
        return !String.IsNullOrWhiteSpace(TemplateOid) && !String.IsNullOrWhiteSpace(RequesterName);
    }

    public override String ToString() {
        return $"{TemplateOid}:{RequesterName}";
    }

    public static TemplateRequesterMap FromMap(String templateOid, String requesterName) {
        return new TemplateRequesterMap {
            TemplateOid = templateOid,
            RequesterName = requesterName
        };
    }
}