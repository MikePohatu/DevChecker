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
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using Core.Logging;
using System.Collections;
using System.Text;
using Core;

namespace WindowsHelpers
{
    public static class PoshHandler
    {
        public static bool LogVerbose { get; set; } = false;
        public static bool LogProgress { get; set; } = false;

        private static PowerShell GetRunner(string script, string computerName, bool useSSL, int port, Credential cred)
        {
            bool credsSet = cred == null ? false : cred.CredentialsSet;
            Log.Trace($"PoshHandler.GetRunner called. computerName:{computerName}, useSSL:{useSSL}, port:{port}, cred:{credsSet}");
            //create the creds
            PSCredential currentCred;
            if (credsSet)
            {
                string user = string.IsNullOrWhiteSpace(cred.Domain) ? cred.Username : cred.Domain + "\\" + cred.Username;
                currentCred = new PSCredential(user, cred.SecurePassword);
            }
            else
            {
                currentCred = PSCredential.Empty;
            }

            //create the connection
            WSManConnectionInfo connectioninfo;
            if (string.IsNullOrWhiteSpace(computerName) || computerName == "." || computerName == "localhost" || computerName == "127.0.0.1")
            {
                Log.Debug("Connecting to localhost uri: " + computerName);
                connectioninfo = null;
            }
            else 
            { 
                string shellUri = "http://schemas.microsoft.com/powershell/Microsoft.PowerShell";
                connectioninfo = new WSManConnectionInfo(useSSL, computerName, port, "/wsman", shellUri, currentCred, 5000);
                if (currentCred != PSCredential.Empty && cred.UseKerberos)
                {
                    Log.Debug("Connecting with kerberos");
                    connectioninfo.AuthenticationMechanism = AuthenticationMechanism.Kerberos;
                }  
                else
                {
                    Log.Debug("No kerberos");
                }
            }

            //create the runspace
            Runspace runspace = connectioninfo == null ? RunspaceFactory.CreateRunspace() : RunspaceFactory.CreateRunspace(connectioninfo);
            runspace.Open();

            runspace.CreatePipeline();
            PowerShell posh = PowerShell.Create();
            posh.Streams.Warning.DataAdded += WarnEventHandler;
            posh.Streams.Error.DataAdded += ErrorEventHandler;
            posh.Streams.Information.DataAdded += InfoEventHandler;
            posh.Streams.Verbose.DataAdded += VerboseEventHandler;
            posh.Streams.Progress.DataAdded += ProgressEventHandler;

            posh.Runspace = runspace;
            if (string.IsNullOrWhiteSpace(script) == false)
            {
                posh.AddScript(script,false);
            }
            return posh;
        }

        public async static Task<PSDataCollection<PSObject>> InvokeRunnerAsync(PowerShell posh)
        {
            return await InvokeRunnerAsync(posh, false);
        }

        public async static Task<PSDataCollection<PSObject>> InvokeRunnerAsync(PowerShell posh, bool hideScript)
        {
            PSDataCollection<PSObject> result = null;
            try
            {
                if (!hideScript) 
                {
                    StringBuilder builder = new StringBuilder();
                    foreach (Command command in posh.Commands.Commands)
                    {
                        builder.AppendLine(command.CommandText);
                    }
                    Log.Info(builder.ToString().Trim()); 
                }
                result = await Task.Factory.FromAsync(posh.BeginInvoke(), asyncResult => posh.EndInvoke(asyncResult));
            }
            catch (Exception e)
            {
                Log.Error(e, "Error running PowerShell script");
            }
            return result;
        }

        public static PowerShell GetRunner(string script)
        {
            return GetRunner(script, "localhost", false, null);
        }

        public static PowerShell GetRunner(string script, RemoteSystem remote)
        {
            return GetRunner(script, remote.ComputerName, remote.UseSSL, remote.Credential);
        }

        public static PowerShell GetRunner(string script, Credential cred)
        {
            return GetRunner(script, "localhost", false, cred);
        }

        public static PowerShell GetRunner(string computerName, bool useSSL, Credential cred)
        {
            return GetRunner(null, computerName, useSSL, cred);
        }

        public static PowerShell GetRunner(string script, string computerName, bool useSSL, Credential cred)
        {
            int port = useSSL ? 5986 : 5985;
            return GetRunner(script, computerName, useSSL, port, cred);
        }

        public static T GetPropertyValue<T>(PSObject obj, string valueName)
        {
            T converted = default(T);
            if (obj != null)
            {
                object newobj = obj.Properties[valueName]?.Value;

                if (newobj != null)
                {
                    converted = (T)Convert.ChangeType(newobj, typeof(T));
                }
            }

            return converted;
        }

        public static T GetFirstHashTableValue<T>(PSDataCollection<PSObject> objList, string valueName)
        {
            if (objList != null)
            {
                foreach (PSObject obj in objList)
                {
                    Hashtable hash = obj.BaseObject as Hashtable;
                    object newobj = hash[valueName];

                    if (newobj != null)
                    {
                        T converted = (T)Convert.ChangeType(newobj, typeof(T));
                        return converted;
                    }
                }
            }
            
            return default(T);
        }

