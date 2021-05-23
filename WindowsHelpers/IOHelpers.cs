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
using System.Text;
using System.Threading.Tasks;

namespace WindowsHelpers
{
    public static class IOHelpers
    {
        public async static Task<string> ReadFileAsync(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("File not found: " + path);
            }

            string script = string.Empty;
            try
            {

                byte[] result;
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    result = new byte[fs.Length];
                    await fs.ReadAsync(result, 0, (int)fs.Length);
                }

                script = System.Text.Encoding.UTF8.GetString(result);

                return script;
            }
            catch (Exception e)
            {
                LoggerFacade.Error(e, "Failed to load file: " + path);
            }

            return script;
        }

        /// <summary>
        /// Write a text file with UTF-8 Encoding
        /// </summary>
        /// <param name="path"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public async static Task WriteTextFileAsync(string path, string text)
        {
            try
            {
                DirectoryInfo parent = Directory.GetParent(path);
                Directory.CreateDirectory(parent.FullName);

                using (StreamWriter writer = File.CreateText(path))
                {
                    await writer.WriteAsync(text);
                }
            }
            catch (Exception e)
            {
                LoggerFacade.Error(e, "Failed to write file: " + path);
            }
        }
    }
}
