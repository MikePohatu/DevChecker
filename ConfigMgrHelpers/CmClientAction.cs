#region license
// Copyright (c) 2021 20Road Limited
//
// This file is part of 20Road Remote Admin.
//
// 20Road Remote Admin is free software: you can redistribute it and/or modify
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
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using WindowsHelpers;

namespace ConfigMgrHelpers
{
    public class CmClientAction
    {
        public string ID { get; private set; }
        public string Name { get; private set; }
        public string DisplayName { get; private set; }

        public CmClientAction(string name, string id, string displayName, CmClient parent)
        {
            this.ID = id;
            this.Name = name;
            this.DisplayName = displayName;
        }

        public async Task RunActionAsync()
        {
            Log.Info("Running client action: " + this.DisplayName);
            string scriptPath = AppDomain.CurrentDomain.BaseDirectory + "Scripts\\CMRunClientAction.ps1";
            string script = await IOHelpers.ReadFileAsync(scriptPath);
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("ClientAction", this.Name);

            PowerShell posh = PoshHandler.GetRunner(script, RemoteSystem.Current);
            posh.AddStatement().AddCommand("Run-CMAction").AddParameter("ClientAction", this.ID);
    
            await PoshHandler.InvokeRunnerAsync(posh, true);
        }
    }
}
