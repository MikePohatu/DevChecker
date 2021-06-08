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
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WindowsHelpers;

namespace DevChecker.Tabs
{
    public class HotfixesTableViewer : TableViewer
    {
        public HotfixesTableViewer(): base()
        {
            MenuItem unist = new MenuItem();
            unist.Click += this.onUninstallClicked;
            unist.Header = "Uninstall";
            this.AddContextMenuItem(unist);
        }


        protected override async void onRefreshClicked(object sender, RoutedEventArgs e)
        {
            await RemoteSystem.Current.UpdateHotfixesAsync();
        }

        protected override void onSearchFilter(object sender, FilterEventArgs e)
        {
            var obj = e.Item as Hotfix;
            if (obj != null)
            {
                if (obj.HotFixID != null && obj.HotFixID.IndexOf(this.searchBox.Text, StringComparison.OrdinalIgnoreCase) >= 0) { e.Accepted = true; }
                else { e.Accepted = false; }
            }
        }
        private async void onUninstallClicked(object sender, RoutedEventArgs e)
        {
            var selected = (Hotfix)this.DataGrid.SelectedItem;
            if (selected != null && MessageBox.Show("Are you sure you want to uninstall " + selected.HotFixID + "?", "Uninstall hotfix", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                await selected.UninstallAsync();
            }
        }
    }
}
