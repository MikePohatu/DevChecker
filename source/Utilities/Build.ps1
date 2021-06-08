Function SignAssembliesInPath {
    Param(
        [Parameter(Mandatory=$true)][string]$PackagePath,
        [string]$SignPath='C:\Program Files (x86)\Windows Kits\10\bin\10.0.19041.0\x64',
        [string]$Description
    )

    $assemblies = Get-ChildItem $PackagePath -Recurse -Include ('*.exe', '*.dll') | Foreach-Object {
        $signature = Get-AuthenticodeSignature $_ 
        if ($signature.Status -ne "Valid") { $_.FullName }
    }


    if ($assemblies) {
        $signer = "$SignPath\signtool.exe"
        [Array]$signparams = "sign","/v","/n","20Road Limited","/d",$Description,"/du","https://20road.com","/tr","http://ts.ssl.com","/td","SHA256","/fd","SHA256"
        $signparams += $assemblies

        & $signer $signparams
    }
}

Function VerifyAssembliesInPath {
    Param(
        [Parameter(Mandatory=$true)][string]$PackagePath
    )

    Get-ChildItem $PackagePath -Recurse -Include ('*.exe', '*.dll') | Foreach-Object {
        $signature = Get-AuthenticodeSignature $_ 
        if ($signature.Status -eq "Valid") { Write-Information "Valid: $($_.FullName)" }
        else { Write-Error -Message "Issue: $($_.FullName)"}

        $outobj = New-Object -TypeName psobject -Property @{
            'FilePath'= $_.FullName 
            'Status' = $signature.Status }

        write-output $outobj
    }
}

Function PackageFolder {
    Param(
        [Parameter(Mandatory=$true)][string]$PackageFolder,
        [Parameter(Mandatory=$true)][string]$DestinationPath
    )

    add-type -AssemblyName System.IO.Compression.FileSystem
    [System.IO.Compression.ZipFile]::CreateFromDirectory("$PackageFolder",$DestinationPath,'Optimal',$false)
}

function Set-ProjectVersion {
    Param (
        [Parameter(Mandatory=$true)][string]$ProjectPath,
        [Parameter(Mandatory=$true)][string]$Version
    )

    #https://stackoverflow.com/q/57666790
    $assemblyInfoPath = "$($ProjectPath)\Properties\AssemblyInfo.cs"

    Write-Host "Updating $assemblyInfoPath to $Version"

    $assemblyInfoText = (Get-Content -Path $assemblyInfoPath -Encoding UTF8 -ReadCount 0)
    $assemblyInfoText = $assemblyInfoText -replace '\[assembly: AssemblyVersion\("((\d)+|(\.))*"\)\]', "[assembly: AssemblyVersion(`"$Version`")]"
    $assemblyInfoText -replace '\[assembly: AssemblyFileVersion\("((\d)+|(\.))*"\)\]', "[assembly: AssemblyFileVersion(`"$Version`")]" | Set-Content -Path $assemblyInfoPath -Encoding UTF8
}

Function pause ()
{
    Param(
        [string]$message = 'Press any key to continue...'
    )
    # Check if running Powershell ISE
    if ($psISE)
    {
        Add-Type -AssemblyName System.Windows.Forms
        [System.Windows.Forms.MessageBox]::Show("$message")
    }
    else
    {
        Write-Host "$message" -ForegroundColor Yellow
        $x = $host.ui.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    }
}

#run builds
$version = '0.9.2.0'
$repoRoot = 'C:\Source\repos\DevChecker'
$devenv = 'C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\devenv.exe'
$dotnet = 'dotnet.exe'
$cmd = "cmd"
$productName = "DevChecker"
$ReleaseRootPath='C:\Source\release'
$PackagePath = "$($ReleaseRootPath)\$($productName)_$($version).zip"
$ProductReleasePath="$($ReleaseRootPath)\devchecker"
$BuildFile = "$($repoRoot)\source\DevChecker\DevChecker.csproj"

Write-Host "Updating $productName versions to $version"
Set-ProjectVersion -ProjectPath "$($repoRoot)\source\ConfigMgrHelpers" -Version $version
Set-ProjectVersion -ProjectPath "$($repoRoot)\source\Core" -Version $version
Set-ProjectVersion -ProjectPath "$($repoRoot)\source\CustomActions" -Version $version
Set-ProjectVersion -ProjectPath "$($repoRoot)\source\DevChecker" -Version $version
Set-ProjectVersion -ProjectPath "$($repoRoot)\source\WindowsHelpers" -Version $version




Write-Host "Building $BuildSln"
if (!(Test-Path -Path $ProductReleasePath)) { md $ProductReleasePath }
Start-Process -FilePath $cmd -ArgumentList "/c `"`"$devenv`" `"$BuildFile`" /rebuild Release`""

Pause

Write-Host "Signing assemblies"
SignAssembliesInPath -PackagePath $ProductReleasePath -Description $productName

$appstatus = VerifyAssembliesInPath -PackagePath $ProductReleasePath

$allstatus = @()
$allstatus += $appstatus

$errorfound = $false

$allstatus | ForEach-Object { 
    if ($_.Status -ne "Valid") { 
        $errorfound = $true
        break
    }
}

if (Test-Path -Path $PackagePath) { Remove-Item -Path $PackagePath }

PackageFolder -PackageFolder $ProductReleasePath -DestinationPath $PackagePath
