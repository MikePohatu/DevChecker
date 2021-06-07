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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsHelpers;

namespace CustomActions
{
    public class CustomActionSettings
    {
        private string _outputtype;
        /// <summary>
        /// What the action will return. Valid values: Object, List, Log, None
        /// </summary>
        public string OutputType
        {
            get { return this._outputtype; }
            set
            {
                if (string.IsNullOrWhiteSpace(value)) { this._outputtype = "Log"; }
                string val = value.Trim().ToLower();
                switch (val)
                {
                    case "log":
                        this._outputtype = "Log";
                        break;
                    case "list":
                        this._outputtype = "List";
                        break;
                    case "object":
                        this._outputtype = "Object";
                        break;
                    case "none":
                        this._outputtype = "None";
                        break;
                    default:
                        Log.Error("Invalid output type set");
                        this._outputtype = "Log";
                        break;
                }
            }
        }

        private string _displayElement;
        /// <summary>
        /// How the returned data will be displayed. Valid values: Tab, Modal, Log
        /// </summary>
        public string DisplayElement
        {
            get { return this._displayElement; }
            set
            {
                if (string.IsNullOrWhiteSpace(value)) { this._displayElement = "Log"; }
                else
                {
                    string val = value.Trim().ToLower();
                    switch (val)
                    {
                        case "tab":
                            this._displayElement = "Tab";
                            break;
                        case "modal":
                            this._displayElement = "Modal";
                            break;
                        case "log":
                            this._displayElement = "Log";
                            break;
                        default:
                            Log.Error("Invalid display element set");
                            break;
                    }
                }                
            }
        }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool RunOnConnect { get; set; } = false;

        /// <summary>
        /// Whether to run on the client. Otherwise will be run on local computer
        /// </summary>
        public bool RunOnClient { get; set; } = true;
        public bool LogScriptContent { get; set; } = false;
        public List<string> FilterProperties { get; set; } = new List<string>();

        public static CustomActionSettings Create(string json)
        {
            try
            {
                CustomActionSettings settings = JsonConvert.DeserializeObject<CustomActionSettings>(json);
                return settings;
            }
            catch (Exception e)
            {
                Log.Error(e, "Error loading ActionSettings");
                return null;
            }
        }
    }
}
