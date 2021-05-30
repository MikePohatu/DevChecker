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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsHelpers;

namespace ConfigMgrHelpers
{
	/// <summary>
	/// Client side information for the ConfigMgr client
	/// </summary>
    public class CmClient
	{ 
		public bool ClientInstalled { get; set; } = false;

		public string ClientVersion { get; private set; }

		/// <summary>
		/// The name reported by the ConfigMgrServer
		/// </summary>
		public string ReportedName { get; private set; }

		public List<CmClientAction> ClientActions { get; private set; }

		public static CmClient Current;
		public static CmClient New()
        {
			Current = new CmClient();
			return Current;
        }

		private CmClient()
        {
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

		public async Task QueryClientAsync()
		{
			if (this.ClientInstalled)
			{
				Log.Info("Gathering ConfigMgr client info");
				string command = @"Get-WmiObject -Namespace root\ccm -Query 'SELECT * FROM SMS_Client'"; 

				var posh = PoshHandler.GetRunner(command, RemoteSystem.Current);
				var result = await PoshHandler.InvokeRunnerAsync(posh);

				if (result.Count > 0)
				{
					this.ClientVersion = PoshHandler.GetFirstPropertyValue<string>(result, "ClientVersion");
					this.ReportedName = PoshHandler.GetFirstPropertyValue<string>(result, "PSComputerName");
					Log.Info("Finished gathering ConfigMgr client info");
				}
			}
		}

		public void OpenLogs()
		{
			string path = @"\\" + RemoteSystem.Current.ComputerName + @"\c$\Windows\ccm\Logs";

			try
			{
				Process.Start(path);
			}
			catch (Exception e)
			{
				Log.Error(e, "Error opening " + path);
			}
		}
	}
}
