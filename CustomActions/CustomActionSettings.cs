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
        /// <summary>
        /// What the action will return. Valid values: Object, String, Number, Boolean, List
        /// </summary>
        public string OutputType { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool RunOnConnect { get; set; } = false;
        public bool LogScriptContent { get; set; } = false;

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
