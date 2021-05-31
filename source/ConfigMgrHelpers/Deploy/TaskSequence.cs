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
    public class TaskSequence
    {
        public static string GetterCommand { get; } = @"get-wmiobject -query 'SELECT * FROM CCM_Program WHERE TaskSequence = True' -namespace 'ROOT\ccm\ClientSDK' | Select  Name, PackageID, HighImpactTaskSequence, LastRunTime, RepeatRunBehavior, RestartRequired";

        public string Name { get; set; }
        public string PackageID { get; set; }
        public bool HighImpactTaskSequence { get; set; }

        public static TaskSequence New(PSObject poshObj)
        {
            var cmobj = new TaskSequence();
            cmobj.Name = PoshHandler.GetPropertyValue<string>(poshObj, "Name");
            cmobj.PackageID = PoshHandler.GetPropertyValue<string>(poshObj, "PackageID");
            cmobj.HighImpactTaskSequence = PoshHandler.GetPropertyValue<bool>(poshObj, "HighImpactTaskSequence");
            return cmobj;
        }

        public async Task RunAsync()
        {
            if (string.IsNullOrWhiteSpace(this.PackageID) == false)
            {
                string command = "Get-WmiObject -Query 'SELECT PackageID, ProgramID FROM CCM_Program WHERE PackageID=\""+this.PackageID+"\"' -Namespace 'root\\ccm\\clientsdk' | ForEach-Object { Invoke-WmiMethod -class CCM_ProgramsManager -Namespace 'root\\ccm\\clientsdk' -Name ExecutePrograms -argumentlist $_ }";
                Log.Info("Running task sequence " + this.Name + ", ID:" + this.PackageID);
                var posh = PoshHandler.GetRunner(command, RemoteSystem.Current);
                await PoshHandler.InvokeRunnerAsync(posh);
            }
        }
    }
}
