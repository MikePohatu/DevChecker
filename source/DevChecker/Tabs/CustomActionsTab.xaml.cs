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
using Core;
using Core.Logging;
using CustomActions;
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
    /// Interaction logic for CustomActionScriptsTab.xaml
    /// </summary>
    public partial class CustomActionsTab : UserControl
    {
        public CustomActionsTab()
        {
            InitializeComponent();
        }

        private async void onRefreshClicked(object sender, RoutedEventArgs e)
        {
            await this.Refresh();
        }

        public async void onRunActionClicked(object sender, RoutedEventArgs e)
        {
            var uiElement = sender as FrameworkElement;
            var component = uiElement?.DataContext as CustomActionScript;
            bool success = await component?.RunActionAsync();

            if (success && component.Settings.DisplayElement == DisplayElements.Modal)
            {
                this.ShowModal(component);
            }            
        }

        private void ShowModal(CustomActionScript actionScript)
        {
            var modal = new Modal();

            string header = actionScript.DisplayName;
            if (string.IsNullOrWhiteSpace(actionScript.Settings.Description) == false) { header = header + " : " + actionScript.Settings.Description; }
            var group = new GroupBox();
            group.Margin = new Thickness(5);
            group.Padding = new Thickness(5);
            group.Header = header;
            group.Content = GetControl(actionScript);

            modal.Content = group;
            Application.Current.MainWindow.Closing += (o, args) => { modal.Close(); };
            modal.Show();
        }

        public async Task Refresh()
        {
            await ActionLibrary.Instance.RefreshAsync();
            //clear our the tabs

            this.tabs.Items.Clear();
            this.tabs.Items.Add(this.actionsTabItem);

            //var dbg = ActionLibrary.Instance;
            foreach (CustomActionScript actionScript in ActionLibrary.Instance.Tabs)
            {
                TabItem tab = new TabItem();
                tab.Header = actionScript.DisplayName;
                if (string.IsNullOrWhiteSpace(actionScript.Settings.Description) == false) { tab.ToolTip = actionScript.Settings.Description; }
                UIElement viewer = GetControl(actionScript);
                tab.Content = viewer;
                this.tabs.Items.Add(tab);
            }
        }

        private UIElement GetControl(CustomActionScript actionScript)
        {
            if (actionScript.Settings.OutputType == OutputTypes.Text)
            {
                TextBlock tb = new TextBlock();
                StringBuilder sb = new StringBuilder();
                if (actionScript.ResultList != null)
                {
                    foreach (var obj in actionScript.ResultList)
                    {
                        sb.AppendLine(PoshHandler.ToOutputString(obj));
                    }
                }
                
                tb.Text = sb.ToString();
                return tb;
            }
            else if (actionScript.Settings.OutputType == OutputTypes.Object)
            {
                ObjectViewer oviewer = new ObjectViewer();
                var results = PoshHandler.GetFromHashTableAsOrderedDictionary(actionScript.ResultList);
                oviewer.ObjectSource = Overflow.CreateFromDictionary(results, actionScript.Settings.MaxRowsPerColumn);
                return oviewer;
            }
            else if (actionScript.Settings.OutputType == OutputTypes.List)
            {
                return new CustomScriptTableViewer(actionScript);
            }
            else
            {
                Log.Error("DisplayElement set to Tab or Modal but OutputType set to None. Script: " + actionScript.Name);
                return null;
            }
        }
    }
}
