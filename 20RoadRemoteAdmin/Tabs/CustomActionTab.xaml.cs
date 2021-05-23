#region license
// Copyright (c) 2021 20Road Limited
//
// This file is part of 20Road Remote Admin.
//
// 20Road Remote Admin is free software: you can redistribute it and/or modify
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
using CustomActions;

namespace _20RoadRemoteAdmin.Tabs
{
    /// <summary>
    /// Interaction logic for CustomActionTab.xaml
    /// </summary>
    public partial class CustomActionTab : UserControl
    {
        public CustomActionTab()
        {
            InitializeComponent();
        }

        public async void onRunActionClicked(object sender, RoutedEventArgs e)
        {
            var uiElement = sender as FrameworkElement;
            var component = uiElement?.DataContext as CustomActionScript;
            await component?.RunActionAsync();
        }

        public async void onRefreshClicked(object sender, RoutedEventArgs e)
        {
            await ActionLibrary.RefreshAsync();
        }
    }
}
