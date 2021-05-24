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
using Diags.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using WindowsHelpers;
using Newtonsoft.Json;

namespace CustomActions
{
    public class CustomActionScript: IComparable<CustomActionScript>
    {
        private bool _loaded = false;
        private string _scriptpath = string.Empty;
        private string _script = string.Empty;
        private string _filename = string.Empty;

        /// <summary>
        /// The actual script name
        /// </summary>
        public string Name { get { return this._filename; } }

        /// <summary>
        /// The display name of the script. Will return the Name attribute if not set in the Settings
        /// </summary>
        public string DisplayName
        {
            get { return string.IsNullOrWhiteSpace(this.Settings?.DisplayName) == true ? this._filename : this.Settings.DisplayName; }
        }
        public CustomActionSettings Settings { get; private set; }

        public CustomActionScript()
        {
            RemoteSystem.Connected += this.OnConnected;
        }

        public int CompareTo(CustomActionScript other)
        {
            if (other == null) return 1;
            return this.DisplayName.CompareTo(other.DisplayName);
        }

        public async void OnConnected (object sender, EventArgs args)
        {
            if (this.Settings != null && this.Settings.RunOnConnect == true)
            {
                await this.RunActionAsync();
            }
        }

        /// <summary>
        /// Load the script file 
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public async Task Load(string filepath)
        {
            if (string.IsNullOrWhiteSpace(filepath) == false)
            {
                this._scriptpath = filepath;
                this._script = await IOHelpers.ReadFileAsync(filepath);

                if (string.IsNullOrWhiteSpace(this._script) == false)
                {
                    this._loaded = true;

                    using (StringReader reader = new StringReader(this._script))
                    {
                        bool readingsettings = false;
                        StringBuilder builder = new StringBuilder();
                        string line = string.Empty;
                        do
                        {
                            line = reader.ReadLine();
                            if (line != null)
                            {
                                if (readingsettings)
                                {
                                    if (line.TrimStart().ToLower().StartsWith("actionsettings#>")) { readingsettings = false; }
                                    else { builder.AppendLine(line); }
                                }
                                else
                                {
                                    if (line.TrimStart().ToLower().StartsWith("<#actionsettings")) { readingsettings = true; }
                                }
                            }

                        } while (line != null);

                        string settingsjson = builder.ToString();
                        if (string.IsNullOrWhiteSpace(settingsjson) == false)
                        {
                            this.Settings = CustomActionSettings.Create(settingsjson);
                        }
                    }
                    this._filename = Path.GetFileNameWithoutExtension(filepath);
                }
            }
        }

        public async Task RunActionAsync()
        {
            if (this._loaded == true && string.IsNullOrWhiteSpace(this._script) == false)
            {
                LoggerFacade.Info("Running custom action: " + this.DisplayName);

                PowerShell posh = PoshHandler.GetRunner(this._script, RemoteSystem.Current.ComputerName, RemoteSystem.Current.UseSSL);

                await PoshHandler.InvokeRunnerAsync(posh, !this.Settings.LogScriptContent);
            }
            else
            {
                LoggerFacade.Warn("Script hasn't finished loading yet. Please try again soon. Script: " + this._scriptpath);
            }
        }
    }
}
