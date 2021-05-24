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
using Diags.Logging;
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
using _20RoadRemoteAdmin.Config;
using ConfigMgrHelpers;
using WindowsHelpers;
using System.Windows.Threading;
using CustomActions;
using System.Collections.ObjectModel;

namespace _20RoadRemoteAdmin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private static Action EmptyDelegate = delegate () { };
        private string _configFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"20Road\RemoteAdmin\conf.json");

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(object sender, string name)
        {
            PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(name));
        }

        public MainWindow()
        {
            InitializeComponent();
            LoggingHelpers.AddLoggingHandler(this.OnNewLogMessage);
            LoggerFacade.Debug("20Road Remote Admin started");
            this.StartUpAsync().ConfigureAwait(false);
        }

        private bool _clientssl = false;
        public bool ClientSSL
        {
            get { return this._clientssl; }
            set { this._clientssl = value; this.OnPropertyChanged(this, "ClientSSL"); }
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

        private string _remoteComputer;
        public string RemoteComputer
        {
            get { return this._remoteComputer; }
            set { this._remoteComputer = value; this.OnPropertyChanged(this, "RemoteComputer"); }
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

        public ObservableCollection<CustomActionScript> CustomActions
        {
            get { return ActionLibrary.PoshScripts; }
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
            if (File.Exists(this._configFilePath))
            {
                await Configuration.LoadAsync(this._configFilePath);
                this.ConfigMgrServerName = Configuration.Instance.ConfigMgrServer;
                this.RemoteComputer = Configuration.Instance.LastDevice;
                this.ClientSSL = Configuration.Instance.ClientSSL;
                this.ServerSSL = Configuration.Instance.ServerSSL;
            }
            await ActionLibrary.RefreshAsync();
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
                LoggerFacade.Error("No remote computer specified");
            }
            else
            {
                //reset and prep
                this.ControlsEnabled = false;
                this.RemoteSystem = null;
                this.CmClient = null;
                this.CmServer = null;
                this.SpinnerVisibility = Visibility.Visible;
                this._connect_Pane.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);

                //update
                await CmServer.Create(this.ConfigMgrServerName, this.ServerSSL).ConnectPoshAsync();
                RemoteSystem.New(this._remoteComputer, this._clientssl);
                LoggerFacade.Info("Connecting to device: " + RemoteSystem.Current.ComputerName);

                await RemoteSystem.Current.ConnectAsync();

                this.RemoteSystem = RemoteSystem.Current;

                var cmclient = new CmClient(RemoteSystem.Current.ComputerName, RemoteSystem.Current.UseSSL);
                    
                if (RemoteSystem.Current.ConfigMgrClientStatus != "NotInstalled" && RemoteSystem.Current.ConfigMgrClientStatus != "Unknown")
                {
                    cmclient.ClientInstalled = true;
                }
                await cmclient.QueryServerAsync();
                this.CmClient = cmclient;

                //release
                this.ControlsEnabled = true;
                this.SpinnerVisibility = Visibility.Collapsed;
            }
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
            });
        }

        private async void onWindowClosing(object sender, CancelEventArgs e)
        {
            Configuration.Instance.ConfigMgrServer = this.ConfigMgrServerName;
            Configuration.Instance.LastDevice = this.RemoteComputer;
            Configuration.Instance.ClientSSL = this.ClientSSL;
            Configuration.Instance.ServerSSL = this.ServerSSL;

            await Configuration.Instance.WriteAsync(this._configFilePath);
        }
    }
}
