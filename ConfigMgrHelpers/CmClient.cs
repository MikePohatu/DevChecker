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
		public bool IsConnected { get; set; } = true;
		public string ConnectString { get; private set; }
		public bool ConnectSSL { get; private set; }
		public List<CmClientAction> ClientActions { get; private set; }

		public CmClient(string connectstring, bool usessl)
        {
			this.ConnectString = connectstring;
			this.ConnectSSL = usessl;
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
	}
}
