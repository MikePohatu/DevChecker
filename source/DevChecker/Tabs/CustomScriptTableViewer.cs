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
using CustomActions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using WindowsHelpers;

namespace DevChecker.Tabs
{
    public class CustomScriptTableViewer : TableViewer
    {
        private CustomActionScript _action;
        public CustomScriptTableViewer(CustomActionScript action): base()
        {
            this._action = action;

            Binding databinding = new Binding("TableData");
            databinding.Source = action;
            this.SetBinding(TableViewer.TableSourceProperty, databinding);

            Binding runningbinding = new Binding("IsRunning");
            runningbinding.Source = action;
            this.SetBinding(TableViewer.IsLoadingProperty, runningbinding);

            this.IsSearchEnabled = action.Settings.FilterProperties.Count < 1 ? false : true;

            this._action.RunCompleted += onFirstActionRun;
        }

        //onFirstActionRun and onFirstFocus are to work around an issue where the column sort doesn't work.
        //The EnableSorting method only works after the DataGrid is focused. Before that it doesn't seem like
        //the columns are built yet, so you can't enable sorting on them. This waits for first action run, the sets
        //up the first focus to run the enable sorting.
        private void onFirstActionRun(object sender, EventArgs e)
        {
            this._action.RunCompleted -= onFirstActionRun;
            this.MouseMove += this.onFirstFocus;
        }

        private void onFirstFocus(object sender, RoutedEventArgs e)
        {
            this.MouseMove -= this.onFirstFocus;
            this.EnableSorting();
        }


        protected override async void onRefreshClicked(object sender, RoutedEventArgs e)
        {
            await this._action.RunActionAsync();
        }

        protected override void onSearchFilter(object sender, FilterEventArgs e)
        {
            if (this._action?.Settings?.FilterProperties == null || string.IsNullOrEmpty(this.searchBox.Text)) { e.Accepted = true; return; }

            var obj = e.Item;
            if (this._action.Settings.FilterProperties.Count > 0)
            {
                foreach (string prop in this._action.Settings.FilterProperties)
                {
                    string val;
                    var poshobj = obj as PSObject;
                    if (poshobj != null)
                    {
                        val = PoshHandler.GetPropertyValue<string>(poshobj, prop);
                        if (val != null && val.IndexOf(this.searchBox.Text, StringComparison.OrdinalIgnoreCase) >= 0) { e.Accepted = true; return; }
                    }
                    else
                    {
                        Type type = obj.GetType();
                        System.Reflection.PropertyInfo info = type.GetProperty(prop);
                        if (info != null)
                        {
                            val = info.GetValue(obj, null).ToString();
                            if (val != null && val.IndexOf(this.searchBox.Text, StringComparison.OrdinalIgnoreCase) >= 0) { e.Accepted = true; return; }
                        }
                        //prop not found.
                        else
                        {
                            e.Accepted = true; return;
                        }
                    }                    
                }
            }
            
            e.Accepted = false;
        }
    }
}
