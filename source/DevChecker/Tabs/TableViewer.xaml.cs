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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Collections.ObjectModel;

namespace DevChecker.Tabs
{
    /// <summary>
    /// Interaction logic for CmApplicationsTab.xaml
    /// </summary>
    public abstract partial class TableViewer : UserControl
    {
        private bool _hasContextMenu = false;

        public static readonly DependencyProperty IsSearchEnabledProperty = DependencyProperty.Register("IsSearchEnabled", typeof(bool), typeof(TableViewer));
        public bool IsSearchEnabled
        {
            get { return (bool)GetValue(IsSearchEnabledProperty); }
            set { SetValue(IsSearchEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register("IsLoading", typeof(bool), typeof(TableViewer));
        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }

        public static readonly DependencyProperty TableSourceProperty = DependencyProperty.Register("TableSource", typeof(ObservableCollection<object>), typeof(TableViewer));
        public ObservableCollection<object> TableSource
        {
            get { return (ObservableCollection<object>)GetValue(TableSourceProperty); }
            set { SetValue(TableSourceProperty, value); }
        }


        public TableViewer()
        {
            InitializeComponent();
        }

        private void onSearchBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource source = this.Resources["filtered"] as CollectionViewSource;
            if (source?.View != null)
            {
                source.View.Refresh();
            }
        }

        protected abstract void onSearchFilter(object sender, FilterEventArgs e);
        protected abstract void onRefreshClicked(object sender, RoutedEventArgs e);

        protected void AddContextMenuItem(MenuItem item)
        {
            if (this._hasContextMenu == false)
            {
                this._hasContextMenu = true;
                this.DataGrid.ContextMenu = new ContextMenu();
            }

            this.DataGrid.ContextMenu.Items.Add(item);
        }
    }
}
