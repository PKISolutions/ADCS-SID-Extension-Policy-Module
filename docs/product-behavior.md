This document provides Policy Module behavior depending on incoming request and module configuration.

SID Policy Module enforces its logic only when the following pre-conditions are met:
- Native/Underlying policy module does not deny request. If deny is requested by native policy module, the request is denied. This behavior cannot be changed.
- Template is offline template, i.e. accepts subject from incoming request. If template builds subject from Active Directory, SID Policy Module forwards native policy module result back to CA and do not evaluate/modify original request.

The following table provides information about columns used in subsequent tables, their descriptions and applicable values:

<table>
    <thead>
        <tr>
            <th>Column name</th>
            <th>Description</th>
            <th>Possible values</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>CSR has SID Ext</td>
            <td>Specifies whether incoming request contains SID extension.</td>
            <td>
                <ul>
                    <li><strong>Yes</strong></li>
                    <li><strong>No</strong></li>
                </ul>
            </td>
        </tr>
        <tr>
            <td>CSR has SID in SAN</td>
            <td>Specifies whether incoming request contains SID value as part of SAN extension. See <a href="https://techcommunity.microsoft.com/t5/ask-the-directory-services-team/preview-of-san-uri-for-certificate-strong-mapping-for-kb5014754/ba-p/3789785">SAN URI for Certificate Strong Mapping for KB5014754</a> for more information.</td>
            <td>
                <ul>
                    <li><strong>Yes</strong></li>
                    <li><strong>No</strong></li>
                </ul>
            </td>
        </tr>
        <tr>
            <td>Trusted SID Policy</td>
            <td>Specifies Trusted SID policy configuration. This setting applies when incoming request matches any Template/Requester map entry.</td>
            <td>
                <ul>
                    <li><strong>*</strong> — any setting</li>
                    <li><strong>PassThrough</strong></li>
                    <li><strong>Pending</strong></li>
                    <li><strong>Suppress</strong></li>
                    <li><strong>Deny</strong></li>
                </ul>
            </td>
        </tr>
        <tr>
            <td>Untrusted SID Policy</td>
            <td>Specifies Untrusted SID policy configuration.&nbsp;This setting applies when incoming request doesn&#39;t match any Template/Requester map entry.</td>
            <td>
                <ul>
                    <li><strong>*</strong> — any setting</li>
                    <li><strong>PassThrough</strong></li>
                    <li><strong>Pending</strong></li>
                    <li><strong>Suppress</strong></li>
                    <li><strong>Deny</strong></li>
                </ul>
            </td>
        </tr>
        <tr>
            <td>Request Result</td>
            <td>Specifies the request issuance result</td>
            <td>
                <ul>
                    <li><strong>Native</strong> &mdash; native/underlying policy module result (<strong>Issue</strong>, <strong>Pending</strong>).</li>
                    <li><strong>Pending</strong> &mdash; overrides native/underlying policy module result and put request into pending state.</li>
                    <li><strong>Deny</strong> &mdash; overrides native/underlying policy module result and deny the request</li>
                </ul>
            </td>
        </tr>
        <tr>
            <td>SID Extension</td>
            <td>Specifies the SID extension state in resulting certificate</td>
            <td>
                <ul>
                    <li><strong>*</strong> &mdash; does not apply, request is denied.</li>
                    <li><strong>Unchanged</strong> &mdash; SID extension from request (if presented) is passed as is to issued certificate.</li>
                    <li><strong>Overwrite</strong> &mdash; SID extension value from request is discarded and overwritten by this policy module.</li>
                    <li><strong>Disable</strong> &mdash; forcibly disables (removes) SID extension in request.</li>
                </ul>
            </td>
        </tr>
        <tr>
            <td>SAN Extension</td>
            <td>Specifies the SAN extension state in issued certificate</td>
            <td>
                <ul>
                    <li><strong>*</strong> &mdash; does not apply, request is denied.</li>
                    <li><strong>Unchanged</strong> &mdash; SAN extension from request (if presented) is passed as is to issued certificate.</li>
                    <li><strong>Truncate</strong> &mdash; modifies SAN extension in incoming request by removing SID value (URL name type) name. If SID value is the only entry in SAN extension, SAN extension is forcibly disabled.</li>
                </ul>
            </td>
        </tr>
    </tbody>
