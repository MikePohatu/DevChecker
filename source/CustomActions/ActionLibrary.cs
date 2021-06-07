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
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CustomActions
{
    public class ActionLibrary
    {
        public static ActionLibrary Instance { get; } = new ActionLibrary();

        private ActionLibrary() { }

        public ObservableCollection<CustomActionScript> Scripts { get; } = new ObservableCollection<CustomActionScript>();
        public ObservableCollection<CustomActionScript> Tabs { get; } = new ObservableCollection<CustomActionScript>();

        private async Task UpdateAsync(bool isrefresh)
        {
            this.Scripts.Clear();
            this.Tabs.Clear();

            List<CustomActionScript> scripts = new List<CustomActionScript>();
            var scriptpaths = Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory + "Scripts\\Custom", "*.ps1");
            foreach (string path in scriptpaths)
            {
                CustomActionScript script = new CustomActionScript();
                await script.Load(path);
                scripts.Add(script);
            }

            scripts.Sort();
            foreach (CustomActionScript script in scripts)
            {
                if (script.Settings.DisplayElement == "Tab")
                {
                    this.Tabs.Add(script);
                }
                else
                {
                    this.Scripts.Add(script);
                }
            }
            if (isrefresh) { Log.Info(Log.Highlight("Actions refreshed")); }
            else { Log.Info("Actions loaded"); }
        }

        public async Task RefreshAsync()
        {
            await this.UpdateAsync(true);
        }

        public async Task LoadAsync()
        {
            await UpdateAsync(false);
        }
    }
}