        public static string GetFirstHashTableString(PSDataCollection<PSObject> objList, string valueName)
        {
            if (objList != null)
            {
                foreach (PSObject obj in objList)
                {
                    Hashtable hash = obj.BaseObject as Hashtable;
                    object newobj = hash[valueName];

                    if (newobj != null)
                    {
                        return newobj.ToString();
                    }
                }
            }
                
            return null;
        }

        public static SortedDictionary<string, string> GetFromHashTableAsOrderedDictionary(PSDataCollection<PSObject> objList)
        {
            SortedDictionary<string, string> vals = new SortedDictionary<string, string>();
            if (objList != null)
            {
                foreach (PSObject obj in objList)
                {
                    Hashtable hash = obj.BaseObject as Hashtable;
                    foreach (string key in hash.Keys)
                    {
                        string val = hash[key] == null ? string.Empty : hash[key].ToString();
                        if (vals.ContainsKey(key)) { vals[key] = val; }
                        else { vals.Add(key, val); }
                    }
                }
            }

            return vals;
        }

        public static T GetFirstPropertyValue<T>(PSDataCollection<PSObject> objList)
        {
            if (objList != null)
            {
                return (T)Convert.ChangeType(objList[0], typeof(T));
            }
            else
            {
                return default(T);
            }
        }

        /// <summary>
        /// Get the value of the first property with the specified name from a list of PSObjects
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objList"></param>
        /// <param name="valueName"></param>
        /// <returns></returns>
        public static T GetFirstPropertyValue<T>(PSDataCollection<PSObject> objList, string valueName)
        {
            if (objList != null)
            {
                foreach (PSObject obj in objList)
                {
                    object newobj = obj.Properties[valueName]?.Value;
                    if (newobj != null)
                    {
                        return (T)Convert.ChangeType(newobj, typeof(T));
                    }
                }
            }

            return default(T);
        }

        /// <summary>
        /// From a list of PSObjects, get a list containing all values of the specified property value
        /// from each item in the list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objList"></param>
        /// <param name="valueName"></param>
        /// <returns></returns>
        public static List<T> GetPropertyValues<T>(PSDataCollection<PSObject> objList, string valueName)
        {
            List<T> newList = new List<T>();
            if (objList != null)
            {
                foreach (PSObject obj in objList)
                {
                    object newobj = obj.Properties[valueName]?.Value;
                    if (newobj != null)
                    {
                        newList.Add((T)Convert.ChangeType(newobj, typeof(T)));
                    }
                }
            }

            return newList;
        }

        public static void InfoEventHandler(object sender, DataAddedEventArgs e)
        {
            InformationRecord newRecord = ((PSDataCollection<InformationRecord>)sender)[e.Index];
            Log.Info(newRecord.MessageData.ToString());
        }

        public static void WarnEventHandler(object sender, DataAddedEventArgs e)
        {
            WarningRecord newRecord = ((PSDataCollection<WarningRecord>)sender)[e.Index];
            Log.Warn(newRecord.Message);
        }

        public static void ProgressEventHandler(object sender, DataAddedEventArgs e)
        {
            if (LogProgress)
            {
                ProgressRecord newRecord = ((PSDataCollection<ProgressRecord>)sender)[e.Index];
                if (newRecord.PercentComplete != -1)
                {
                    Log.Info(newRecord.PercentComplete);
                }
            }
        }

        public static void ErrorEventHandler(object sender, DataAddedEventArgs e)
        {
            ErrorRecord newRecord = ((PSDataCollection<ErrorRecord>)sender)[e.Index];
            Log.Error(newRecord.Exception, newRecord.Exception.Message);
        }

        public static void DebugEventHandler(object sender, DataAddedEventArgs e)
        {
            DebugRecord newRecord = ((PSDataCollection<DebugRecord>)sender)[e.Index];
            Log.Debug(newRecord.Message);
        }

        public static void VerboseEventHandler(object sender, DataAddedEventArgs e)
        {
            if (LogVerbose)
            {
                VerboseRecord newRecord = ((PSDataCollection<VerboseRecord>)sender)[e.Index];
                Log.Trace(newRecord.Message);
            }
        }

        public async static Task<string> ReadResourceTextAsync(string scriptResource)
        {
            var assembly = Assembly.GetExecutingAssembly();

            string script = string.Empty;

            try
            {
                using (Stream stream = assembly.GetManifestResourceStream(scriptResource))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        script = await reader.ReadToEndAsync();
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e, $"Failed to load {scriptResource}");
            }

            return script;
        }

        public async static Task RunScriptAsync(string scriptPath, string computer, bool useSSL, Credential cred)
        {
            string script = await IOHelpers.ReadFileAsync(scriptPath);
            try
            {
                using (PowerShell posh = PoshHandler.GetRunner(script, computer, useSSL, cred))
                {
                    PSDataCollection<PSObject> results = await PoshHandler.InvokeRunnerAsync(posh);
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Error running script: " + scriptPath);
            }
        }
    }
}
