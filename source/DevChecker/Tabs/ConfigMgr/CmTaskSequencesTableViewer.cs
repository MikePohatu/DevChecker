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
    public class CmTaskSequencesTableViewer: TableViewer
    {
        public CmTaskSequencesTableViewer(): base()
        {
            MenuItem run = new MenuItem();
            run.Click += this.onRunClicked;
            run.Header = "Run";

            this.AddContextMenuItem(run);
        }        


        private async void onRunClicked(object sender, RoutedEventArgs e)
        {
            var selected = (TaskSequence)this.dataGrid.SelectedItem;
            if (MessageBox.Show("Are you sure you want to run " + selected.Name + "?", "Run Task Sequence", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Log.Info(Log.Highlight("Run task sequence " + selected.Name));
                await selected.RunAsync();
                await CmClient.Current.SoftwareCenter.QueryTaskSequencesAsync();
            }
        }

        protected override async void onRefreshClicked(object sender, RoutedEventArgs e)
        {
            var sc = CmClient.Current.SoftwareCenter;
            await sc.QueryTaskSequencesAsync();
        }

        protected override void onSearchFilter(object sender, FilterEventArgs e)
        {
            var obj = e.Item as TaskSequence;
            if (obj != null)
            {
                if (obj.Name != null && obj.Name.IndexOf(this.searchBox.Text, StringComparison.OrdinalIgnoreCase) >= 0) { e.Accepted = true; }
                else { e.Accepted = false; }
            }
        }
    }
}
