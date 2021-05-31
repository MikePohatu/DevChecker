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
using System.Windows.Shapes;

namespace DevChecker
{
    /// <summary>
    /// Interaction logic for CredentialPopup.xaml
    /// </summary>
    public partial class CredentialPopup : Window
    {
        Credential _existingCred;
        Credential _newcreds;
        public CredentialPopup(Credential existingCreds)
        {
            InitializeComponent();
            this._existingCred = existingCreds;
            this._newcreds = this._existingCred.Clone();
            this.DataContext = this._newcreds;
            this.pwBox.Password = existingCreds.Password;
        }

        public void onPasswordChanged(object sender, RoutedEventArgs args)
        {
            this._newcreds.UpdatePassword(this.pwBox.SecurePassword, this.pwBox.Password);
        }

        private void onCancelClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void onSaveClicked(object sender, RoutedEventArgs e)
        {
            this._existingCred.Update(this._newcreds);
            this.Close();
        }
    }
}
