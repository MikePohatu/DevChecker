  Function Get-ServerSideInfo {
    Param (
        [Parameter(Mandatory=$true)][string]$ComputerName,
        [Parameter(Mandatory=$true)][string]$NameSpace,
        [Parameter(Mandatory=$true)][string]$Server
    )
    $sys = Get-WmiObject -Query "SELECT * FROM SMS_R_System where Name='$ComputerName'" -Namespace $NameSpace -ComputerName $Server

    $ipAddresses = ""
    if ($sys.IPAddresses) { $ipAddresses = [string]::Join(", ", $sys.IPAddresses) }
    $macAddresses = ""
    if ($sys.MacAddresses) { $macAddresses = [string]::Join(", ", $sys.MacAddresses) }

    $ou = ""
    if ($sys.SystemOUName) { $ou = $sys.SystemOUName[-1] }

    $serverSieInfo = @{
        
        ADSiteName = $sys.ADSiteName
        Build = $sys.Build
        BuildExt = $sys.BuildExt
        ClientVersion = $sys.ClientVersion
        IsVirtualMachine = $sys.IsVirtualMachine
        IPAddresses = $ipAddresses
        LastLogonTimestamp = $sys.LastLogonTimestamp
        LastLogonUserName = $sys.LastLogonUserName
        MacAddresses = $macAddresses
        OU = $ou
        DomainORWorkgroup = $sys.ResourceDomainORWorkgroup
        ResourceId = $sys.ResourceId
        VMostName = $sys.VirtualMachineHostName
    }

    return $serverSieInfo
} 
 
