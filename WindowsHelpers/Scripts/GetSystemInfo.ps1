
Function IsRebootPending {
    $existkeys = @(
        "HKLM:\Software\Microsoft\Windows\CurrentVersion\Component Based Servicing\RebootPending",
        "HKLM:\Software\Microsoft\Windows\CurrentVersion\Component Based Servicing\RebootInProgress",
        "HKLM:\Software\Microsoft\Windows\CurrentVersion\Component Based Servicing\PackagesPending",
        "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\PostRebootReporting",
        "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\RebootRequired",
        "HKLM:\SOFTWARE\Microsoft\ServerManager\CurrentRebootAttempts"
    )

    ForEach ($key in $existkeys) {
    
        #write-host "Key: $_"
        if (Test-Path $key) { 
            Write-Information "Reboot pending, key: $key"
            return $true
        }
    }

    $existvalues = @( @("HKLM:\SYSTEM\CurrentControlSet\Control\Session Manager", "PendingFileRenameOperations"),
        @("HKLM:\SYSTEM\CurrentControlSet\Control\Session Manager", "PendingFileRenameOperations2"),
        @("HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce", "DVDRebootSignal"),
        @("HKLM:\SYSTEM\CurrentControlSet\Services\Netlogon", "JoinDomain"),
        @("HKLM:\SYSTEM\CurrentControlSet\Services\Netlogon", "AvoidSpnSet")
        )

    ForEach ($keyval in $existvalues) {
        $Key = $keyval[0]
        $Value = $keyval[1]

        #write-host "Key: $Key"
        #write-host "Value: $Value"

        try {
            Get-ItemProperty -Path $Key | Select-Object -ExpandProperty $Value -ErrorAction Stop | Out-Null
            Write-Information "Reboot pending, key: $Key\$Value"
            return $true
        }
        catch { }
    }

    return $false
}

Function Get-MemorySize {
    $memsize = 0
    Get-WmiObject -Query 'SELECT Capacity FROM Win32_PhysicalMemory' | ForEach-Object {
        $memsize += $_.Capacity
    }
    return $memsize
}

function Get-ConfigMgrClientStatus {
    $client =  Get-WmiObject -NameSpace Root\CCM -Class Sms_Client -Property ClientVersion -ErrorAction SilentlyContinue
    if ($client) {
        $service = Get-Service -Name 'ccmexec' -ErrorAction SilentlyContinue
        if ($service) {
            return $service.Status.ToString()
        } else {
            return 'ServiceError'
        }
    }
    else {
        return 'NotInstalled'
    }
}

function Get-ProductType {
    $product = (Get-WmiObject -Query 'SELECT ProductType FROM Win32_OperatingSystem').ProductType
    if ($product -eq 1) { return "Client" }
    elseif ($product -eq 2) { return "Domain Controller" }
    elseif ($product -eq 3) { return "Server" }
}

function Get-CompInfo {
    return Get-ComputerInfo -Property WindowsProductName, WindowsEditionId, WindowsInstallationType, 
        OsVersion, OsBuildNumber, OsArchitecture, OsUptime, OsLastBootUpTime, 
        BiosName, BiosFirmwareType, BiosSeralNumber, CsModel, CsManufacturer
}

$ipv4s = Get-NetIPAddress -AddressFamily IPv4 | Where-Object {$_.InterfaceIndex -ne 1} | Select IPAddress
$ipv6s = Get-NetIPAddress -AddressFamily IPv6 | Where-Object {$_.InterfaceIndex -ne 1} | Select IPAddress
$compInfo = Get-CompInfo
$systemInfo = @{
    pendingReboot = IsRebootPending
    type = $compInfo.WindowsInstallationType
    memorySize = Get-MemorySize
    ipv4Addresses = [string]::Join(", ", $ipv4s.IPAddress)
    ipv6Addresses = [string]::Join(", ", $ipv6s.IPAddress)
    model = $compInfo.CsModel
    serial = $compInfo.BiosSeralNumber
    manufacturer = $compInfo.CsManufacturer
    architecture = $compInfo.OsArchitecture
    version = $compInfo.OsVersion
    build = $compInfo.OsBuildNumber
    edition = $compInfo.WindowsEditionId
    uptime = $compInfo.OsUptime
    lastBoot = $compInfo.OsLastBootUpTime
    os = $compInfo.WindowsProductName
    name = $env:COMPUTERNAME
    configMgrClientStatus = Get-ConfigMgrClientStatus
}

$systemInfo