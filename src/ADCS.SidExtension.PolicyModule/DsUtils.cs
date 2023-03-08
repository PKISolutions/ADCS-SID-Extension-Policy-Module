using System;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Security.Principal;

namespace ADCS.SidExtension.PolicyModule;

static class DsUtils {
    public static Boolean DoNotUseGC { get; set; }

    public static String FindSidByPrincipalName(String upn) {
        try {
            return findPrincipalSid(getUserQueryFilter(upn), upn);
        } catch { }

        return null;
    }
    public static String FindSidByDnsName(String dnsName) {
        try {
            return findPrincipalSid(getComputerQueryFilter(dnsName), dnsName);
        } catch { }

        return null;
    }

    static String findPrincipalSid(String queryFilter, String name) {
        using var searcher = new DirectorySearcher();
        searcher.Filter = queryFilter;
        searcher.PropertiesToLoad.Add("objectSid");
        searcher.SearchScope = SearchScope.Subtree;
        searcher.SearchRoot = DoNotUseGC
            ? getDomainSearchRoot(name)
            : getGCSearchRoot();

        SearchResult de = searcher.FindOne();
        if (de?.Properties["objectSid"][0] is not Byte[] sidBytes) {
            return null;
        }

        return new SecurityIdentifier(sidBytes, 0).Value;
    }
    static String getUserQueryFilter(String upn) {
        return $"(&(objectCategory=person)(objectClass=user)(userPrincipalName={upn}))";
    }
    static String getComputerQueryFilter(String dnsName) {
        return $"(&(objectCategory=computer)(objectClass=computer)(dNSHostName={dnsName}))";
    }

    static DirectoryEntry getGCSearchRoot() {
        String[] domainTokens = Domain.GetComputerDomain().Name.Split('.');

        return new DirectoryEntry("GC://DC=" + String.Join(",DC=", domainTokens));
    }
    static DirectoryEntry getDomainSearchRoot(String name) {
        String[] domainTokens;

        if (name.Contains("@")) {
            // upn
            Int32 splitIndex = name.IndexOf("@", StringComparison.Ordinal);
            domainTokens = name.Substring(splitIndex + 1).Split('.');
        } else {
            // skip host part and return domain part
            domainTokens = name.Split('.').Skip(1).ToArray();
        }

        return new DirectoryEntry("LDAP://DC=" + String.Join(",DC=", domainTokens));
    }
}