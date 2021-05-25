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
using Diags.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace WindowsHelpers
{
    public static class ServiceHelpers
    {
        public static async Task RestartService(string servicename, string computername, bool useSSL)
        {
            string scriptPath = AppDomain.CurrentDomain.BaseDirectory + "Scripts\\RestartService.ps1";
            
            try
            {
                string script = await IOHelpers.ReadFileAsync(scriptPath);
                using (PowerShell posh = PoshHandler.GetRunner(script, computername, useSSL))
                {
                    posh.AddStatement().AddCommand("Restart").AddParameter("ServiceName", servicename);
                    await PoshHandler.InvokeRunnerAsync(posh, true);
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Error restarting service: " + servicename);
            }
        }
    }
}
