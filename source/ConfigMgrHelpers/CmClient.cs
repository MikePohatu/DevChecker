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
using ConfigMgrHelpers.Deploy;
using Core;
using Core.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using WindowsHelpers;

namespace ConfigMgrHelpers
{
	/// <summary>
	/// Client side information for the ConfigMgr client
	/// </summary>
    public class CmClient: ViewModelBase
	{
		private bool _logonEventsLoading = false;
		public bool LogonEventsLoading
		{
			get { return this._logonEventsLoading; }
			set { this._logonEventsLoading = value; this.OnPropertyChanged(this, "LogonEventsLoading"); }
		}

		public bool ClientInstalled { get; set; } = false;

		public string ClientVersion { get; private set; }

		public string ResourceID { get; private set; }

		/// <summary>
		/// The name reported by the ConfigMgrServer
		/// </summary>
		public string ReportedName { get; private set; }

		public SoftwareCenter SoftwareCenter { get; private set; }
		public List<CmClientAction> ClientActions { get; private set; }

		public ObservableCollection<object> LogonEvents { get; } = new ObservableCollection<object>();

		public static CmClient Current;
		public static CmClient New()
        {
			Current = new CmClient();
			return Current;
        }

		private CmClient()
        {
			this.SoftwareCenter = new SoftwareCenter();
			this.ClientActions = new List<CmClientAction>() {

				new CmClientAction( "AppDeployment", "{00000000-0000-0000-0000-000000000121}","Application Deployment Evaluation"),
				new CmClientAction( "DiscoveryData", "{00000000-0000-0000-0000-000000000003}","Discovery Data Collection" ),
				new CmClientAction( "FileCollection", "{00000000-0000-0000-0000-000000000010}","File Collection Cycle" ),
				new CmClientAction( "HardwareInventory", "{00000000-0000-0000-0000-000000000001}","Hardware Inventory"),
				//new CmClientAction( "MachinePolicyRetrieval", "{00000000-0000-0000-0000-000000000021}", "Machine Policy Retrieval"),
				new CmClientAction( "MachinePolicyEvaluation", "{00000000-0000-0000-0000-000000000022}", "Machine Policy Evaluation"),
				new CmClientAction( "SoftwareInventory", "{00000000-0000-0000-0000-000000000002}","Software Inventory"),
				new CmClientAction( "SoftwareMetering", "{00000000-0000-0000-0000-000000000031}","Software Metering Usage Report"),
				new CmClientAction( "ComplianceEvaluation", "{00000000-0000-0000-0000-000000000071}","Compliance Evaluation"),
				new CmClientAction( "UpdateDeployment", "{00000000-0000-0000-0000-000000000108}","Software Update Deployment Evaluation"),
				new CmClientAction( "UpdateScan", "{00000000-0000-0000-0000-000000000113}","Software Update Scan"),
				new CmClientAction( "StateMessage", "{00000000-0000-0000-0000-000000000111}","State Message Refresh"),
				//new CmClientAction( "UserPolicyRetrieval", "{00000000-0000-0000-0000-000000000026}","User Policy Retrieval"),
				new CmClientAction( "UserPolicyEvaluation", "{00000000-0000-0000-0000-000000000027}","User Policy Evaluation"),
				new CmClientAction( "WindowsInstallersSourceListUpdate", "{00000000-0000-0000-0000-000000000032}","Windows Installers Source List Update")
			};
		}

		public async Task QueryAsync()
        {
			this.ClientInstalled = true;
			List<Task> tasks = new List<Task>();
			tasks.Add(this.QueryClientAsync());
			tasks.Add(this.SoftwareCenter.QueryApplicationsAsync());
			tasks.Add(this.SoftwareCenter.QueryUpdatesAsync());
			tasks.Add(this.SoftwareCenter.QueryTaskSequencesAsync());
			tasks.Add(this.QueryLogonEventsAsync());
			await Task.WhenAll(tasks);
		}

		private async Task QueryClientAsync()
		{
			if (this.ClientInstalled)
			{
				Log.Info("Gathering ConfigMgr client info");
				string command = @"Get-WmiObject -Namespace root\ccm -Query 'SELECT * FROM SMS_Client'"; 

				using (var posh = new PoshHandler(command, RemoteSystem.Current))
				{
					var result = await posh.InvokeRunnerAsync();
					if (result.Count > 0)
					{
						this.ClientVersion = PoshHandler.GetFirstPropertyValue<string>(result, "ClientVersion");
						this.ReportedName = PoshHandler.GetFirstPropertyValue<string>(result, "PSComputerName");
						this.ResourceID = PoshHandler.GetFirstPropertyValue<string>(result, "ResourceId");
						Log.Info("Finished gathering ConfigMgr client info");
					}
				}				
			}
		}

		public async Task QueryLogonEventsAsync()
		{
			if (this.ClientInstalled)
			{
				this.LogonEventsLoading = true;
				Log.Info("Gathering logon events from ConfigMgr client");
				Log.Info(@"Get-WmiObject –Namespace ROOT\CCM –Class CCM_UserLogonEvents | select -First 50 | Sort-Object -Property LogonTime -Descending");
				this.LogonEvents.Clear();
				string scriptPath = AppDomain.CurrentDomain.BaseDirectory + "Scripts\\CMGetLogonHistory.ps1";
				string script = await IOHelpers.ReadFileAsync(scriptPath);

				using (var posh = new PoshHandler(script, RemoteSystem.Current))
				{
					var result = await posh.InvokeRunnerAsync(true);

					if (result.Count > 0)
					{
						foreach (PSObject obj in result)
						{
							this.LogonEvents.Add(new LogonEvent(obj));
						}
						Log.Info("Finished gathering logon events");
					}
				}
				this.LogonEventsLoading = false;
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

		public void OpenSetupLogs()
		{
			string path = @"\\" + RemoteSystem.Current.ComputerName + @"\c$\Windows\ccmsetup\Logs";

			try
			{
				Process.Start(path);
			}
			catch (Exception e)
			{
				Log.Error(e, "Error opening " + path);
			}
		}

		public async Task RepairClientAsync()
		{
			if (this.ClientInstalled)
			{
				Log.Info(Log.Highlight(@"Repairing ConfigMgr client. Please check c:\Windows\ccmsetup\Logs to monitor repair status"));
				string scriptPath = AppDomain.CurrentDomain.BaseDirectory + "Scripts\\CMRepairClient.ps1";
				string script = await IOHelpers.ReadFileAsync(scriptPath);


				using (var posh = new PoshHandler(script, RemoteSystem.Current))
                {
					await posh.InvokeRunnerAsync(true);
				}					
			}
		}
	}
}
