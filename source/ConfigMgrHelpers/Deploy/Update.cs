#region license
// Copyright (c) 2021 20Road Limited
//
// This file is part of DevChecker.
//
// DevChecker is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
#endregion

using Core.Logging;
using System.Management.Automation;
using System.Threading.Tasks;
using WindowsHelpers;

namespace ConfigMgrHelpers.Deploy
{
    public class Update
    {
        public static string GetterCommand { get; }= @"get-wmiobject -query 'SELECT * FROM CCM_SoftwareUpdate' -namespace 'ROOT\ccm\ClientSDK'";

        public string Name { get; set; }
        public string ArticleID { get; set; }
        public string BulletinID { get; set; }
        public string Description { get; set; }
        public int MaxExecutionTime { get; set; }
        public string URL { get; set; }
        public bool IsUpgrade { get; set; }
        public bool IsO365Update { get; set; }

        public static Update New(PSObject poshObj)
        {
            var cmobj = new Update();
            cmobj.Name = PoshHandler.GetPropertyValue<string>(poshObj, "Name");
            cmobj.ArticleID = PoshHandler.GetPropertyValue<string>(poshObj, "ArticleID");
            cmobj.BulletinID = PoshHandler.GetPropertyValue<string>(poshObj, "BulletinID");
            cmobj.Description = PoshHandler.GetPropertyValue<string>(poshObj, "Description");
            cmobj.MaxExecutionTime = PoshHandler.GetPropertyValue<int>(poshObj, "MaxExecutionTime");
            cmobj.URL = PoshHandler.GetPropertyValue<string>(poshObj, "URL");
            cmobj.IsUpgrade = PoshHandler.GetPropertyValue<bool>(poshObj, "MaxExecutIsUpgradeionTime");
            cmobj.IsO365Update = PoshHandler.GetPropertyValue<bool>(poshObj, "IsO365Update");
            return cmobj;
        }

        public async Task InstallAsync()
        {
            if (string.IsNullOrWhiteSpace(this.ArticleID) == false)
            {
                //StringBuilder builder = new StringBuilder();
                //string scriptPath = AppDomain.CurrentDomain.BaseDirectory + "Scripts\\CMInstallApplication.ps1";
                //string script = await IOHelpers.ReadFileAsync(scriptPath);
                //builder.AppendLine(script).Append("Deploy-Application -AppID '").Append(this.Id).AppendLine("' -Action Install");

                string command = @"get-wmiobject -query 'SELECT * FROM CCM_SoftwareUpdate' -namespace 'ROOT\ccm\ClientSDK' | Where-Object {$_.ArticleID -eq "+this.ArticleID+ @"} | ForEach-Object { Invoke-WmiMethod  -Namespace 'root\ccm\clientsdk' -Class CCM_SoftwareUpdatesManager -Name InstallUpdates -ArgumentList (,$_) }";
                Log.Info("Installing update " + this.Name);
                var posh = PoshHandler.GetRunner(command, RemoteSystem.Current);
                await PoshHandler.InvokeRunnerAsync(posh);
            }
        }
    }
}