</table>

# Behaviors
This section provides Policy Module behavior depending on incoming request request conditions and module configuration given that incoming CSR passed global pre-conditions.

## Request does not match any Template/Requester map entry
In this case, **Untrusted SID Policy** setting is enforced as follows:

<table>
	<thead>
		<tr>
			<th>Req has SID Ext</th>
			<th>Req has SID in SAN</th>
			<th>Untrusted SID Policy</th>
			<th>Request Result</th>
			<th>SID Extension</th>
			<th>SAN Extension</th>
		</tr>
	</thead>
	<tbody>
		<tr>
			<td>No</td>
			<td>No</td>
			<td>*</td>
			<td>Native</td>
			<td>Unchanged</td>
			<td>Unchanged</td>
		</tr>
		<tr>
			<td>Yes</td>
			<td>No</td>
			<td>PassThrough</td>
			<td>Native</td>
			<td>PassThrough</td>
			<td>Unchanged</td>
		</tr>
		<tr>
			<td>No</td>
			<td>Yes</td>
			<td>PassThrough</td>
			<td>Native</td>
			<td>Unchanged</td>
			<td>PassThrough</td>
		</tr>
		<tr>
			<td>Yes</td>
			<td>Yes</td>
			<td>PassThrough</td>
			<td>Native</td>
			<td>PassThrough</td>
			<td>PassThrough</td>
		</tr>
		<tr>
			<td>Yes</td>
			<td>No</td>
			<td>Pending</td>
			<td>Pending</td>
			<td>PassThrough</td>
			<td>Unchanged</td>
		</tr>
		<tr>
			<td>No</td>
			<td>Yes</td>
			<td>Pending</td>
			<td>Pending</td>
			<td>Unchanged</td>
			<td>PassThrough</td>
		</tr>
		<tr>
			<td>Yes</td>
			<td>Yes</td>
			<td>Pending</td>
			<td>Pending</td>
			<td>PassThrough</td>
			<td>PassThrough</td>
		</tr>
		<tr>
			<td>Yes</td>
			<td>No</td>
			<td>Suppress</td>
			<td>Native</td>
			<td>Disable</td>
			<td>Unchanged</td>
		</tr>
		<tr>
			<td>No</td>
			<td>Yes</td>
			<td>Suppress</td>
			<td>Native</td>
			<td>Unchanged</td>
			<td>Truncate</td>
		</tr>
		<tr>
			<td>Yes</td>
			<td>Yes</td>
			<td>Suppress</td>
			<td>Native</td>
			<td>Disable</td>
			<td>Truncate</td>
		</tr>
		<tr>
			<td>Yes</td>
			<td>No</td>
			<td>Deny</td>
			<td>Deny</td>
			<td>*</td>
			<td>*</td>
		</tr>
		<tr>
			<td>No</td>
			<td>Yes</td>
			<td>Deny</td>
			<td>Deny</td>
			<td>*</td>
			<td>*</td>
		</tr>
		<tr>
			<td>Yes</td>
			<td>Yes</td>
			<td>Deny</td>
			<td>Deny</td>
			<td>*</td>
			<td>*</td>
		</tr>
	</tbody>
</table>

## Request matches at least one entry in Template/Requester map table and target principal was not resolved
SID Policy Module attempts to find target identity from the incoming request's SAN extension. If template subject is user, then at least one UPN entry in SAN extension is expected. If template subject is machine (computer), then at least one entry of type of `dnsName` name type is expected. In both cases, only first occurrence of matching name type is evaluated.

