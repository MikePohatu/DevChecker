
function Restart {
    Param (
        [Parameter(Mandatory)][string]$ServiceName
    )

    $service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue

    if ($service) {
        if ($service.Status -eq 'Running') {
            Write-Information "Stopping service $ServiceName"
            $service.Stop()
            while ($service.Status -ne 'Stopped')
            {
                Start-Sleep -seconds 5
                $service.Refresh()
            }
        }

        Write-Information "Starting service $ServiceName"
        $service.Start()
        while ($service.Status -ne 'Running')
        {
            Start-Sleep -seconds 5
            $service.Refresh()
        }

        Write-Information "Done"
    } else {
        Write.Error "Service not found: $ServiceName"
    }
}