This page describes how policy module performs account lookup for offline requests.

# Subject type
Account lookup details depend on requested template's subject type.

## Machine template
If requested template's subject type is Machine, then policy module examines Subject Alternative Names (SAN) extension in request and looks for the first occurence of `dnsName` name type. If found, policy module looks for a computer account in Active Directory with matching `dNSHostName` DS attribute.

## User
If requested template's subject type is User, then policy module examines Subject Alternative Names (SAN) extension in request and looks for the first occurence of `otherName` name type with `userPrincipalName` (UPN) other name identifier. If found, policy module looks for a user account in Active Directory with matching `userPrincipalName` DS attribute.

## Other
No lookup will be performed.

# Account lookup in AD
- if **Do not use Global Catalog** setting is unchecked, a LDAP search is sent to any available global catalog server installed in current domain. Search may fail if there is no global catalogs installed in current domain. We do not require a direct LDAP link between CA and target domain in case if name identifier belongs to external domain, however search results may fail if global catalog doesn't yet replicated forest data from other global catalogs in the forest.
- if **Do not use Global Catalog** setting is checked, a LDAP search is sent to any available domain controller in destination (external) domain. This requires a direct LDAP link between CA and destination domain controllers.
