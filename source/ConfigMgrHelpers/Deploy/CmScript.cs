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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using WindowsHelpers;

namespace ConfigMgrHelpers.Deploy
{
    public class CmScript
    {
        public static string GetterQuery()
        {
            return "Get-WmiObject -Query 'SELECT ScriptName, ScriptVersion, Comment, Author, Approver, ScriptGuid FROM SMS_Scripts WHERE ApprovalState = 3 AND Feature=0' -ComputerName " + CmServer.Current.ServerName + " -Namespace \"" + CmServer.Current.SiteWmiNamespace + "\"";
        }

        public string Name { get; set; }
        public string Version { get; set; }
        public string Comment { get; set; }
        public string Author { get; set; }
        public string Approver { get; set; }
        public string Guid { get; set; }

        public CmScript(PSObject posh)
        {
            this.Name = PoshHandler.GetPropertyValue<string>(posh, "ScriptName");
            this.Comment = PoshHandler.GetPropertyValue<string>(posh, "Comment");
            this.Author = PoshHandler.GetPropertyValue<string>(posh, "Author");
            this.Approver = PoshHandler.GetPropertyValue<string>(posh, "Approver");
            this.Guid = PoshHandler.GetPropertyValue<string>(posh, "ScriptGuid");
            this.Version = PoshHandler.GetPropertyValue<string>(posh, "ScriptVersion");
        }



        public async Task RunAsync()
        {
            if (RemoteSystem.Current.IsLocalhostClient)
            {
                Log.Error("Can't run script if device is localhost");
            }
            else
            {
                Log.Info("Running script " + this.Name);
                string scriptPath = AppDomain.CurrentDomain.BaseDirectory + "Scripts\\CMInvokeScript.ps1";
                string script = await IOHelpers.ReadFileAsync(scriptPath);
                StringBuilder sb = new StringBuilder(script);
                sb.Append("Invoke-SCCMRunScript -SiteServer ").Append(CmServer.Current.ServerName).Append(" -Namespace ").Append(CmServer.Current.SiteWmiNamespace);
                sb.Append(" -ScriptGuid ").Append(this.Guid).Append(" -TargetResourceIDs ").Append(CmServerSideClient.Current.ResourceID);

                script = sb.ToString();
                using (var posh = new PoshHandler(script))
                {
                    var result = await posh.InvokeRunnerAsync(true);
                }
                Log.Info("Initiated script " + this.Name + ". Check the ConfigMgr console for status.");
            }
        }
    }
}
