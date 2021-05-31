Function Deploy-Application {
    Param (
        [Parameter(Mandatory)][string]$AppID,
        [Parameter(Mandatory)][ValidateSet("Install","Uninstall")][string]$Action
    )

    $app = Get-WmiObject -Query "SELECT IsMachineTarget, Revision FROM CCM_Application WHERE Id=`"$AppID`"" -Namespace 'root\ccm\clientsdk'

    if ($app) {
        $args = @{ 
            EnforcePreference = [UINT32]0
            Id = $AppID
            IsMachineTarget = $app.IsMachineTarget
            IsRebootIfNeeded = $False
            Priority = 'High'
            Revision = "$($app.Revision)"
        }

        Invoke-CimMethod -Namespace "root\ccm\clientSDK" -ClassName CCM_Application -MethodName $Action -Arguments $args
        Write-Information "**$Action action initiated for application. Please refresh the view to check on application status"
    } else {
        Write-Error "Unable to find application $AppID"
    }
}
