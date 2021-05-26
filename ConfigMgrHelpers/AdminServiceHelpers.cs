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
using Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMgrHelpers
{
    public static class AdminServiceHelpers
    {
        private static readonly HttpClient _client = new HttpClient(new HttpClientHandler() {
            UseDefaultCredentials = true
        });

        public static async Task ConnectAsync(string server)
        {
            string url = @"https://" + server + @"/AdminService/wmi/SMS_Site";

            try
            {
                HttpResponseMessage response = await _client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                Log.Info("Connected to server  " + server);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error accessing admin server, url: " + url);
            }
        }
    }
}
