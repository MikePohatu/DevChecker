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
using ConfigMgrHelpers.Deploy;

namespace ConfigMgrHelpers
{
    /// <summary>
    /// Server side information for the device from ConfigMgr
    /// </summary>
    public class CmServer: ViewModelBase
    {
        /// <summary>
        /// Static event. Fired when server is connected
        /// </summary>
        public static event EventHandler Connected;

        //private Microsoft.ConfigurationManagement.Messaging.Framework. _connector;
        /// <summary>
        /// ** Not currently supported. Use WMI as opposed to Administration Service
        /// </summary>
        public bool UseWMI { get; set; } = false;

        public bool UseSSL { get; set; } = true;

        private bool _scriptsLoading = false;
        public bool ScriptsLoading
        {
            get { return this._scriptsLoading; }
            set { this._scriptsLoading = value; this.OnPropertyChanged(this, "ScriptsLoading"); }
        }

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
        /// A list of ConfigMgr scripts
        /// </summary>
        public ObservableCollection<object> Scripts { get; } = new ObservableCollection<object>();

        public CmServerSideClient Client { get; private set; }

        public static CmServer Current { get; private set; }
        private CmServer(string ServerName)
        {
            this.ServerName = ServerName;
        }

        public static CmServer Create(string serverName, bool useSSL, string clientName, Credential servercred)
        {
            Current = new CmServer(serverName);
            Current.UseSSL = useSSL;
            Current.Credential = servercred;
            Current.Client = CmServerSideClient.Create(clientName);
            return Current;
        }

        public async Task ConnectAsync()
        {
            string command = "Get-WmiObject -Namespace \"ROOT\\SMS\" -Query \"SELECT * FROM SMS_ProviderLocation\" -ComputerName " + this.ServerName;

            using (var posh = new PoshHandler(command))
            {
                var result = await posh.InvokeRunnerAsync();

                if (result != null)
                {
                    this.IsConnected = true;
                    this.WmiNamespacePath = PoshHandler.GetFirstPropertyValue<string>(result, "NamespacePath");
                    this.SiteCode = PoshHandler.GetFirstPropertyValue<string>(result, "SiteCode");
                    this.ReportedServerName = PoshHandler.GetFirstPropertyValue<string>(result, "Machine");
                    this.SiteWmiNamespace = @"root\sms\site_" + this.SiteCode;
                    Log.Info("Connected to ConfigMgr server " + this.ServerName + ", site code: " + this.SiteCode);

                    Connected?.Invoke(this, new EventArgs());
                    await this.Client.QueryClientAsync();
                }
            }
        }

        public async Task QueryScripts()
        {
            Log.Info("Refreshing ConfigMgr scripts");
            this.ScriptsLoading = true;
            this.Scripts.Clear();
            //string command = "Get-WmiObject -Namespace \"ROOT\\SMS\" -Query \"SELECT * FROM SMS_ProviderLocation\" -ComputerName " + this.ServerName;
            string command = CmScript.GetterQuery();

            using (var posh = new PoshHandler(command))
            {
                var result = await posh.InvokeRunnerAsync();

                if (result != null)
                {
                    foreach (var obj in result)
                    {
                        this.Scripts.Add(new CmScript(obj));
                    }
                }
            }
                
            this.ScriptsLoading = false;
        }
    }
}
