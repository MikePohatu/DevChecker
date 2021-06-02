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
using ConfigMgrHelpers;
using Core.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shapes;
using WindowsHelpers;

namespace DevChecker.Tabs.ConfigMgr
{
    /// <summary>
    /// Interaction logic for CmSystemActions.xaml
    /// </summary>
    public partial class CmSystemActions : UserControl
    {
        private RemoteService _remoteService = new RemoteService() { Name = "ccmexec" };
        public CmSystemActions()
        {
            InitializeComponent();
        }

        private async void onRestartClicked(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to restart the ConfigMgr client service?", "Restart ConfigMgr Client", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                await _remoteService.RestartServiceAsync();
            }
        }

        public void onLogsClicked(object sender, RoutedEventArgs e)
        {
            CmClient.Current.OpenLogs();
        }

        public void onSetupLogsClicked(object sender, RoutedEventArgs e)
        {
            CmClient.Current.OpenSetupLogs();
        }

        public async void onRepairClicked(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to run ConfigMgr client repair?", "ConfigMgr Client Repair", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                await CmClient.Current.RepairClientAsync();
            }
        }
    }
}
