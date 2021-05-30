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
using WindowsHelpers;

namespace DevChecker.Tabs
{
    /// <summary>
    /// Interaction logic for ProcessesTab.xaml
    /// </summary>
    public partial class ProcessesTab : UserControl
    {
        public ProcessesTab()
        {
            InitializeComponent();
        }

        private async void onKillClicked(object sender, RoutedEventArgs e)
        {
            RemoteProcess proc = (RemoteProcess)this.procGrid.SelectedItem;
            if (MessageBox.Show("Are you sure you want to kill "+proc.Name+"?", "Kill process", MessageBoxButton.YesNo)== MessageBoxResult.Yes)
            {
                Log.Info(Log.Highlight("Killing process " + proc.Name));
                await proc.KillAsync();
                await RemoteSystem.Current.UpdateProcessesAsync();
            }
        }

        private async void onRefreshClicked(object sender, RoutedEventArgs e)
        {
            await RemoteSystem.Current.UpdateProcessesAsync();
        }

        private void onSearchFilter(object sender, FilterEventArgs e)
        {
            var obj = e.Item as RemoteProcess;
            if (obj != null)
            {
                if (obj.Name != null && obj.Name.IndexOf(this.searchBox.Text, StringComparison.OrdinalIgnoreCase) >= 0) { e.Accepted = true; }
                else if (obj.Product != null && obj.Product.IndexOf(this.searchBox.Text, StringComparison.OrdinalIgnoreCase) >= 0) { e.Accepted = true; }
                else { e.Accepted = false; }
            }
        }

        private void onSearchBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource source = this.Resources["filteredProcs"] as CollectionViewSource;
            if (source != null)
            {
                source.View.Refresh();
            }
        }
    }
}
