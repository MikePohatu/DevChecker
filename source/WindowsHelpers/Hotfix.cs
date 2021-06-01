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
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace WindowsHelpers
{
    public class Hotfix
    {
        public static string GetterCommand { get; } = "Get-Hotfix";

        public string HotFixID { get; set; }
        public string Description { get; set; }
        public string InstalledOn { get; set; }
        public string InstalledBy { get; set; }

        public static Hotfix New(PSObject poshObj)
        {
            var obj = new Hotfix();
            obj.Description = PoshHandler.GetPropertyValue<string>(poshObj, "Description");
            obj.InstalledOn = PoshHandler.GetPropertyValue<string>(poshObj, "InstalledOn");
            obj.HotFixID = PoshHandler.GetPropertyValue<string>(poshObj, "HotFixID");
            obj.InstalledBy = PoshHandler.GetPropertyValue<string>(poshObj, "InstalledBy");
            return obj;
        }

        public async Task UninstallAsync()
        {
            if (string.IsNullOrWhiteSpace(this.HotFixID) == false)
            {
                string command = "Start-Process wusa.exe -ArgumentList \"/ uninstall / KB:"+this.HotFixID+" / quiet / norestart\"";
                Log.Info("Uninstalling hotfix " + this.HotFixID);
                var posh = PoshHandler.GetRunner(command, RemoteSystem.Current);
                await PoshHandler.InvokeRunnerAsync(posh);
            }
        }
    }
}
