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
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsHelpers;

namespace DevChecker.Config
{
    public class Configuration
    {
        public string ConfigMgrServer { get; set; }
        public string LastDevice { get; set; }

        public List<string> DeviceHistory { get; set; }

        public bool ClientSSL { get; set; }
        public bool ServerSSL { get; set; }

        public bool ClientKerberos { get; set; }
        public bool ServerKerberos { get; set; }

        public bool UseSSL { get; set; } = false;

        public string ClientUsername { get; set; }
        public string ClientDomain { get; set; }
        public string ServerUsername { get; set; }
        public string ServerDomain { get; set; }

        public static Configuration Instance { get; private set; } = new Configuration();
        private Configuration() { }

        public static async Task LoadAsync(string filePath)
        {
            string json = await IOHelpers.ReadFileAsync(filePath);
            Instance = JsonConvert.DeserializeObject<Configuration>(json);
            if (string.IsNullOrWhiteSpace(Instance.ConfigMgrServer))
            {
                Instance.ConfigMgrServer = RegistryHelpers.GetStringValue(@"HKEY_CURRENT_USER\Software\Microsoft\ConfigMgr10\AdminUI\MRU\1", "ServerName", null);
            }
            Log.Info("Done loading config file: " + filePath);
        }

        public async Task WriteAsync(string filePath)
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            await IOHelpers.WriteTextFileAsync(filePath, json);
        }
    }
}
