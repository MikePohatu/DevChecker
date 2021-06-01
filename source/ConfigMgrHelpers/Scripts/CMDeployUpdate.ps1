Function Install-Update {
    Param (
        [Parameter(Mandatory)][string]$KB
    )

    $InstallStates = @{
        0="None"
        1="Available"
        2="Submitted"
        3="Detecting"
        4="PreDownload"
        5="Downloading"
        6="WaitInstall"
        7="Installing"
    }
    
    $update = Get-WmiObject -Namespace 'root\ccm\clientsdk' -Query "SELECT * FROM CCM_SoftwareUpdate"
 
    if ($update) {
        Invoke-WmiMethod  -Namespace 'root\ccm\clientsdk' -Class CCM_SoftwareUpdatesManager -Name InstallUpdates -ArgumentList (,$update)
        Write-Information "**Install action initiated for update. Please refresh the view to check on application status"
    } else {
        Write-Error "Unable to find update $KB"
    }
}
