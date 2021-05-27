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
using ConfigMgrHelpers;
using Core.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace _20RoadRemoteAdmin.Tabs
{
    /// <summary>
    /// Interaction logic for CmSystemActions.xaml
    /// </summary>
    public partial class CmSystemActions : UserControl
    {
        public CmSystemActions()
        {
            InitializeComponent();
        }

        private async void onRestartClicked(object sender, RoutedEventArgs e)
        {
            if (RemoteSystem.Current != null)
            {
                await ServiceHelpers.RestartService("ccmexec", RemoteSystem.Current);
            }
        }

        public void onLogsClicked(object sender, RoutedEventArgs e)
        {
            string path = @"\\" + RemoteSystem.Current.ComputerName + @"\c$\Windows\ccm\Logs";
            try
            {
                Process.Start(path);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error opening path: " + path);
            }
        }
    }
}
