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
    public static class RepairTools
    {
		public static async Task RunRestoreHealthAsync()
		{
			if (RemoteSystem.Current.IsConnected)
			{
				Log.Info(Log.Highlight("Initiating RestoreHealth. Note that this may take some time"));
				string command = "Repair-WindowsImage -Online -RestoreHealth -LimitAccess -NoRestart | Foreach-Object { Write-Information \"**Image state: $($_.ImageHealthState)\" }";

				var posh = PoshHandler.GetRunner(command, RemoteSystem.Current);
				await PoshHandler.InvokeRunnerAsync(posh);
				Log.Info("Finished RestoreHealth repair");
			}
		}

		public static async Task RunCheckHealthAsync()
		{
			if (RemoteSystem.Current.IsConnected)
			{
				string command = "Repair-WindowsImage -Online -CheckHealth -NoRestart | Foreach-Object { Write-Information \"**Image state: $($_.ImageHealthState)\" }";

				var posh = PoshHandler.GetRunner(command, RemoteSystem.Current);
				await PoshHandler.InvokeRunnerAsync(posh);
				Log.Info("Finished CheckHealth");
			}
		}

		public static async Task RunScanHealthAsync()
		{
			if (RemoteSystem.Current.IsConnected)
			{
				Log.Info(Log.Highlight("Initiating ScanHealth. Note that this may take some time"));
				string command = "Repair-WindowsImage -Online -ScanHealth -NoRestart | Foreach-Object { Write-Information \"**Image state: $($_.ImageHealthState)\" }";

				var posh = PoshHandler.GetRunner(command, RemoteSystem.Current);
				await PoshHandler.InvokeRunnerAsync(posh);
				Log.Info("Finished ScanHealth");
			}
		}

		public static async Task RunSfcScanNowAsync()
		{
			try
			{
				string script = "Start-Process 'sfc.exe' '/scannow' -Wait";

				using (PowerShell posh = PoshHandler.GetRunner(script, RemoteSystem.Current))
				{
					await PoshHandler.InvokeRunnerAsync(posh);
					Log.Info("Done");
				}
			}
			catch (Exception e)
			{
				Log.Error(e, "Error running sfc /scannow");
			}
		}
	}
}
