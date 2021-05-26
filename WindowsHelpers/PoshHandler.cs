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
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using Diags.Logging;
using System.Collections;
using Diags;
using System.Text;

namespace WindowsHelpers
{
    public static class PoshHandler
    {
        public static bool LogVerbose { get; set; } = false;
        public static bool LogProgress { get; set; } = false;

        private static PowerShell GetRunner(string script, string computerName, bool useSSL, int port)
        {
            Runspace runspace;

            if (string.IsNullOrWhiteSpace(computerName) || computerName == "." || computerName == "localhost" || computerName == "127.0.0.1")
            {
                runspace = RunspaceFactory.CreateRunspace();
                runspace.Open();
            }
            else { 
                string shellUri = "http://schemas.microsoft.com/powershell/Microsoft.PowerShell";
                //PSCredential remoteCredential = new PSCredential(domainAndUsername, securePW);
                PSCredential currentCred = PSCredential.Empty;
                WSManConnectionInfo connectioninfo = new WSManConnectionInfo(useSSL, computerName, port, "/wsman", shellUri, currentCred);

                runspace = RunspaceFactory.CreateRunspace(connectioninfo);
                runspace.Open();
            }
                
            runspace.CreatePipeline();
            PowerShell posh = GetRunner();
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
            PowerShell posh = GetRunner().AddScript(script);
            return posh;
        }

        public static PowerShell GetRunner(string computerName, bool useSSL)
        {
            return GetRunner(null, computerName, useSSL);
        }

        public static PowerShell GetRunner(string script, string computerName, bool useSSL)
        {
            int port = useSSL ? 5986 : 5985;
            return GetRunner(script, computerName, useSSL, port);
        }

        public static PowerShell GetRunner()
        {
            PowerShell posh = PowerShell.Create();

            posh.Streams.Warning.DataAdded += WarnEventHandler;
            posh.Streams.Error.DataAdded += ErrorEventHandler;
            posh.Streams.Information.DataAdded += InfoEventHandler;
            posh.Streams.Verbose.DataAdded += VerboseEventHandler;
            posh.Streams.Progress.DataAdded += ProgressEventHandler;

            return posh;
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

        public static SortedDictionary<string, string> GetHashTableAsOrderedDictionary(PSDataCollection<PSObject> objList)
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

        public async static Task RunScriptAsync(string scriptPath, string computer, bool useSSL)
        {
            string script = await IOHelpers.ReadFileAsync(scriptPath);
            try
            {
                using (PowerShell posh = PoshHandler.GetRunner(script, computer, useSSL))
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
