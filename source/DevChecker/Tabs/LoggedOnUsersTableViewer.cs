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
    public class LoggedOnUsersTableViewer : TableViewer
    {
        public LoggedOnUsersTableViewer(): base()
        {
            MenuItem logoff = new MenuItem();
            logoff.Click += this.onLogOffClicked;
            logoff.Header = "Log user off";
            this.AddContextMenuItem(logoff);
        }


        protected override async void onRefreshClicked(object sender, RoutedEventArgs e)
        {
            await RemoteSystem.Current.UpdateLoggedOnUsersAsync();
        }

        protected override void onSearchFilter(object sender, FilterEventArgs e)
        {
            var obj = e.Item as LoggedOnUser;
            if (obj != null)
            {
                if (obj.UserName != null && obj.UserName.IndexOf(this.searchBox.Text, StringComparison.OrdinalIgnoreCase) >= 0) { e.Accepted = true; }
                else { e.Accepted = false; }
            }
        }
        private async void onLogOffClicked(object sender, RoutedEventArgs e)
        {
            var selected = (LoggedOnUser)this.DataGrid.SelectedItem;
            
            if (selected != null && MessageBox.Show("Are you sure you want to log off user " + selected.UserName + "?", "Log off user", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                await selected.LogOff();
            }
        }
    }
}
