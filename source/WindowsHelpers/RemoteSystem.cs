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
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Core.Logging;
using Core;
using WindowsHelpers;
using System.Management.Automation;
using System.IO;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace WindowsHelpers
{
    public class RemoteSystem: ViewModelBase
    {
        /// <summary>
        /// Static event. Fired when any RemoteSystem object is connected
        /// </summary>
        public static event EventHandler Connected;

        public bool IsLocalhostClient { get; set; } = false;

        /// <summary>
        /// Is the current RemoteSystem connected
        /// </summary>
        public bool IsConnected { get; set; } = false;

        private bool _processesLoading = false;
        public bool ProcessesLoading
        {
            get { return this._processesLoading; }
            set { this._processesLoading = value; this.OnPropertyChanged(this, "ProcessesLoading"); }
        }

        private bool _hotfixesLoading = false;
        public bool HotfixesLoading
        {
            get { return this._hotfixesLoading; }
            set { this._hotfixesLoading = value; this.OnPropertyChanged(this, "HotfixesLoading"); }
        }

        private bool _servicesLoading = false;
        public bool ServicesLoading
        {
            get { return this._servicesLoading; }
            set { this._servicesLoading = value; this.OnPropertyChanged(this, "ServicesLoading"); }
        }

        /// <summary>
        /// The credentials used to connect to the client device
        /// </summary>
        public Credential Credential { get; set; }

        /// <summary>
        /// Is the \\device\c$ share accessible
        /// </summary>
        public bool CDollarAccessible { get; set; } = false;

        /// <summary>
        /// The computer name supplied by the user
        /// </summary>
        public string ComputerName { get; set; }

        /// <summary>
        /// The computer name supplied by the user with any domain prefixes removed
        /// </summary>
        public string BareComputerName { get { return this.ComputerName?.Split('.')[0]; } }

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

        public string IPv4Address { get; private set; }
        public string IPv6Address { get; private set; }
        public string ConfigMgrClientStatus { get; private set; } = "Unknown";

        public SortedDictionary<string, string> Properties { get; private set; }

        /// <summary>
        /// PropertyBlocks is the properties dictionary split into blocks of 15
        /// </summary>
        public List<IDictionary<string, string>> PropertyBlocks { get; private set; }

        private List<RemoteProcess> _processes;
        public List<RemoteProcess> Processes
        {
            get { return this._processes; }
            set { this._processes = value; this.OnPropertyChanged(this, "Processes"); }
        }

        private List<RemoteService> _services;
        public List<RemoteService> Services
        {
            get { return this._services; }
            set { this._services = value; this.OnPropertyChanged(this, "Services"); }
        }

        public ObservableCollection<Hotfix> Hotfixes { get; } = new ObservableCollection<Hotfix>();

        /// <summary>
        /// The current RemoteSystem instance. Behaves like a singleton
        /// </summary>
        public static RemoteSystem Current { get; private set; }
        private RemoteSystem(string ComputerName, bool useSSL, Credential cred)
        {
            this.ComputerName = ComputerName;
            this.UseSSL = useSSL;
            this.Credential = cred;
            if (string.IsNullOrWhiteSpace(ComputerName) || ComputerName.ToLower() == "localhost" || ComputerName == "127.0.0.1")
            {
                this.IsLocalhostClient = true;
            }
        }

        /// <summary>
        /// Create a new RemoteSystem object and set the instance
        /// </summary>
        /// <param name="ComputerName"></param>
        /// <returns></returns>
        public static RemoteSystem New(string ComputerName, bool useSSL, Credential cred)
        {
            Current = new RemoteSystem(ComputerName, useSSL, cred);
            return Current;
        }

        /// <summary>
        /// Connect the remote system and populate information
        /// </summary>
        /// <returns></returns>
        public async Task ConnectAsync()
        {
            this.IsConnected = false;

            try
            {
                Log.Info("Connecting to " + this.ComputerName);
                string scriptPath = AppDomain.CurrentDomain.BaseDirectory + "Scripts\\GetSystemInfo.ps1";
                string script = await IOHelpers.ReadFileAsync(scriptPath);

                using (PowerShell posh = PoshHandler.GetRunner(script, this))
                {
                    PSDataCollection<PSObject> results = await PoshHandler.InvokeRunnerAsync(posh, true);
                    if (results != null)
                    {
                        this.IsConnected = true;
                        this.CDollarAccessible = true;
                        this.ConfigMgrClientStatus = PoshHandler.GetFirstHashTableString(results, "ConfigMgrClientStatus");
                        this.SystemPendingReboot = PoshHandler.GetFirstHashTableValue<bool>(results, "PendingReboot");
                        this.InstalledOSType = PoshHandler.GetFirstHashTableString(results, "Type");
                        this.IPv4Address = PoshHandler.GetFirstHashTableString(results, "IPv4Addresses");
                        this.IPv6Address = PoshHandler.GetFirstHashTableString(results, "IPv6Addresses");
                        this.ReportedComputerName = PoshHandler.GetFirstHashTableString(results, "Name");
                        this.Properties = PoshHandler.GetFromHashTableAsOrderedDictionary(results);
                        this.PropertyBlocks = Overflow.CreateFromDictionary(this.Properties, 12);
                        
                        Log.Info(Log.Highlight("Connected to " + this.ReportedComputerName));
                        Connected?.Invoke(this, new EventArgs());
                    }
                    else
                    {
                        Log.Error("Couldn't gather device information for " + this.ComputerName);
                    }
                }             
            }
            catch (Exception e)
            {
                var cdollar = new DirectoryInfo(@"\\" + this.ComputerName + @"\c$");

                if (cdollar.Exists)
                {
                    this.CDollarAccessible = true;
                    Log.Error("Error connecting to remote system using WinRM, but device appears to be up");
                }
                else
                {
                    Log.Error("Error connecting to remote system " + this.ComputerName + ": " + e.Message);
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

        public void OpenCDollar()
        {
            string path = @"\\" + RemoteSystem.Current.ComputerName + @"\c$";
            try
            {
                Process.Start(path);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error opening " + path);
            }
        }

        public void OpenMstsc()
        {
            try
            {
                Process.Start("mstsc.exe", "/v:" + this.ComputerName);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error opening mstsc");
            }
        }

        public void OpenCompMgmt()
        {
            try
            {
                if (this.Credential != null && this.Credential.CredentialsSet)
                {
                    ImpersonationHelpers.Impersonate(this.Credential.Domain, this.Credential.Username, this.Credential.Password, delegate
                    {
                        this.StartProcess(@"C:\Windows\System32\mmc.exe", @"c:\windows\system32\compmgmt.msc /computer:\\" + this.ComputerName, null);
                    });
                }
                else
                {
                    this.StartProcess(@"C:\Windows\System32\mmc.exe", @"c:\windows\system32\compmgmt.msc /computer:\\" + this.ComputerName, null);
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Error opening Computer Management");
            }
        }

        public void OpenPosh()
        {
            try
            {
                string ssl = this.UseSSL ? " -UseSSL" : "";
                string command = @"C:\Windows\System32\WindowsPowershell\v1.0\powershell.exe";
                string args = " -noexit -command \"Enter-PSSession -ComputerName " + this.ComputerName + ssl + "\"";
                this.StartProcess(command, args, this.Credential);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error opening remote PowerShell session");
            }
        }

        public async Task UpdateProcessesAsync()
        {
            this.ProcessesLoading = true;
            this.Processes = null;
            List<RemoteProcess> procs = new List<RemoteProcess>();
            string script = RemoteProcess.GetScript;

            try
            {
                using (PowerShell posh = PoshHandler.GetRunner(script, this))
                {
                    var results = await PoshHandler.InvokeRunnerAsync(posh);
                    foreach (var result in results)
                    {
                        procs.Add(RemoteProcess.Create(result));
                    }
                }
                procs.Sort();
                this.Processes = procs;
            }
            catch (Exception e)
            {
                Log.Error(e, "Error getting process information");
            }
            this.ProcessesLoading = false;
        }

        public async Task UpdateServicesAsync()
        {
            this.ServicesLoading = true;
            try
            {
                this.Services = null;
                List<RemoteService> services = new List<RemoteService>();
                string script = RemoteService.GetScript;
                using (PowerShell posh = PoshHandler.GetRunner(script, this))
                {
                    var results = await PoshHandler.InvokeRunnerAsync(posh);
                    foreach (var result in results)
                    {
                        services.Add(RemoteService.Create(result));
                    }
                }
                services.Sort();
                this.Services = services;
            }
            catch (Exception e)
            {
                Log.Error(e, "Error getting service information");
            }

            this.ServicesLoading = false;
        }

        public async Task UpdateHotfixesAsync()
        {
            this.HotfixesLoading = true;
            try
            {
                this.Hotfixes.Clear();
                string script = Hotfix.GetterCommand;
                using (PowerShell posh = PoshHandler.GetRunner(script, this))
                {
                    var results = await PoshHandler.InvokeRunnerAsync(posh);
                    foreach (var result in results)
                    {
                        this.Hotfixes.Add(Hotfix.New(result));
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Error getting hotfix information");
            }
            this.HotfixesLoading = false;
        }

        public async Task GpUpdateAsync()
        {
            try
            {
                //string script = "Start-Process 'gpupdate.exe' '/force' -Wait";
                string script = "(gpupdate /force) | Foreach-Object { Write-Information $_ }";

                using (PowerShell posh = PoshHandler.GetRunner(script, this))
                {
                    await PoshHandler.InvokeRunnerAsync(posh);
                    Log.Info("Done");
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Error running gpupdate");
            }
        }

        public async Task ShutdownAsync()
        {
            try
            {
                string script = "Stop-Computer -Force";

                using (PowerShell posh = PoshHandler.GetRunner(script, this))
                {
                    await PoshHandler.InvokeRunnerAsync(posh);
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Error running shutdown");
            }
        }

        public async Task RebootAsync()
        {
            try
            {
                string script = "Restart-Computer -Force";

                using (PowerShell posh = PoshHandler.GetRunner(script, this))
                {
                    await PoshHandler.InvokeRunnerAsync(posh);
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Error running restart");
            }
        }

        public void StartProcess(string file, string args, Credential creds)
        {
            ProcessStartInfo info = string.IsNullOrWhiteSpace(args) ? new ProcessStartInfo(file) : new ProcessStartInfo(file, args);
            info.UseShellExecute = false;

            if (creds != null && creds.CredentialsSet)
            {
                info.UserName = this.Credential.Username;
                if (!this.Credential.Username.Contains("@")) { info.Domain = this.Credential.Domain; }
                info.Password = this.Credential.SecurePassword;
            }

            Process.Start(info);
        }
    }
}
