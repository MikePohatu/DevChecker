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
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using WindowsHelpers;

namespace Tests
{
    [TestFixture]
    public class PoshTests
    {
        [Test]
        [TestCase(".", false, ExpectedResult = "5.1.19041.906")]
        [TestCase("sccm01.home.local", true, ExpectedResult = "1.0.0.0")]
        public async Task<string> PoshConnectionTest(string Computer, bool useSSL)
        {
            PowerShell posh = PoshHandler.GetRunner("Get-Host", Computer, useSSL, null);
            PSDataCollection<PSObject> result = await PoshHandler.InvokeRunnerAsync(posh);

            string version = "0";

            foreach (PSObject obj in result)
            {
                foreach (var prop in obj.Properties)
                {
                    if (prop.Name == "Version")
                    {
                        version = prop.Value.ToString();
                    }
                }
            }

            return version;
        }

        [Test]
        [TestCase(".", false, ExpectedResult = "1")]
        //[TestCase("homedc05.home.local", true, ExpectedResult = "2")]
        [TestCase("sccm01.home.local", true, ExpectedResult = "3")]
        public async Task<string> OSTypeTest(string Computer, bool useSSL)
        {
            PowerShell posh = PoshHandler.GetRunner("Get-WmiObject -Query 'SELECT ProductType FROM Win32_OperatingSystem'", Computer, useSSL, null);

            PSDataCollection<PSObject> results = await PoshHandler.InvokeRunnerAsync(posh);
            string type = PoshHandler.GetPropertyValues<string>(results, "ProductType").First();

            return type;
        }
    }
}
