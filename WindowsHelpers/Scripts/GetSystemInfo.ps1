
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

Function Get-PowerInfo {
    $batt = Get-WmiObject -Query 'SELECT * FROM Win32_Battery'
    #BatteryStatus -eq means power is plugged in. Null means no battery
    if ($batt.BatteryStatus) {
        $charge = "$($batt.EstimatedChargeRemaining)%"
        if ($batt.Status -eq $null) {
            $status = "Unknown"
        } else {
            $status = $batt.Status
        }

        if ($batt.BatteryStatus -eq 2) {
            $connected = $true
        }
        else {
            $connected = $false
        }
    }
    else {
        $charge = "N/A"
        $status = "N/A"
        $connected = $true
    }

    return @{
        Connected = $connected
        Status = $status
        Charge = $charge
    }
}

function Get-ProductType {
    $product = (Get-WmiObject -Query 'SELECT ProductType FROM Win32_OperatingSystem').ProductType
    if ($product -eq 1) { return "Client" }
    elseif ($product -eq 2) { return "Domain Controller" }
    elseif ($product -eq 3) { return "Server" }
}

Function Get-LoggedOnUsers {
    $users = ((quser) -replace '^>', '' -replace '\s{20}', ',' -replace '\s{2,}', ',') | ConvertFrom-Csv

    $active = $users | Where {$_.STATE -eq 'Active'}
    $disconnected = $users | Where {$_.STATE -eq 'Disc'}
    $consoleuser = $users | Where {$_.SESSIONNAME -eq 'Console'}

    $activeUsersString = [string]::Empty
    $disconnectedUsersString = [string]::Empty
    $consoleUserString = [string]::Empty

    if ($active) {
        $activeUsersString =  [string]::Join(', ', $active.USERNAME)
    }

    if ($disconnected) {
        $disconnectedUsersString =  [string]::Join(', ', $disconnected.USERNAME)
    }

    if ($consoleuser) {
        $consoleUserString = $consoleuser.USERNAME
    }

    return @{
        activeUsers = $activeUsersString
        disconnectedUsers = $disconnectedUsersString
        consoleUser = $consoleUserString
    }
}

$ipv4s = Get-NetIPAddress -AddressFamily IPv4 | Where-Object {$_.InterfaceIndex -ne 1} | Select IPAddress
$ipv6s = Get-NetIPAddress -AddressFamily IPv6 | Where-Object {$_.InterfaceIndex -ne 1} | Select IPAddress
$compSys = Get-WmiObject -Query 'SELECT * FROM Win32_ComputerSystem' | Select Manufacturer, Model, SystemType, TotalPhysicalMemory
$compOS = Get-WmiObject -Query 'SELECT * FROM Win32_OperatingSystem' | Select BuildNumber, Caption, LastBootUpTime, OSArchitecture, Version, WindowsDirectory
$compBIOS = Get-WmiObject -Query 'SELECT * FROM Win32_BIOS' | Select SerialNumber, SMBIOSBIOSVersion
$users = Get-LoggedOnUsers
$power = Get-PowerInfo

$systemInfo = @{
    pendingReboot = IsRebootPending
    type = $compOS.OSArchitecture
    memorySize = $compSys.TotalPhysicalMemory
    ipv4Addresses = [string]::Join(", ", $ipv4s.IPAddress)
    ipv6Addresses = [string]::Join(", ", $ipv6s.IPAddress)
    consoleUser = $users.consoleUser
    activeUsers = $users.activeUsers
    disconnectedUsers = $users.disconnectedUsers
    model = $compSys.Model
    biosVersion = $compBIOS.SMBIOSBIOSVersion
    serial = $compBIOS.SerialNumber
    manufacturer = $compSys.Manufacturer
    architecture = $compOS.OSArchitecture
    version = $compOS.Version
    build = $compOS.BuildNumber
    lastBoot = ([System.Management.ManagementDateTimeConverter]::ToDateTime($compOS.LastBootUpTime).ToUniversalTime()).ToString("dd-MMM-yyyy HH:mm:ss")
    os = $compOS.Caption
    name = $env:COMPUTERNAME
    configMgrClientStatus = Get-ConfigMgrClientStatus
    batteryCharge = $power.Charge
    batteryStatus = $power.Status
    powerConnected = $power.Connected
}

$systemInfo