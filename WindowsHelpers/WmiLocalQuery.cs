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
using System.Threading.Tasks;
using Diags;
using Diags.Logging;
using System.Management;

namespace WindowsHelpers
{
    internal class WmiLocalQuery
    {
        private List<ManagementBaseObject> _results = new List<ManagementBaseObject>();

        public string NameSpace { get; private set; }
        public string QueryString { get; private set; }
        public bool Completed { get; private set; } = false;


        /// <summary>
        /// Create a query specifying the WMI namespace, and the WMI query
        /// </summary>
        /// <param name="NameSpace"></param>
        /// <param name="WmiQuery"></param>
        public WmiLocalQuery(string NameSpace, string WmiQuery)
        {
            this.NameSpace = string.IsNullOrWhiteSpace(NameSpace) ? @"root\CIMV2" : NameSpace;
            this.QueryString = WmiQuery;
        }

        /// <summary>
        /// Create query with the default root\CIMV2 namespace
        /// </summary>
        /// <param name="WmiQuery"></param>
        public WmiLocalQuery(string WmiQuery)
        {
            this.NameSpace = @"root\CIMV2";
            this.QueryString = WmiQuery;
        }

        /// <summary>
        /// Set the specified query, ComputerName to local computer, and namespace to root\CIMV2 and use other pre-configured object options. Run the query
        /// </summary>
        /// <param name="WmiQuery"></param>
        /// <returns></returns>
        public async Task<List<ManagementBaseObject>> RunQueryAsync(string WmiQuery)
        {
            this.NameSpace = @"root\CIMV2";
            this.QueryString = WmiQuery;

            return await this.RunAsync();
        }

        /// <summary>
        /// Set the specified query and namespace, ComputerName to local computer, and use other pre-configured object options. Run the query
        /// </summary>
        /// <param name="NameSpace"></param>
        /// <param name="WmiQuery"></param>
        /// <returns></returns>
        public async Task<List<ManagementBaseObject>> RunQueryAsync(string NameSpace, string WmiQuery)
        {
            this.NameSpace = NameSpace;
            this.QueryString = WmiQuery;

            return await this.RunAsync();
        }

        /// <summary>
        /// Run the query using the options configured on the object
        /// </summary>
        /// <returns></returns>
        public async Task<List<ManagementBaseObject>> RunAsync()
        {
            if (this.Completed)
            {
                string message = "Query has already been run: \\\\.\\" + this.NameSpace + " : " + this.QueryString;
                LoggerFacade.Error(message);
                throw new KnownException(message, "");
            }

            try
            {
                ManagementScope scope = new ManagementScope("\\\\.\\" + this.NameSpace);
                ObjectQuery query = new ObjectQuery(this.QueryString);
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
                ManagementOperationObserver observer = new ManagementOperationObserver();

                // Attach handler to events for results and completion.  
                observer.ObjectReady += new ObjectReadyEventHandler(this.NewObject);
                observer.Completed += new CompletedEventHandler(this.Done);

                // Call the asynchronous overload of Get()  
                // to start the enumeration.  
                searcher.Get(observer);

                // Do something else while results  
                // arrive asynchronously.  
                while (!this.Completed)
                {
                    await Task.Delay(500);
                }

                return this._results;
            }
            catch (UnauthorizedAccessException e)
            {
                this.Completed = true;
                LoggerFacade.Error("Access denied to computer. " + e.Message);
                throw e;
            }
            catch (Exception e)
            {
                this.Completed = true;
                LoggerFacade.Error("Failed to run query: " + e.Message);
                throw e;
            }
        }

        private void Done(object sender, CompletedEventArgs obj)
        {
            this.Completed = true;
        }

        private void NewObject(object sender, ObjectReadyEventArgs obj)
        {
            this._results.Add(obj.NewObject);
        }
    }
}
