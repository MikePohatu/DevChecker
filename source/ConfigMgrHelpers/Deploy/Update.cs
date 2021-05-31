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
        public static string GetterCommand { get; }= @"get-wmiobject -query 'SELECT * FROM CCM_SoftwareUpdate' -namespace 'ROOT\ccm\ClientSDK' | Select Name, ArticleID, BulletinID, MaxExecutionTime, URL";

        public string Name { get; set; }
        public string ArticleID { get; set; }
        public string BulletinID { get; set; }
        public int MaxExecutionTime { get; set; }
        public string URL { get; set; }

        public static Update New(PSObject poshObj)
        {
            var cmobj = new Update();
            cmobj.Name = PoshHandler.GetPropertyValue<string>(poshObj, "Name");
            cmobj.ArticleID = PoshHandler.GetPropertyValue<string>(poshObj, "ArticleID");
            cmobj.BulletinID = PoshHandler.GetPropertyValue<string>(poshObj, "BulletinID");
            cmobj.MaxExecutionTime = PoshHandler.GetPropertyValue<int>(poshObj, "MaxExecutionTime");
            cmobj.URL = PoshHandler.GetPropertyValue<string>(poshObj, "URL");
            return cmobj;
        }

        public async Task InstallAsync()
        {
            if (string.IsNullOrWhiteSpace(this.Name) == false)
            {
                //StringBuilder builder = new StringBuilder();
                //string scriptPath = AppDomain.CurrentDomain.BaseDirectory + "Scripts\\CMInstallApplication.ps1";
                //string script = await IOHelpers.ReadFileAsync(scriptPath);
                //builder.AppendLine(script).Append("Deploy-Application -AppID '").Append(this.Id).AppendLine("' -Action Install");

                Log.Info("Installing update " + this.Name);
                //var posh = PoshHandler.GetRunner(builder.ToString(), RemoteSystem.Current);
                //await PoshHandler.InvokeRunnerAsync(posh, true);
            }
        }

        public async Task UninstallAsync()
        {
            if (string.IsNullOrWhiteSpace(this.Name) == false)
            {
                //StringBuilder builder = new StringBuilder();
                //string scriptPath = AppDomain.CurrentDomain.BaseDirectory + "Scripts\\CMInstallApplication.ps1";
                //string script = await IOHelpers.ReadFileAsync(scriptPath);
                //builder.AppendLine(script).Append("Deploy-Application -AppID '").Append(this.Id).AppendLine("' -Action Uninstall");

                Log.Info("Uninstalling update " + this.Name);
                //var posh = PoshHandler.GetRunner(builder.ToString(), RemoteSystem.Current);
                //await PoshHandler.InvokeRunnerAsync(posh, true);
            }
        }
    }
}
