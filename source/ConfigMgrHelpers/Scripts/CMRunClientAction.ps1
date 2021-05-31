
Function Run-CMAction {
    Param (
	    [Parameter(Mandatory)]
	    [ValidateSet('{00000000-0000-0000-0000-000000000021}','{00000000-0000-0000-0000-000000000003}','{00000000-0000-0000-0000-000000000071}',
        '{00000000-0000-0000-0000-000000000121}','{00000000-0000-0000-0000-000000000001}','{00000000-0000-0000-0000-000000000108}',
        '{00000000-0000-0000-0000-000000000113}','{00000000-0000-0000-0000-000000000002}' )][string]$ClientAction
	    )

    Try {
        Write-Information "Processing on client"
	    Invoke-WMIMethod -Namespace root\ccm -Class SMS_CLIENT -Name TriggerSchedule $ClientAction -ErrorAction Stop
        Write-Information "Done"
    } catch {
        Write-Error "Error running action: $($_.Exception.Message)"
    }
}
