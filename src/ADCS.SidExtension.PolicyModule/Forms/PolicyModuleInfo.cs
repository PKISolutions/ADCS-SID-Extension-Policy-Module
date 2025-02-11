using System;
using System.Text.RegularExpressions;
using ADCS.CertMod.Managed.Policy;

namespace ADCS.SidExtension.PolicyModule.Forms;

class PolicyModuleInfo {
    static readonly Regex _regex = new(@"\.Policy\.(\d+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    PolicyModuleInfo(String progID) {
        Name = ProgID = progID;
    }

    public String Name { get; set; }
    public String ProgID { get; }

    public override Boolean Equals(Object? obj) {
        return !ReferenceEquals(null, obj) && (ReferenceEquals(this, obj) ||
                                               obj.GetType() == GetType() && Equals((PolicyModuleInfo)obj));
    }
    protected Boolean Equals(PolicyModuleInfo other) {
        return String.Equals(ProgID, other.ProgID, StringComparison.OrdinalIgnoreCase);
    }
    public override Int32 GetHashCode() {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(ProgID);
    }


    public static PolicyModuleInfo CreateFromProgID(String progID) {
        var policyInfo = new PolicyModuleInfo(progID);

        try {
            var comType = Type.GetTypeFromProgID(progID, true);
            if (Activator.CreateInstance(comType) is not ICertPolicy policyModule) {
                return policyInfo;
            }

            policyInfo.Name = policyModule.GetDescription();
            // Newer versions of policy module are appended by ".X", where X is the number.
            Match match = _regex.Match(progID);
            if (match.Success) {
                policyInfo.Name += $" ({match.Groups[1].Value})";
            }
        } catch { }

        return policyInfo;
    }
}