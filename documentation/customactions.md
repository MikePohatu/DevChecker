# Custom Actions

## Adding a Custom Action
Custom actions are simply PowerShell scripts that will be run on the remote device. 

To add a custom action, copy your custom script file to the **Scripts\Custom** folder. If DevChecker is already running, you can click *Refresh* on the Client Side\Actions\Custom Actions tab.

## Metadata

Additional metadata can be added to your script to control how your custom action is presented in the DevChecker UI.

To add metadata, copy and paste the following somewhere in your PowerShell script. This is a comment block so will not effect the functionality of your script. 

```json
<#ActionSettings
{
    "DisplayName": "Get Applications",
    "OutputType": "None",
    "Description": "List installed applications recorded in registry",
    "RunOnConnect": false,
    "LogScriptContent": false
}
ActionSettings#>
```

Update the fields appropriately:
* DisplayName: The name to be shown in DevChecker. If this is empty the script name will be used
* OutputType: *Not currently implemented. This will be used to change the output to a table view if desired
* Descripttion: The description to appear in DevChecker
* RunOnConnect: *Not currently implemented. This will be used to set whether to automatically run the script on connect to the device
* LogScriptContent: Set this to true to output the content of the script file to the logging pane in DevChecker

The json in the comment block will be parsed when the script is read to create the appropriate configuration.