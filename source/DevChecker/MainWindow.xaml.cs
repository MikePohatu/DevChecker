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
using Core.Logging;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;
using DevChecker.Config;
using ConfigMgrHelpers;
using WindowsHelpers;
using System.Windows.Threading;
using CustomActions;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using Core;
using System.Reflection;

namespace DevChecker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private static Action EmptyDelegate = delegate () { };
        private string _configFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"20Road\DevChecker\conf.json");

        public event PropertyChangedEventHandler PropertyChanged;


        Credential _clientCred;
        Credential _serverCred;

        protected void OnPropertyChanged(object sender, string name)
        {
            PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(name));
        }

        public MainWindow()
        {
            string name = Assembly.GetEntryAssembly().GetName().Name;
            string version = Assembly.GetEntryAssembly().GetName().Version.ToString();
            this.Title = name + "  :  v" + version;
            InitializeComponent();
            LoggingHelpers.AddLoggingHandler(this.OnNewLogMessage);
            Log.Info(name + " " + version + " started");
            this._clientCred = new Credential();
            this._serverCred = new Credential();

            this.StartUpAsync().ConfigureAwait(false);
        }

        public Visibility RelaunchVisibility { get { return UacHelpers.IsAdministrator() ? Visibility.Collapsed : Visibility.Visible; } }

        private bool _clientssl = false;
        public bool ClientSSL
        {
            get { return this._clientssl; }
            set { this._clientssl = value; this.OnPropertyChanged(this, "ClientSSL"); }
        }

        private string _remoteComputer;
        public string RemoteComputer
        {
            get { return this._remoteComputer; }
            set { this._remoteComputer = value; this.OnPropertyChanged(this, "RemoteComputer"); }
        }

        private ObservableCollection<string> _deviceHistory = new ObservableCollection<string>();
        public ObservableCollection<string> DeviceHistory
        {
            get { return this._deviceHistory; }
            set 
            { 
                this._deviceHistory = value; 
                this.OnPropertyChanged(this, "DeviceHistory");
                this.RemoteComputer = this._deviceHistory.First();
            }
        }

        private bool _serverssl = true;
        public bool ServerSSL
        {
            get { return this._serverssl; }
            set { this._serverssl = value; this.OnPropertyChanged(this, "ServerSSL"); }
        }

        private string _configmgrServerName;
        public string ConfigMgrServerName
        {
            get { return this._configmgrServerName; }
            set { this._configmgrServerName = value; this.OnPropertyChanged(this, "ConfigMgrServerName"); }
        }

        private bool _connectEnabled = true;
        public bool ControlsEnabled
        {
            get { return this._connectEnabled; }
            set { this._connectEnabled = value; this.OnPropertyChanged(this, "ControlsEnabled"); }
        }
        private RemoteSystem _remotesystem;
        public RemoteSystem RemoteSystem
        {
            get { return this._remotesystem; }
            set { this._remotesystem = value; this.OnPropertyChanged(this, "RemoteSystem"); }
        }

        private CmClient _cmclient;
        public CmClient CmClient
        {
            get { return this._cmclient; }
            set { this._cmclient = value; this.OnPropertyChanged(this, "CmClient"); }
        }

        private CmServer _cmserver;
        public CmServer CmServer
        {
            get { return this._cmserver; }
            set { this._cmserver = value; this.OnPropertyChanged(this, "CmServer"); }
        }

        public ActionLibrary ActionLibrary
        {
            get { return ActionLibrary.Instance; }
        }

        private Visibility _spinnervisibility = Visibility.Collapsed;
        public Visibility SpinnerVisibility
        {
            get { return this._spinnervisibility; }
            set { this._spinnervisibility = value; this.OnPropertyChanged(this, "SpinnerVisibility"); }
        }

        public bool LogVerbose
        {
            get { return PoshHandler.LogVerbose; }
            set
            {
                PoshHandler.LogVerbose = value;
                if (value == true)
                {
                    LoggingHelpers.SetLoggingLevel(LogLevel.Trace);
                }
                else
                {
                    LoggingHelpers.SetLoggingLevel(LogLevel.Info);
                }
                Log.Debug("Log level updated");
            }
        }

        public bool LogProgress
        {
            get { return PoshHandler.LogProgress; }
            set
            {
                PoshHandler.LogProgress = value;
            }
        }

        private async Task StartUpAsync()
        {
            await Configuration.LoadAsync(this._configFilePath);
            this.ConfigMgrServerName = Configuration.Instance.ConfigMgrServer;
            this.RemoteComputer = Configuration.Instance.LastDevice;
            if (Configuration.Instance.DeviceHistory != null && Configuration.Instance.DeviceHistory.Count > 0)
            {
                this.DeviceHistory = new ObservableCollection<string>(Configuration.Instance.DeviceHistory);
            }
            this.ClientSSL = Configuration.Instance.ClientSSL;
            this.ServerSSL = Configuration.Instance.ServerSSL;
            this._clientCred.Username = Configuration.Instance.ClientUsername;
            this._clientCred.Domain = Configuration.Instance.ClientDomain;
            this._serverCred.Username = Configuration.Instance.ServerUsername;
            this._serverCred.Domain = Configuration.Instance.ServerDomain;
            this._serverCred.UseKerberos = Configuration.Instance.ServerKerberos;
            this._clientCred.UseKerberos = Configuration.Instance.ClientKerberos;

            await this.customActionsTab.Refresh();
        }

        private async void onConnectClick(object sender, RoutedEventArgs e)
        {
            await this.ConnectAsync();
        }

        private async void onConnectionKeyup(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.Enter)
            {
                e.Handled = true;
                await this.ConnectAsync();
            }
        }

        private async Task ConnectAsync()
        {
            if (string.IsNullOrWhiteSpace(RemoteComputer))
            {
                this.RemoteSystem = null;
                this.CmClient = null;
                Log.Error("No remote computer specified");
            }
            else
            {
                string bars = "----------------------------------------";
                Log.Info("Connecting to " + this._remoteComputer + Environment.NewLine + bars);
                //reset and prep
                this.ControlsEnabled = false;
                this.RemoteSystem = null;
                this.CmClient = null;
                this.CmServer = null;
                this.SpinnerVisibility = Visibility.Visible;
                this._connectPane.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);

                //update
                RemoteSystem.New(this._remoteComputer, this._clientssl, this._clientCred);

                List<Task> connectTasks = new List<Task>();
                connectTasks.Add(CmServer.Create(this.ConfigMgrServerName, this.ServerSSL, RemoteSystem.Current.BareComputerName, this._serverCred).ConnectAsync());
                connectTasks.Add(RemoteSystem.Current.ConnectAsync());
                await Task.WhenAll(connectTasks);

                this.CmServer = CmServer.Current;
                this.RemoteSystem = RemoteSystem.Current;

                if (this.RemoteSystem != null) {
                    this.UpdateDeviceHistory(this._remoteComputer);
                }

                //release
                this.ControlsEnabled = true;
                this.SpinnerVisibility = Visibility.Collapsed;

                //further updates
                List<Task> postconnectTasks = new List<Task>();
                if (this.RemoteSystem.IsConnected)
                {
                    postconnectTasks.Add(this.PostConnectCmClient());
                    postconnectTasks.Add(this.RemoteSystem.UpdateProcessesAsync());
                    postconnectTasks.Add(this.RemoteSystem.UpdateServicesAsync());
                    postconnectTasks.Add(this.RemoteSystem.UpdateHotfixesAsync());
                    postconnectTasks.Add(this.RemoteSystem.UpdatePrintersAsync());
                    postconnectTasks.Add(this.RemoteSystem.UpdatePrintDriversAsync());
                }
                
                if (CmServer.Current.IsConnected) 
                { 
                    postconnectTasks.Add(CmServer.Current.Client.QueryCollectionsAsync());
                    postconnectTasks.Add(CmServer.Current.QueryScripts());
                }

                await Task.WhenAll(postconnectTasks);
                Log.Info(Log.Highlight("Finished loading data"));
            }
        }

        private async Task PostConnectCmClient()
        {
            var cmclient = CmClient.New();

            if (RemoteSystem.Current.IsConnected && RemoteSystem.Current.ConfigMgrClientStatus != "NotInstalled" && RemoteSystem.Current.ConfigMgrClientStatus != "Unknown")
            {
                await cmclient.QueryAsync();
            }

            this.CmClient = cmclient;
        }

        public void OnNewLogMessage(LogLevel loglevel, string message, bool ishighlighted)
        {
            if (string.IsNullOrWhiteSpace(message)) { return; }
            this.Dispatcher.Invoke(() =>
            {
                Run loglevevlrun = new Run($"{loglevel.ToString()} | ");
                Run messagerun = new Run($"{message}\n");

                if (loglevel == LogLevel.Fatal)
                {
                    loglevevlrun.Foreground = Brushes.Red;
                }
                else if (loglevel == LogLevel.Error)
                {
                    loglevevlrun.Foreground = Brushes.Red;
                }
                else if (loglevel == LogLevel.Warn)
                {
                    loglevevlrun.Foreground = Brushes.DarkOrange;
                }
                else if (ishighlighted)
                {
                    loglevevlrun.Foreground = Brushes.Green;
                }

                if (ishighlighted)
                {
                    loglevevlrun.FontWeight = FontWeights.Bold;
                    messagerun.FontWeight = FontWeights.Bold;
                }

                this.outputTb.Inlines.Add(loglevevlrun);
                this.outputTb.Inlines.Add(messagerun);
                this.outputScroll.ScrollToBottom();
            });
        }

        private async void onWindowClosing(object sender, CancelEventArgs e)
        {
            Configuration.Instance.ConfigMgrServer = this.ConfigMgrServerName;
            Configuration.Instance.LastDevice = this.RemoteComputer;
            int historykeepcount = this.DeviceHistory.Count > 20 ? 20 : this.DeviceHistory.Count;
            Configuration.Instance.DeviceHistory = this.DeviceHistory.ToList().GetRange(0, historykeepcount);
            Configuration.Instance.ClientSSL = this.ClientSSL;
            Configuration.Instance.ServerSSL = this.ServerSSL;
            Configuration.Instance.ClientKerberos = this._clientCred.UseKerberos;
            Configuration.Instance.ServerKerberos = this._serverCred.UseKerberos;

            Configuration.Instance.ClientUsername = this._clientCred.Username;
            Configuration.Instance.ClientDomain = this._clientCred.Domain;
            Configuration.Instance.ServerUsername = this._serverCred.Username;
            Configuration.Instance.ServerDomain = this._serverCred.Domain;

            await Configuration.Instance.WriteAsync(this._configFilePath);
        }

        private void onSaveLogClicked(object sender, RoutedEventArgs e)
        {
            string log = this.outputTb.Text;
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.AddExtension = true;
            saveFileDialog.FileName = "DevChecker-"+ DateTime.Now.ToString("ddMMMyyyy-HHmmss") + ".log";
            saveFileDialog.Filter = "Log Files (*.log)|*.log|Text Files(*.txt)|*.txt";
            saveFileDialog.Title = "Save DevChecker log file";
            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, log);
            } 
        }

        private void onClientConnectAsClicked(object sender, RoutedEventArgs e)
        {

            var popup = new CredentialPopup(this._clientCred);
            popup.ShowDialog();
        }

        private void onServerConnectAsClicked(object sender, RoutedEventArgs e)
        {
            var popup = new CredentialPopup(this._serverCred);
            popup.ShowDialog();
        }

        private void onAdminClicked(object sender, RoutedEventArgs e)
        {
            UacHelpers.RestartToAdmin();
        }

        private void UpdateDeviceHistory(string newdevice)
        {
            this.DeviceHistory.Insert(0, newdevice);
            var enumerator = this.DeviceHistory.GetEnumerator();
            int index = 0;
            List<int> toremove = new List<int>();

            while (enumerator.MoveNext())
            {
                if (enumerator.Current == newdevice && index != 0)
                {
                    toremove.Add(index);
                }
                index++;
            }

            foreach (int i in toremove) { this.DeviceHistory.RemoveAt(i); }
            this.clientBox.SelectedIndex = 0;
        }
    }
}
