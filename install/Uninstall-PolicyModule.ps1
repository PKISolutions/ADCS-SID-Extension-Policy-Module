[CmdletBinding()]
    param(
        [switch]$Restart
    )
$regTemplate = @'
Windows Registry Editor Version 5.00

[-HKEY_CLASSES_ROOT\PKISolutions_SID.Policy]
[-HKEY_CLASSES_ROOT\CLSID\{{4335DB31-EDC5-4277-B1EE-25B88A05192C}}]

[-HKEY_CLASSES_ROOT\PKISolutions_SID.PolicyManage]
[-HKEY_CLASSES_ROOT\CLSID\{{6369E566-25A1-456C-8CD8-C00D07E59B99}}]
'@

# de-register COM components
$tempFile = [System.IO.Path]::GetTempFileName()
Set-Content -Path $tempFile -Value $regTemplate
reg import $tempFile | Out-Null
Remove-Item $tempFile -Force

# roll back active CA node
$regPolTemplate = "HKLM:\System\CurrentControlSet\Services\CertSvc\Configuration"
$Active = (Get-ItemProperty $regPolTemplate -Name Active).Active
$CurrentPolicyModuleRegPath= $regTemplate + "\$Active\PolicyModules\PKISolutions_SID.Policy"
if (!(Test-Path $CurrentPolicyModuleRegPath)) {
    return
}
$nativeProgID = Get-ItemProperty -Path $CurrentPolicyModuleRegPath -Name "NativeProgID" -ErrorAction SilentlyContinue
if (!$nativeProgID) {
    $nativeProgID = "CertificateAuthority_MicrosoftDefault.Policy"
}
Set-ItemProperty -Path ($regTemplate + "\$Active\PolicyModules") -Name "Active" -Value $nativeProgID
Remove-Item $CurrentPolicyModuleRegPath -Force
if ($Restart) {
    Restart-Service CertSvc
}