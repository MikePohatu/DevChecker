$apps = get-wmiobject -query "SELECT * FROM CCM_Application" -namespace "ROOT\ccm\ClientSDK" | Select Name, Id, InstallState, ResolvedState, Revision
$updates = get-wmiobject -query "SELECT * FROM CCM_SoftwareUpdate" -namespace "ROOT\ccm\ClientSDK" | Select Name, ArticleID, BulletinID, MaxExecutionTime, URL
$tasksequences = get-wmiobject -query "SELECT * FROM CCM_Program WHERE TaskSequence='True'" -namespace "ROOT\ccm\ClientSDK" | Select Name, PackageID, HighImpactTaskSequence

$softwareCenter = @{
    applications = $apps
    updates = $updates
    taskSequences = $tasksequences
}

$softwareCenter 
