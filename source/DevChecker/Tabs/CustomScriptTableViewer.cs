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
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WindowsHelpers;

namespace DevChecker.Tabs
{
    public class CustomScriptTableViewer : TableViewer
    {
        private CustomActionScript _action;
        public CustomScriptTableViewer(CustomActionScript action) : base()
        {
            this._action = action;
            this.TableSource = action.TableData;
            this.IsSearchEnabled = action.Settings.FilterProperties.Count < 1 ? false : true;
        }

        protected override async void onRefreshClicked(object sender, RoutedEventArgs e)
        {
            this.IsLoading = true;
            await this._action.RunActionAsync();
            this.IsLoading = false;
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
