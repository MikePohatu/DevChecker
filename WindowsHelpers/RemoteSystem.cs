﻿#region license
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
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Diags.Logging;
using WindowsHelpers;
using System.Management.Automation;
using System.IO;

namespace WindowsHelpers
{
    public class RemoteSystem
    {
        /// <summary>
        /// Static event. Fired when any RemoteSystem object is connected
        /// </summary>
        public static event EventHandler Connected;

        /// <summary>
        /// Is the current RemoteSystem connected
        /// </summary>
        public bool IsConnected { get; set; } = false;

        /// <summary>
        /// Is the \\device\c$ share accessible
        /// </summary>
        public bool CDollarAccessible { get; set; } = false;

        /// <summary>
        /// The computer name supplied by the user
        /// </summary>
        public string ComputerName { get; set; }

        /// <summary>
        /// The computer name reported by the device on connection
        /// </summary>
        public string ReportedComputerName { get; set; }

        /// <summary>
        /// Connect to WinRM over SSL i.e. HTTPS on port 5986
        /// </summary>
        public bool UseSSL { get; set; } = false;

        public string InstalledOSType { get; private set; } = "Unknown";
        public bool SystemPendingReboot { get; private set; } = false;
        public UInt64 SystemMemory { get; private set; } = 0;
        public UInt64 SystemMemoryMB { get { return ( SystemMemory / 1024 ) / 1024; } }

        public string IPv4Address { get; private set; }
        public string IPv6Address { get; private set; }
        public string ConfigMgrClientStatus { get; private set; } = "Unknown";
        public SortedDictionary<string, string> Properties { get; private set; }

        /// <summary>
        /// The current RemoteSystem instance. Behaves like a singleton
        /// </summary>
        public static RemoteSystem Current { get; private set; }
        private RemoteSystem(string ComputerName, bool useSSL)
        {
            this.ComputerName = ComputerName;
            this.UseSSL = useSSL;
        }

        /// <summary>
        /// Create a new RemoteSystem object and set the instance
        /// </summary>
        /// <param name="ComputerName"></param>
        /// <returns></returns>
        public static RemoteSystem New(string ComputerName, bool useSSL)
        {
            Current = new RemoteSystem(ComputerName, useSSL);
            return Current;
        }

        /// <summary>
        /// Connect the remote system and populate information
        /// </summary>
        /// <returns></returns>
        public async Task ConnectAsync()
        {
            this.IsConnected = false;
            if (string.IsNullOrWhiteSpace(this.ComputerName))
            {
                throw new ArgumentException("No computer name specified");
            }

            try
            {
                string scriptPath = AppDomain.CurrentDomain.BaseDirectory + "Scripts\\GetSystemInfo.ps1";
                string script = await IOHelpers.ReadFileAsync(scriptPath);

                using (PowerShell posh = PoshHandler.GetRunner(script, this.ComputerName, this.UseSSL))
                {
                    PSDataCollection<PSObject> results = await PoshHandler.InvokeRunnerAsync(posh, true);
                    if (results != null)
                    {
                        this.IsConnected = true;
                        this.CDollarAccessible = true;
                        this.SystemPendingReboot = PoshHandler.GetFirstHashTableValue<bool>(results, "pendingReboot");
                        this.SystemMemory = PoshHandler.GetFirstHashTableValue<ulong>(results, "memorySize");
                        this.InstalledOSType = PoshHandler.GetFirstHashTableString(results, "type");
                        this.IPv4Address = PoshHandler.GetFirstHashTableString(results, "ipv4Addresses");
                        this.IPv6Address = PoshHandler.GetFirstHashTableString(results, "ipv6Addresses");
                        this.ReportedComputerName = PoshHandler.GetFirstHashTableString(results, "name");
                        this.ConfigMgrClientStatus = PoshHandler.GetFirstHashTableString(results, "configMgrClientStatus");
                        this.Properties = PoshHandler.GetHashTableAsOrderedDictionary(results);

                        LoggerFacade.Info(LoggingHelpers.Highlight("Connected to " + this.ReportedComputerName));
                        Connected?.Invoke(this, new EventArgs());
                    }
                    else
                    {
                        LoggerFacade.Error("Couldn't gather device information for " + this.ComputerName);
                    }
                }             
            }
            catch (Exception e)
            {
                var cdollar = new DirectoryInfo(@"\\" + this.ComputerName + @"\c$");

                if (cdollar.Exists)
                {
                    this.CDollarAccessible = true;
                    LoggerFacade.Error("Error connecting to remote system using WinRM, but device appears to be up");
                }
                else
                {
                    LoggerFacade.Error("Error connecting to remote system " + this.ComputerName + ": " + e.Message);
                }                
            }
        }

        private string GetRemoteVariableValue(string Variable)
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
    }
}