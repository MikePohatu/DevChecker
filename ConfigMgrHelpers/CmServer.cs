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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using WindowsHelpers;
using Diags.Logging;

namespace ConfigMgrHelpers
{
    public class CmServer
    {
        //private Microsoft.ConfigurationManagement.Messaging.Framework. _connector;
        /// <summary>
        /// ** Not currently supported. Use WMI as opposed to Administration Service
        /// </summary>
        public bool UseWMI { get; set; } = false;

        public bool UseSSL { get; set; } = true;

        /// <summary>
        /// Path of the WMI namespace for the SMS_Provider
        /// </summary>
        public string WmiNamespacePath { get; private set; }

        /// <summary>
        /// Path of the WMI namespace for the SMS_Provider
        /// </summary>
        public string SiteWmiNamespace { get; private set; }

        /// <summary>
        /// The ConfigMgr site code
        /// </summary>
        public string SiteCode { get; private set; }

        /// <summary>
        /// The server name of the configmgr server to connect to
        /// </summary>
        public string ServerName { get; set; } = string.Empty;

        /// <summary>
        /// The version of the ConfigMgr site
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// The server name reported by ConfigMgr
        /// </summary>
        public string ReportedServerName { get; set; } = string.Empty;
        public static CmServer Current { get; private set; }
        private CmServer(string ServerName)
        {
            this.ServerName = ServerName;
        }

        public static CmServer Create(string ServerName, bool useSSL)
        {
            Current = new CmServer(ServerName);
            Current.UseSSL = useSSL;
            return Current;
        }

        public async Task ConnectPoshAsync()
        {
            string command = "Get-WmiObject -Namespace \"ROOT\\SMS\" -Query \"SELECT * FROM SMS_ProviderLocation\" -ComputerName " + this.ServerName;

            var posh = PoshHandler.GetRunner(command);
            var result = await PoshHandler.InvokeRunnerAsync(posh);

            this.WmiNamespacePath = PoshHandler.GetFirstPropertyValue<string>(result, "NamespacePath");
            this.SiteCode = PoshHandler.GetFirstPropertyValue<string>(result, "SiteCode");
            this.ReportedServerName = PoshHandler.GetFirstPropertyValue<string>(result, "Machine");
            this.SiteWmiNamespace = @"root\sms\site_" + this.SiteCode;
            LoggerFacade.Info("Connected to ConfigMgr server "+this.ServerName +", site code: " + this.SiteCode);
        }
    }
}
