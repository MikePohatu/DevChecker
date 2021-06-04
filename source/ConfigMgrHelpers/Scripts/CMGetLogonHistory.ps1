 $logins = @()

Get-WmiObject –Namespace ROOT\CCM –Class CCM_UserLogonEvents | select -First 50 | Sort-Object -Property LogonTime -Descending | ForEach-Object {
    $Username = (New-Object System.Security.Principal.SecurityIdentifier($_.UserSID)).Translate( [System.Security.Principal.NTAccount]).Value
    $LogonTime = ([datetime]'1/1/1970').AddSeconds($_.LogonTime).ToString("yyyy/MM/dd-HH:mm:ss")
    $LogoffTime = ""
    $SessionLength = ""
    if ($_.LogoffTime) { 
        $LogoffTime = ([datetime]'1/1/1970').AddSeconds($_.LogoffTime).ToString("yyyy/MM/dd-HH:mm:ss") 
        $SessionLength = ([timespan]::fromseconds($_.LogoffTime - $_.LogonTime)).ToString() 
        }

    $logins += (
        [PSCustomObject]@{
        Username = $Username
        LogonTime = $LogonTime
        LogoffTime = $LogoffTime
        SessionLength = $SessionLength
        })
}

$logins
