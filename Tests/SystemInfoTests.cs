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
using System.Text;
using System.Threading.Tasks;
using WindowsHelpers;

namespace Tests
{
    [TestFixture]
    public class SystemInfoTests
    {
        [Test]
        public async Task RemoteSysTest(string Computer, bool useSSL, string ostype, bool rebootPending, ulong memAtLeast)
        {
            RemoteSystem.New(Computer);
            RemoteSystem.Current.UseSSL = useSSL;

            await RemoteSystem.Current.ConnectAsync();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(RemoteSystem.Current.InstalledOSType, ostype);
                Assert.GreaterOrEqual(RemoteSystem.Current.SystemMemoryMB, memAtLeast);
                Assert.IsTrue(RemoteSystem.Current.SystemPendingReboot == rebootPending);
            });
        }
    }
}
