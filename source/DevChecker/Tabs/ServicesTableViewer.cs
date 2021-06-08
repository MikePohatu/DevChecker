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
    public class ServicesTableViewer : TableViewer
    {
        public ServicesTableViewer(): base()
        {
            MenuItem item = new MenuItem();
            item.Click += this.onRestartClicked;
            item.Header = "Restart";
            this.AddContextMenuItem(item);

            item = new MenuItem();
            item.Click += this.onStartClicked;
            item.Header = "Start";
            this.AddContextMenuItem(item);

            item = new MenuItem();
            item.Click += this.onStopClicked;
            item.Header = "Stop";
            this.AddContextMenuItem(item);

            
        }

        private async void onStartClicked(object sender, RoutedEventArgs e)
        {
            RemoteService selected = (RemoteService)this.DataGrid.SelectedItem;
            if (selected != null && MessageBox.Show("Are you sure you want to start " + selected.Name + "?", "Start service", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Log.Info(Log.Highlight("Starting service " + selected.Name));
                await selected.StartServiceAsync();
            }
        }

        private async void onRestartClicked(object sender, RoutedEventArgs e)
        {
            RemoteService selected = (RemoteService)this.DataGrid.SelectedItem;
            if (selected != null && MessageBox.Show("Are you sure you want to restart " + selected.Name + "?", "Restart service", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Log.Info(Log.Highlight("Restarting service " + selected.Name));
                await selected.RestartServiceAsync();
            }
        }

        private async void onStopClicked(object sender, RoutedEventArgs e)
        {
            RemoteService selected = (RemoteService)this.DataGrid.SelectedItem;
            if (selected != null && MessageBox.Show("Are you sure you want to stop " + selected.Name + "?", "Stop service", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Log.Info(Log.Highlight("Stopping service " + selected.Name));
                await selected.StopServiceAsync();
            }
        }

        protected override async void onRefreshClicked(object sender, RoutedEventArgs e)
        {
            await RemoteSystem.Current.UpdateServicesAsync();
        }
        protected override void onSearchFilter(object sender, FilterEventArgs e)
        {
            var obj = e.Item as RemoteService;
            if (obj != null)
            {
                if (obj.DisplayName != null && obj.DisplayName.IndexOf(this.searchBox.Text, StringComparison.OrdinalIgnoreCase) >= 0) { e.Accepted = true; }
                else if (obj.Name != null && obj.Name.IndexOf(this.searchBox.Text, StringComparison.OrdinalIgnoreCase) >= 0) { e.Accepted = true; }
                else { e.Accepted = false; }
            }
        }
    }
}
