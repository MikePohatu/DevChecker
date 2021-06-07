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
using Core;
using Core.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using WindowsHelpers;

namespace ConfigMgrHelpers
{
    public class CmServerSideClient: ViewModelBase
    { 


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
        /// The Active Directory Organisational Unit of the client
        /// </summary>
        public string LastLogonUserName { get; private set; }

        /// <summary>
        /// Has a localhost name or IP been specified
        /// </summary>
        public bool IsLocalhostClient { get; private set; } = false;

        /// <summary>
        /// CM resource ID
        /// </summary>
        public string ResourceID { get; private set; }

        /// <summary>
        /// Properties dictionary from script
        /// </summary>
        public SortedDictionary<string, string> Properties { get; private set; }


        /// <summary>
        /// List of collections assigned to the device
        /// </summary>
        public ObservableCollection<CmCollection> Collections { get; private set; } = new ObservableCollection<CmCollection>();

        public static CmServerSideClient Current { get; private set; }

        private CmServerSideClient(string clientName)
        {
            if (string.IsNullOrWhiteSpace(clientName) || clientName.ToLower() == "localhost" || clientName == "127.0.0.1")
            {
                this.IsLocalhostClient = true;
                Log.Info("Skipping ConfigMgr check for localhost client");
            }
            this.ClientName = clientName;
        }

        public static CmServerSideClient Create(string clientName)
        {
            Current = new CmServerSideClient(clientName);
            return Current;
        }


        public async Task QueryClientAsync()
        {
            if (!this.IsLocalhostClient)
            {
                try
                {
                    Log.Info("Gathering ConfigMgr server data for client");
                    string scriptPath = AppDomain.CurrentDomain.BaseDirectory + "Scripts\\CMGetServerSideInfo.ps1";
                    string script = await IOHelpers.ReadFileAsync(scriptPath);
                    StringBuilder sb = new StringBuilder(script);
                    sb.AppendLine("Get-ServerSideInfo -ComputerName " + this.ClientName + " -NameSpace " + CmServer.Current.SiteWmiNamespace + " -Server " + CmServer.Current.ServerName);


                    using (var posh = new PoshHandler(sb.ToString()))
                    {
                        PSDataCollection<PSObject> results = await posh.InvokeRunnerAsync(true);
                        if (results != null)
                        {
                            this.ClientIPs = PoshHandler.GetFirstHashTableString(results, "IPAddresses");
                            this.ClientOU = PoshHandler.GetFirstHashTableString(results, "OU");
                            this.ResourceID = PoshHandler.GetFirstHashTableString(results, "ResourceId");
                            this.Properties = PoshHandler.GetFromHashTableAsOrderedDictionary(results);
                            Log.Info("Finished gathering ConfigMgr server data for client");
                        }
                        else
                        {
                            Log.Error("Couldn't gather device information for " + this.ClientName);
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e, "Error querying ConfigMgr client system: " + this.ClientName);
                }
            }
        }

        public async Task QueryCollectionsAsync()
        {
            if (!this.IsLocalhostClient)
            {
                Log.Info("Gathering collections");
                string command = "Get-WmiObject -ComputerName " + CmServer.Current.ServerName + " -Namespace \"" + CmServer.Current.SiteWmiNamespace + "\"  -Query \"SELECT DISTINCT SMS_Collection.* FROM SMS_FullCollectionMembership, SMS_Collection where name = '" + this.ClientName + "' and SMS_FullCollectionMembership.CollectionID = SMS_Collection.CollectionID\"";

                using (var posh = new PoshHandler(command))
                {
                    var result = await posh.InvokeRunnerAsync();

                    if (result.Count > 0)
                    {
                        foreach (PSObject obj in result)
                        {
                            string colname = PoshHandler.GetPropertyValue<string>(obj, "Name");
                            string colid = PoshHandler.GetPropertyValue<string>(obj, "CollectionID");
                            this.Collections.Add(new CmCollection(colname, colid));
                        }

                        Log.Info("Finished gathering collections");
                    }
                }                    
            }
        }
    }
}
