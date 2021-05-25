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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsHelpers;
using System.Text.Json;
using Diags.Logging;

namespace _20RoadRemoteAdmin.Config
{
    public class Configuration
    {
        private static JsonSerializerOptions _jsonOptions = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        public string ConfigMgrServer { get; set; }
        public string LastDevice { get; set; }

        public bool ClientSSL { get; set; } = false;
        public bool ServerSSL { get; set; } = true;
        public static Configuration Instance { get; private set; }
        
        // This should be a singleton, but this works around limitation in System.Text.Json that doesn't support
        // private constructors
        public Configuration() {
            if (Instance != null) { throw new InvalidOperationException("Multiple Configurations created"); }
            Instance = this;
        }

        public static async Task<Configuration> LoadAsync(string filePath)
        {
            string json = await IOHelpers.ReadFileAsync(filePath);
            Configuration config = null;
            try
            {
                config = JsonSerializer.Deserialize<Configuration>(json, _jsonOptions);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error loading configuration file: " + filePath);
            }
            
            Instance = config;
            return config;
        }

        public async Task WriteAsync(string filePath)
        {
            string json = JsonSerializer.Serialize(this, _jsonOptions);
            await IOHelpers.WriteTextFileAsync(filePath, json);
        }
    }
}
