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

// SystemConnector.cs - class to connect to standard Windows components (WMI and 
// environment variables. 

using System;
using System.Management;
using System.Collections.Generic;
using Diags.Logging;
using System.Threading.Tasks;

namespace WindowsHelpers
{
    public static class LocalSystemConnector
    {
        public static string GetVariableValue(string Variable)
        {
            if (Variable == null) { return null; }
            string s;

            //try process variables
            s = Environment.GetEnvironmentVariable(Variable, EnvironmentVariableTarget.Process);
            if (!string.IsNullOrEmpty(s)) { return s; }

            //try computer variables
            s = Environment.GetEnvironmentVariable(Variable, EnvironmentVariableTarget.Machine);
            if (!string.IsNullOrEmpty(s)) { return s; }

            //try user variables
            s = Environment.GetEnvironmentVariable(Variable, EnvironmentVariableTarget.User);
            if (!string.IsNullOrEmpty(s)) { return s; }

            //not found. return null
            return null;
        }


        /// <summary>
        /// Get a value from WMI, using the root\CIMV2 namespace
        /// </summary>
        /// <param name="WmiQuery"></param>
        /// <returns></returns>
        public static async Task<string> GetWmiStringAsync(string WmiQuery)
        {
            return await GetWmiStringAsync(@"root\CIMV2", WmiQuery);
        }


        /// <summary>
        /// /get a value from WMI, specifying the desired namespace
        /// </summary>
        /// <param name="NameSpace"></param>
        /// <param name="WmiQuery"></param>
        /// <returns></returns>
        public static async Task<string> GetWmiStringAsync(string NameSpace, string WmiQuery)
        {
            string s = null;
            try
            {
                WmiLocalQuery query = new WmiLocalQuery(NameSpace, WmiQuery);
                List<ManagementBaseObject> results = await query.RunAsync();

                foreach (ManagementBaseObject m in results)
                {
                    foreach (PropertyData propdata in m.Properties)
                    {
                        s = s + propdata.Value;
                    }
                }

                if (String.IsNullOrEmpty(s)) { return null; }
                else { return s; }
            }
            catch (Exception e)
            {
                LoggerFacade.Error(e, "Error running query against namespace " + NameSpace + ": " + WmiQuery);
                return null;
            }

        }

        /// <summary>
        /// /get a ManagementBaseObject List from WMI, specifying the desired namespace
        /// </summary>
        /// <param name="NameSpace"></param>
        /// <param name="WmiQuery"></param>
        public static async Task<List<ManagementBaseObject>> GetWmiManagementObjectListAsync(string NameSpace, string WmiQuery)
        {
            try
            {
                WmiLocalQuery query = new WmiLocalQuery(NameSpace, WmiQuery);
                var results = await query.RunAsync();
                return results;
            }
            catch (Exception e)
            {
                LoggerFacade.Error(e, "Error running query against namespace " + NameSpace + ": " + WmiQuery);
                throw e;
            }
        }
    }
}