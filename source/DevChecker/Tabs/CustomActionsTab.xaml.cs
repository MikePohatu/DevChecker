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
            await component?.RunActionAsync();
        }

        public async Task Refresh()
        {
            await ActionLibrary.Instance.RefreshAsync();
            //clear our the tabs

            this.tabs.Items.Clear();
            this.tabs.Items.Add(this.actionsTabItem);

            //var dbg = ActionLibrary.Instance;
            foreach (CustomActionScript script in ActionLibrary.Instance.Tabs)
            {
                TabItem tab = new TabItem();
                tab.Header = script.DisplayName;
                CustomScriptTableViewer viewer = new CustomScriptTableViewer(script);
                tab.Content = viewer;
                this.tabs.Items.Add(tab);
            }
        }
    }
}
