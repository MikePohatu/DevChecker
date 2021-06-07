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
using ConfigMgrHelpers.Deploy;
using Core.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DevChecker.Tabs.ConfigMgr
{
    public class CmApplicatonsTableViewer : TableViewer
    {
        public CmApplicatonsTableViewer(): base()
        {
            MenuItem install = new MenuItem();
            install.Click += this.onInstallClicked;
            install.Header = "Install";
            MenuItem uninstall = new MenuItem();
            uninstall.Click += this.onUninstallClicked;
            uninstall.Header = "Uninstall";
            this.AddContextMenuItem(install);
            this.AddContextMenuItem(uninstall);
        }


        private async void onInstallClicked(object sender, RoutedEventArgs e)
        {
            var selected = (ConfigMgrHelpers.Deploy.Application)this.dataGrid.SelectedItem;
            if (MessageBox.Show("Are you sure you want to install " + selected.Name + "?", "Install application", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                await selected.InstallAsync();
            }
        }

        private async void onUninstallClicked(object sender, RoutedEventArgs e)
        {
            var selected = (ConfigMgrHelpers.Deploy.Application)this.dataGrid.SelectedItem;
            if (MessageBox.Show("Are you sure you want to uninstall " + selected.Name + "?", "Uninstall application", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                await selected.UninstallAsync();
            }
        }

        protected override async void onRefreshClicked(object sender, RoutedEventArgs e)
        {
            var sc = CmClient.Current.SoftwareCenter;
            await sc.QueryApplicationsAsync();
        }

        protected override void onSearchFilter(object sender, FilterEventArgs e)
        {
            var obj = e.Item as ConfigMgrHelpers.Deploy.Application;
            if (obj != null)
            {
                if (obj.Name != null && obj.Name.IndexOf(this.searchBox.Text, StringComparison.OrdinalIgnoreCase) >= 0) { e.Accepted = true; }
                else { e.Accepted = false; }
            }
        }
    }
}
