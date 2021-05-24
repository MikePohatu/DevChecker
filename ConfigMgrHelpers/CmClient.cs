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
using System.Text;
using System.Threading.Tasks;
using WindowsHelpers;

namespace ConfigMgrHelpers
{
    public class CmClient
	{ 
		public bool ClientInstalled { get; set; } = false;
		public string ConnectString { get; private set; }

		/// <summary>
		/// The name reported by the ConfigMgrServer
		/// </summary>
		public string ReportedName { get; private set; }

		/// <summary>
		/// The IPs address recorded in ConfigMgr
		/// </summary>
		public string IPs { get; private set; }

		/// <summary>
		/// The Active Directory Organisational Unit
		/// </summary>
		public string OU { get; private set; }
		public List<CmClientAction> ClientActions { get; private set; }

		public CmClient(string connectstring, bool usessl)
        {
			this.ConnectString = connectstring;
			this.ClientActions = new List<CmClientAction>() {
				new CmClientAction( "MachinePolicy", "{00000000-0000-0000-0000-000000000021}", "Machine Policy", this),
				new CmClientAction( "DiscoveryData", "{00000000-0000-0000-0000-000000000003}","Discovery Data", this ),
				new CmClientAction( "ComplianceEvaluation", "{00000000-0000-0000-0000-000000000071}","Compliance Evaluation", this),
				new CmClientAction( "AppDeployment", "{00000000-0000-0000-0000-000000000121}","App Deployment", this),
				new CmClientAction( "HardwareInventory", "{00000000-0000-0000-0000-000000000001}","Hardware Inventory", this),
				new CmClientAction( "UpdateDeployment", "{00000000-0000-0000-0000-000000000108}","Update Deployment", this),
				new CmClientAction( "UpdateScan", "{00000000-0000-0000-0000-000000000113}","Update Scan", this),
				new CmClientAction( "SoftwareInventory", "{00000000-0000-0000-0000-000000000002}","Software Inventory", this)
			};
		}

		public async Task QueryServerAsync()
        {
			if (string.IsNullOrWhiteSpace(this.ConnectString) || this.ConnectString.ToLower() == "localhost" || this.ConnectString == "127.0.0.1")
            {
				LoggerFacade.Info("Skipping ConfigMgr check for localhost client");
            } 
			else
            {
				LoggerFacade.Info("Gathering ConfigMgr data client");
				string command = "(Get-WmiObject -Class SMS_R_SYSTEM -Namespace \"" + CmServer.Current.SiteWmiNamespace + "\" -ComputerName " + CmServer.Current.ServerName + " | where {$_.Name -eq \"" + this.ConnectString + "\"})";

				var posh = PoshHandler.GetRunner(command);
				var result = await PoshHandler.InvokeRunnerAsync(posh);

				if (result.Count > 0)
                {
					this.IPs = string.Join(", ", PoshHandler.GetFirstPropertyValue<string[]>(result, "IPAddresses"));
					this.OU = PoshHandler.GetFirstPropertyValue<string[]>(result, "SystemOUName").Last();
					this.ReportedName = PoshHandler.GetFirstPropertyValue<string>(result, "Name");

					LoggerFacade.Info("Finished gathering ConfigMgr data for client");
				}
			}
		}
	}
}
