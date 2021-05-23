

$existkeys = @(
    "HKLM:\Software\Microsoft\Windows\CurrentVersion\Component Based Servicing\RebootPending",
    "HKLM:\Software\Microsoft\Windows\CurrentVersion\Component Based Servicing\RebootInProgress",
    "HKLM:\Software\Microsoft\Windows\CurrentVersion\Component Based Servicing\PackagesPending",
    "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\PostRebootReporting",
    "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\RebootRequired",
    "HKLM:\SOFTWARE\Microsoft\ServerManager\CurrentRebootAttempts"
)

$existkeys | ForEach-Object {
    
    write-host "Key: $_"
    if (Test-Path $_) { 
        $true 
        exit
    }
}

$existvalues = @( @("HKLM:\SYSTEM\CurrentControlSet\Control\Session Manager", "PendingFileRenameOperations"),
    @("HKLM:\SYSTEM\CurrentControlSet\Control\Session Manager", "PendingFileRenameOperations2"),
    @("HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce", "DVDRebootSignal"),
    @("HKLM:\SYSTEM\CurrentControlSet\Services\Netlogon", "JoinDomain"),
    @("HKLM:\SYSTEM\CurrentControlSet\Services\Netlogon", "AvoidSpnSet")
    )

$existvalues | ForEach-Object {
    $Key = $_[0]
    $Value = $_[1]

    write-host "Key: $Key"
    write-host "Value: $Value"

    try {
        Get-ItemProperty -Path $Key | Select-Object -ExpandProperty $Value -ErrorAction Stop | Out-Null
        $true
        exit
    }
    catch { }
}