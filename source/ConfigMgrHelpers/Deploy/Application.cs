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
using System;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using WindowsHelpers;

namespace ConfigMgrHelpers.Deploy
{
    public class Application
    {
        public static string GetterCommand { get; } = @"get-wmiobject -query 'SELECT * FROM CCM_Application' -namespace 'ROOT\ccm\ClientSDK' | Select Name, Id, InstallState, ResolvedState, Revision, SoftwareVersion";
        public string Name { get; set; }
        public string Version { get; set; }
        public string InstallState { get; set; }
        public string ResolvedState { get; set; }
        public int Revision { get; set; }
        public string Id { get; set; }

        public Application (PSObject poshObj)
        {
            this.Name = PoshHandler.GetPropertyValue<string>(poshObj, "Name");
            this.Version = PoshHandler.GetPropertyValue<string>(poshObj, "SoftwareVersion");
            this.Id = PoshHandler.GetPropertyValue<string>(poshObj, "Id");
            this.InstallState = PoshHandler.GetPropertyValue<string>(poshObj, "InstallState");
            this.ResolvedState = PoshHandler.GetPropertyValue<string>(poshObj, "ResolvedState");
            this.Revision = PoshHandler.GetPropertyValue<int>(poshObj, "Revision");
        }

        public async Task InstallAsync()
        {
            if (string.IsNullOrWhiteSpace(this.Id) == false)
            {
                StringBuilder builder = new StringBuilder();
                string scriptPath = AppDomain.CurrentDomain.BaseDirectory + "Scripts\\CMDeployApplication.ps1";
                string script = await IOHelpers.ReadFileAsync(scriptPath);
                builder.AppendLine(script).Append("Deploy-Application -AppID '").Append(this.Id).AppendLine("' -Action Install");

                Log.Info("Installing application " + this.Name);
                using (var posh = new PoshHandler(builder.ToString(), RemoteSystem.Current))
                {
                    await posh.InvokeRunnerAsync(true);
                }                    
            }
        }

        public async Task UninstallAsync()
        {
            if (string.IsNullOrWhiteSpace(this.Id) == false)
            {
                StringBuilder builder = new StringBuilder();
                string scriptPath = AppDomain.CurrentDomain.BaseDirectory + "Scripts\\CMInstallApplication.ps1";
                string script = await IOHelpers.ReadFileAsync(scriptPath);
                builder.AppendLine(script).Append("Deploy-Application -AppID '").Append(this.Id).AppendLine("' -Action Uninstall");

                Log.Info("Uninstalling application " + this.Name);
                using (var posh = new PoshHandler(builder.ToString(), RemoteSystem.Current))
                {
                    await posh.InvokeRunnerAsync(true);
                }                    
            }
        }
    }
}
