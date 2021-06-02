$ccmrepairpath="$($env:WinDir)\ccm\ccmrepair.exe"

if (Test-Path $ccmrepairpath) {
    Start-Process $ccmrepairpath
    Write-Information "CCMRepair initiated"
} else {
    Write-Warning "Repair tool not found: $ccmrepairpath"
}



<#ActionSettings
{
    "DisplayName": "Repair ConfigMgr agent",
    "OutputType": "None",
    "Description": "Run the ConfigMgr repair tool",
    "RunOnConnect": false,
    "LogScriptContent": false
}
ActionSettings#>