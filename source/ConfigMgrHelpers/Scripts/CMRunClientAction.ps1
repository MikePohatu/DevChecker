# https://rid500.wordpress.com/2017/07/23/sccm-refresh-machine-policy-retrieval-evaluation-cycle-via-wmi/
# https://systemcenterdudes.com/configuration-manager-2012-client-command-list/
Function Run-CMAction {
    Param (
	    [Parameter(Mandatory)][string]$ClientAction
	    )

    Try {
        Write-Information "Processing on client"
	    Invoke-WMIMethod -Namespace root\ccm -Class SMS_CLIENT -Name TriggerSchedule $ClientAction -ErrorAction Stop
        Write-Information "Done"
    } catch {
        Write-Error "Error running action: $($_.Exception.Message)"
    }
}
