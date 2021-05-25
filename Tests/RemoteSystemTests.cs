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
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using WindowsHelpers;
using System.Text.Json;
using System.Reflection;

namespace Tests
{
    [TestFixture]
    public class RemoteSystemTests
    {
        public static List<RemoteSystemDetails> RemoteSystemConfig()
        {
            string json = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "Configs\\RemoteSystem-testconfig.json");
            return JsonSerializer.Deserialize<RemoteSystemConfig>(json).RemoteConnecctionTests;
        }

        [Test, TestCaseSource("RemoteSystemConfig")]
        public async Task RemoteSysTest(RemoteSystemDetails details)
        {
            RemoteSystem sys = RemoteSystem.New(details.Device, details.UseSSL);

            await RemoteSystem.Current.ConnectAsync();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(RemoteSystem.Current.ReportedComputerName.ToLower(), details.Device.ToLower());
                Assert.GreaterOrEqual(RemoteSystem.Current.SystemMemoryMB, details.MinMem);
                //Assert.IsTrue(RemoteSystem.Current.SystemPendingReboot == rebootPending);
            });
        } 
    }

    public class RemoteSystemDetails
    {
        public string Device { get; set; }
        public ulong MinMem { get; set; }
        public bool UseSSL { get; set; }
    }

    public class RemoteSystemConfig
    {
        public List<RemoteSystemDetails> RemoteConnecctionTests { get; set; }
    }

}
