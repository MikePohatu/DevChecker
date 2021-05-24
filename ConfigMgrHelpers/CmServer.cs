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

namespace ConfigMgrHelpers
{
    public class CmServer
    {
        //private Microsoft.ConfigurationManagement.Messaging.Framework. _connector;
        /// <summary>
        /// ** Not currently supported. Use WMI as opposed to Administration Service
        /// </summary>
        public bool UseWMI { get; set; } = true;

        /// <summary>
        /// Path of the WMI namespace for the SMS_Provider
        /// </summary>
        public string WmiNamespacePath { get; private set; }

        /// <summary>
        /// The ConfigMgr site code
        /// </summary>
        public string SiteCode { get; private set; }

        /// <summary>
        /// The server name of the configmgr server
        /// </summary>
        public string ServerName { get; set; } = string.Empty;

        public static CmServer Current { get; private set; }
        private CmServer(string ServerName)
        {
            this.ServerName = ServerName;
        }

        public static CmServer Create(string ServerName)
        {
            Current = new CmServer(ServerName);
            return Current;
        }

        public void ConnectWmi()
        {
            string query = "SELECT * FROM SMS_ProviderLocation";

            var results = WmiQuery.Create("ROOT\\SMS", query, this.ServerName).Run();
            this.WmiNamespacePath = WmiQuery.GetWmiProperty<string>(results, "NamespacePath");
            this.SiteCode = WmiQuery.GetWmiProperty<string>(results, "SiteCode");
        }
    }
}
