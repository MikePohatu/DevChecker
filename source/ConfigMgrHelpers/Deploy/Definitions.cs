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
using System.Collections.Generic;
using System.Management.Automation;
using WindowsHelpers;

namespace ConfigMgrHelpers.Deploy
{
    public static class Definitions
    {
        public static string GetAppState(int EvalCode)
        {
            switch (EvalCode)
            {
                case 0:
                    return "Unknown";
                case 1:
                    return "Available";
                case 2:
                    return "Submitted";
                case 3:
                    return "Detecting";
                case 4:
                    return "Pre-Download";
                case 5:
                    return "Downloading";
                case 6:
                    return "Wait Install";
                case 7:
                    return "Installing";
                case 8:
                    return "Waiting for Service Window";
                case 9:
                    return "Waiting for Reboot";
                case 10:
                    return "Waiting To Enforce";
                case 11:
                    return "Enforcing Dependencies";
                case 12:
                    return "Enforcing";
                case 13:
                    return "Soft Reboot Pending";
                case 14:
                    return "Hard Reboot Pending";
                case 15:
                    return "Pending Update";
                case 16:
                    return "Evaluation Failed";
                case 17:
                    return "Waiting User Reconnect";
                case 18:
                    return "Waiting for User Logoff";
                case 19:
                    return "Waiting for User Logon";
                case 20:
                    return "In Progress Waiting Retry";
                case 21:
                    return "Waiting for Pres Mode Off";
                case 22:
                    return "Advance Downloading Content";
                case 23:
                    return "Advance Dependencies Download";
                case 24:
                    return "Download Failed";
                case 25:
                    return "Advance Download Failed";
                case 26:
                    return "Download Success";
                case 27:
                    return "Post Enforce Evaluation";
                default:
                    return "Unknown State (" + EvalCode.ToString() + ")";
            }
        }

        public static string GetAppState(PSObject poshObj)
        {
            int state = PoshHandler.GetPropertyValue<int>(poshObj, "EvaluationState");
            return GetAppState(state);
        }

        public static string GetProgramState(int EvalCode)
        {
            switch (EvalCode)
            {
                case 0:
                    return "No information is available";
                case 1:
                    return "Available";
                case 2:
                    return "Installing (Waiting for Dependencies)";
                case 3:
                    return "Waiting for content locations";
                case 4:
                    return "Downloading manifest";
                case 5:
                    return "Processing manifest";
                case 6:
                    return "Creating directories";
                case 7:
                    return "Preparing download";
                case 8:
                    return "Verifying download";
                case 9:
                    return "Content is currently downloading";
                case 10:
                    return "Waiting on user";
                case 11:
                    return "Waiting for service window";
                case 12:
                    return "Waiting for another program";
                case 14:
                    return "Installing";
                case 15:
                    return "There is a pending reboot";
                case 16:
                    return "Waiting for a logoff";
                case 17:
                    return "Install was successful";
                case 18:
                    return "Install has failed";
                case 28:
                    return "Install not allowed or supported";
                default:
                    return "Unknown State (" + EvalCode.ToString() + ")";
            }
        }

        public static string GetProgramState(PSObject poshObj)
        {
            int state = PoshHandler.GetPropertyValue<int>(poshObj, "EvaluationState");
            return GetProgramState(state);
        }
    }
}
