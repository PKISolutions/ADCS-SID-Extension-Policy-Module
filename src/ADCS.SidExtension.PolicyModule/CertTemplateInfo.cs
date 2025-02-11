using System;

namespace ADCS.SidExtension.PolicyModule;

class CertTemplateInfo {
    public CertTemplateInfo(String displayName, String oid, SubjectType subjectType, Boolean isOffline) {
        DisplayName = displayName;
        OID = oid;
        SubjectType = subjectType;
        IsOffline = isOffline;
    }

    public String DisplayName { get; }
    public String OID { get; }
    public SubjectType SubjectType { get; }
    public Boolean IsOffline { get; }

    public override Boolean Equals(Object? obj) {
        return !ReferenceEquals(null, obj) && (ReferenceEquals(this, obj) ||
                                               obj.GetType() == GetType() && Equals((CertTemplateInfo)obj));
    }
    protected Boolean Equals(CertTemplateInfo other) {
        return OID == other.OID;
    }
    public override Int32 GetHashCode() {
        return OID.GetHashCode();
    }
}