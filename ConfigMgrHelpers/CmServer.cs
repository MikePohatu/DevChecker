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
using Core.Logging;
using System.Collections.ObjectModel;
using System.Management.Automation;
using Core;

namespace ConfigMgrHelpers
{
    /// <summary>
    /// Server side information for the device from ConfigMgr
    /// </summary>
    public class CmServer
    {
        //private Microsoft.ConfigurationManagement.Messaging.Framework. _connector;
        /// <summary>
        /// ** Not currently supported. Use WMI as opposed to Administration Service
        /// </summary>
        public bool UseWMI { get; set; } = false;

        public bool UseSSL { get; set; } = true;


        /// <summary>
        /// The credentials used to connect to the ConfigMgr server
        /// </summary>
        public Credential Credential { get; set; }

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
        /// Whether the connection to the ConfigMgr server is successful
        /// </summary>
        public bool IsConnected { get; private set; } = false;

        /// <summary>
        /// Has a localhost name or IP been specified
        /// </summary>
        public bool IsLocalhostClient { get; private set; } = false;

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

        /// <summary>
        /// The name of the client to be queried in ConfigMgr
        /// </summary>
        public string ClientName { get; private set; }

        /// <summary>
		/// The IPs address recorded in ConfigMgr for the client
		/// </summary>
		public string ClientIPs { get; private set; }

        /// <summary>
        /// The Active Directory Organisational Unit of the client
        /// </summary>
        public string ClientOU { get; private set; }

        /// <summary>
        /// List of collections assigned to the device
        /// </summary>
        public ObservableCollection<CmCollection> Collections { get; private set; } = new ObservableCollection<CmCollection>();

        public static CmServer Current { get; private set; }
        private CmServer(string ServerName)
        {
            this.ServerName = ServerName;
        }

        public static CmServer Create(string serverName, bool useSSL, string clientName)
        {
            Current = new CmServer(serverName);
            Current.UseSSL = useSSL;
            Current.ClientName = clientName;
            if (string.IsNullOrWhiteSpace(clientName) || clientName.ToLower() == "localhost" || clientName == "127.0.0.1")
            {
                Current.IsLocalhostClient = true;
                Log.Info("Skipping ConfigMgr check for localhost client");
            }
            return Current;
        }

        public async Task ConnectAsync()
        {
            string command = "Get-WmiObject -Namespace \"ROOT\\SMS\" -Query \"SELECT * FROM SMS_ProviderLocation\" -ComputerName " + this.ServerName;

            var posh = PoshHandler.GetRunner(command);
            var result = await PoshHandler.InvokeRunnerAsync(posh);

            if (result != null)
            {
                this.IsConnected = true;
                this.WmiNamespacePath = PoshHandler.GetFirstPropertyValue<string>(result, "NamespacePath");
                this.SiteCode = PoshHandler.GetFirstPropertyValue<string>(result, "SiteCode");
                this.ReportedServerName = PoshHandler.GetFirstPropertyValue<string>(result, "Machine");
                this.SiteWmiNamespace = @"root\sms\site_" + this.SiteCode;
                Log.Info("Connected to ConfigMgr server " + this.ServerName + ", site code: " + this.SiteCode);

                await this.QueryClientAsync();
            }
        }

        public async Task QueryClientAsync()
        {
            if (!this.IsLocalhostClient)
            {
                Log.Info("Gathering ConfigMgr server data for client");
                string command = "(Get-WmiObject -Class SMS_R_SYSTEM -Namespace \"" + CmServer.Current.SiteWmiNamespace + "\" -ComputerName " + CmServer.Current.ServerName + " | where {$_.Name -eq \"" + this.ClientName + "\"})";

                var posh = PoshHandler.GetRunner(command);
                var result = await PoshHandler.InvokeRunnerAsync(posh);

                if (result.Count > 0)
                {
                    this.ClientIPs = string.Join(", ", PoshHandler.GetFirstPropertyValue<string[]>(result, "IPAddresses"));
                    this.ClientOU = PoshHandler.GetFirstPropertyValue<string[]>(result, "SystemOUName").Last();

                    Log.Info("Finished gathering ConfigMgr server data for client");
                }
            }
        }

        public async Task QueryCollectionsAsync()
        {
            if (!this.IsLocalhostClient)
            {
                Log.Info("Gathering collections");
                string command = "Get-WmiObject -ComputerName " + CmServer.Current.ServerName + " -Namespace \"" + CmServer.Current.SiteWmiNamespace + "\"  -Query \"SELECT DISTINCT SMS_Collection.* FROM SMS_FullCollectionMembership, SMS_Collection where name = '" + this.ClientName + "' and SMS_FullCollectionMembership.CollectionID = SMS_Collection.CollectionID\"";
                
                var posh = PoshHandler.GetRunner(command);
                var result = await PoshHandler.InvokeRunnerAsync(posh);

                if (result.Count > 0)
                {
                    foreach (PSObject obj in result)
                    {
                        string colname = PoshHandler.GetPropertyValue<string>(obj, "Name");
                        string colid = PoshHandler.GetPropertyValue<string>(obj, "CollectionID");
                        this.Collections.Add(new CmCollection(colname, colid));
                    }

                    //this.ClientIPs = string.Join(", ", PoshHandler.GetFirstPropertyValue<string[]>(result, "IPAddresses"));
                    //this.ClientOU = PoshHandler.GetFirstPropertyValue<string[]>(result, "SystemOUName").Last();

                    Log.Info("Finished gathering collections");
                }
            }
        }
    }
}
