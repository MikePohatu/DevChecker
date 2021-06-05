
<#
    Original source: https://gist.github.com/Robert-LTH/7423e418aab033d114d7c8a2df99246b
#>

function Invoke-SCCMRunScript {
    param(
        [Parameter(Mandatory=$true)][ValidateNotNullOrEmpty()][string]$SiteServer,
        [Parameter(Mandatory=$true)][ValidateNotNullOrEmpty()][string]$Namespace,
        [Parameter(Mandatory=$true)][ValidateNotNullOrEmpty()][string]$ScriptGuid,
        [Array]$InputParameters = @(),
        [parameter(Mandatory = $true, ParameterSetName = 'ByResourceID')][Array]$TargetResourceIDs = @()
    )

    # if something goes wrong, we want to stop!
    $ErrorActionPreference = "Stop"

    # Get the script
    $Script = [wmi](Get-WmiObject -class SMS_Scripts -Namespace $Namespace -ComputerName $SiteServer -Filter "ScriptGuid = '$ScriptGuid'").__PATH

    if (-not $Script) {
        throw "Could not find script with GUID $ScriptGuid"
    }
    # Parse the parameter definition
    $Parameters = [xml]([string]::new([Convert]::FromBase64String($Script.ParamsDefinition)))

    $Parameters.ScriptParameters.ChildNodes | % {
        # In the case of a missing required parameter, bail!
        if ($_.IsRequired -and $InputParameters.Count -lt 1) {
            throw "Script has required parameters but no parameters was passed. Script GUID $ScriptGuid"
        }

        if ($_.Name -notin $InputParameters.Name) {
            throw "Parameter '$($_.Name)' has not been passed in InputParamters!"
        }
    }

    # GUID used for parametergroup
    $ParameterGroupGUID = $(New-Guid)

    if ($InputParameters.Count -le 0) {
        # If no ScriptParameters: <ScriptParameters></ScriptParameters> and an empty hash
        $ParametersXML = "<ScriptParameters></ScriptParameters>"
        $ParametersHash = ""
    }
    else {
        foreach ($Parameter in $InputParameters) {
            $InnerParametersXML = "$InnerParametersXML<ScriptParameter ParameterGroupGuid=`"$ParameterGroupGUID`" ParameterGroupName=`"PG_$ParameterGroupGUID`" ParameterName=`"$($Parameter.Name)`" ParameterType=`"$($Parameter.Type)`" ParameterValue=`"$($Parameter.Value)`"/>"
        }
        $ParametersXML = "<ScriptParameters>$InnerParametersXML</ScriptParameters>"

        $SHA256 = [System.Security.Cryptography.SHA256Cng]::new()
        $Bytes = ($SHA256.ComputeHash(([System.Text.Encoding]::Unicode).GetBytes($ParametersXML)))
        $ParametersHash = ($Bytes | ForEach-Object ToString X2) -join ''
    }

    $RunScriptXMLDefinition = "<ScriptContent ScriptGuid='{0}'><ScriptVersion>{1}</ScriptVersion><ScriptType>{2}</ScriptType><ScriptHash ScriptHashAlg='SHA256'>{3}</ScriptHash>{4}<ParameterGroupHash ParameterHashAlg='SHA256'>{5}</ParameterGroupHash></ScriptContent>"
    $RunScriptXML = $RunScriptXMLDefinition -f $Script.ScriptGuid,$Script.ScriptVersion,$Script.ScriptType,$Script.ScriptHash,$ParametersXML,$ParametersHash
    
    # Get information about the class instead of fetching an instance
    # WMI holds the secret of what parameters that needs to be passed and the actual order in which they have to be passed
    $MC = [WmiClass]"\\$SiteServer\$($Namespace):SMS_ClientOperation"
    
    # Get the parameters of the WmiMethod
    $MethodName = 'InitiateClientOperationEx'
    $InParams = $MC.psbase.GetMethodParameters($MethodName)
    
    # Information about the script is passed as the parameter 'Param' as a BASE64 encoded string
    $InParams.Param = ([Convert]::ToBase64String(([System.Text.Encoding]::UTF8).GetBytes($RunScriptXML)))
    
    # Hardcoded to 0 in certain DLLs
    $InParams.RandomizationWindow = "0"
    
    # If we are using a collection, set it. TargetCollectionID can be empty string: ""
    $InParams.TargetCollectionID = $TargetCollectionID
    
    # If we have a list of resources to run the script on, set it. TargetResourceIDs can be an empty array: @()
    # Criteria for a "valid" resource is IsClient=$true and IsBlocked=$false and IsObsolete=$false and ClientType=1
    $InParams.TargetResourceIDs = $TargetResourceIDs
    
    # Run Script is type 135
    $InParams.Type = "135"
    
    # Everything should be ready for processing, invoke the method!
    $R = $MC.InvokeMethod($MethodName, $InParams, $null)
    
    # The result contains the client operation id of the execution
    $R
}