The following table outlines the SID Policy Module behavior when target identity was not resolved for whatever reason. The behavior is identical to a table in previous section with the only difference that **Trusted SID Policy** configuration is evaluated.
<table>
	<thead>
		<tr>
			<th>CSR has SID Ext</th>
			<th>CSR has SID in SAN</th>
			<th>Trusted SID Policy</th>
			<th>Request Result</th>
			<th>SID Extension</th>
			<th>SAN Extension</th>
		</tr>
	</thead>
	<tbody>
		<tr>
			<td>No</td>
			<td>No</td>
			<td>*</td>
			<td>Native</td>
			<td>Unchanged</td>
			<td>Unchanged</td>
		</tr>
		<tr>
			<td>Yes</td>
			<td>No</td>
			<td>PassThrough</td>
			<td>Native</td>
			<td>PassThrough</td>
			<td>Unchanged</td>
		</tr>
		<tr>
			<td>No</td>
			<td>Yes</td>
			<td>PassThrough</td>
			<td>Native</td>
			<td>Unchanged</td>
			<td>PassThrough</td>
		</tr>
		<tr>
			<td>Yes</td>
			<td>Yes</td>
			<td>PassThrough</td>
			<td>Native</td>
			<td>PassThrough</td>
			<td>PassThrough</td>
		</tr>
		<tr>
			<td>Yes</td>
			<td>No</td>
			<td>Pending</td>
			<td>Pending</td>
			<td>PassThrough</td>
			<td>Unchanged</td>
		</tr>
		<tr>
			<td>No</td>
			<td>Yes</td>
			<td>Pending</td>
			<td>Pending</td>
			<td>Unchanged</td>
			<td>PassThrough</td>
		</tr>
		<tr>
			<td>Yes</td>
			<td>Yes</td>
			<td>Pending</td>
			<td>Pending</td>
			<td>PassThrough</td>
			<td>PassThrough</td>
		</tr>
		<tr>
			<td>Yes</td>
			<td>No</td>
			<td>Suppress</td>
			<td>Native</td>
			<td>Disable</td>
			<td>Unchanged</td>
		</tr>
		<tr>
			<td>No</td>
			<td>Yes</td>
			<td>Suppress</td>
			<td>Native</td>
			<td>Unchanged</td>
			<td>Truncate</td>
		</tr>
		<tr>
			<td>Yes</td>
			<td>Yes</td>
			<td>Suppress</td>
			<td>Native</td>
			<td>Disable</td>
			<td>Truncate</td>
		</tr>
		<tr>
			<td>Yes</td>
			<td>No</td>
			<td>Deny</td>
			<td>Deny</td>
			<td>*</td>
			<td>*</td>
		</tr>
		<tr>
			<td>No</td>
			<td>Yes</td>
			<td>Deny</td>
			<td>Deny</td>
			<td>*</td>
			<td>*</td>
		</tr>
		<tr>
			<td>Yes</td>
			<td>Yes</td>
			<td>Deny</td>
			<td>Deny</td>
			<td>*</td>
			<td>*</td>
		</tr>
	</tbody>
</table>

## Request matches at least one entry in Template/Requester map table and target principal was successfully resolved
This section outlines SID Policy Module behavior when all conditions are successfully passed and target identity. In this case, SID extension value in request is discarded and overwritten by a SID value retrieved from Active Directory.
<table>
	<thead>
		<tr>
			<th>CSR has SID in SAN</th>
			<th>Request Result</th>
			<th>SID Extension</th>
			<th>SAN Extension</th>
		</tr>
	</thead>
	<tbody>
		<tr>
			<td>No</td>
			<td>Native</td>
			<td>Overwrite</td>
			<td>Unchanged</td>
		</tr>
		<tr>
			<td>Yes</td>
			<td>Native</td>
			<td>Overwrite</td>
			<td>Truncate</td>
		</tr>
	</tbody>
</table>
