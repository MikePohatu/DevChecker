$Updates = Get-Hotfix | Select-Object -Property KBArticle, InstalledOn, HotFixID, InstalledBy | Sort-Object -Property InstalledOn -Descending

[string]$output = "Updates found on $($env:ComputerName): `nInstalledOn, KBArticle, HotFixID, InstalledBy`n"
$Updates | Sort-Object -Property Vendor | Foreach-Object { $output += "$($_.InstalledOn), $($_.KBArticle), $($_.HotFixID), $($_.InstalledBy)`n" }


Write-Information $output
Write-Information "Done"

<#ActionSettings
{
    DisplayName: "Get Patches",
    OutputType: "None",
    Description: "List installed updates",
    RunOnConnect: false
}
ActionSettings#>