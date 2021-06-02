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
using CustomActions;
using WindowsHelpers;

namespace DevChecker.Tabs
{
    /// <summary>
    /// Interaction logic for CustomActionTab.xaml
    /// </summary>
    public partial class ActionsTab : UserControl
    {
        public ActionsTab()
        {
            InitializeComponent();
        }

        public async void onRunActionClicked(object sender, RoutedEventArgs e)
        {
            var uiElement = sender as FrameworkElement;
            var component = uiElement?.DataContext as CustomActionScript;
            await component?.RunActionAsync();
        }

        public async void onRefreshClicked(object sender, RoutedEventArgs e)
        {
            await ActionLibrary.RefreshAsync();
        }

        public void onCDollorClicked(object sender, RoutedEventArgs e)
        {
            RemoteSystem.Current?.OpenCDollar();
        }

        public async void onGPUpdateClicked(object sender, RoutedEventArgs e)
        {
            await RemoteSystem.Current?.GpUpdateAsync();
        }

        public void onCompMgmtClicked(object sender, RoutedEventArgs e)
        {
            RemoteSystem.Current?.OpenCompMgmt();
        }

        public void onMstscClicked(object sender, RoutedEventArgs e)
        {
            RemoteSystem.Current?.OpenMstsc();
        }

        public void onPoshClicked(object sender, RoutedEventArgs e)
        {
            RemoteSystem.Current?.OpenPosh();
        }

        private async void onCheckHealthClick(object sender, RoutedEventArgs e)
        {
            await RepairTools.RunCheckHealthAsync();
        }

        private async void onScanHealthClick(object sender, RoutedEventArgs e)
        {
            await RepairTools.RunScanHealthAsync();
        }

        private async void onRestoreHealthClick(object sender, RoutedEventArgs e)
        {
            await RepairTools.RunRestoreHealthAsync();
        }

        public async void onShutdownClicked(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want shutdown " + RemoteSystem.Current.ComputerName + "?", "Shutdown " + RemoteSystem.Current.ComputerName, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                await RemoteSystem.Current?.ShutdownAsync();
            }
        }

        public async void onRebootClicked(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want reboot " + RemoteSystem.Current.ComputerName + "?", "Reboot " + RemoteSystem.Current.ComputerName, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                await RemoteSystem.Current?.RebootAsync();
            }
        }

        public async void onSfcScanNowClick(object sender, RoutedEventArgs e)
        {
            await RepairTools.RunSfcScanNowAsync();
        }
        
    }
}
