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
using NLog;
using NLog.Config;
using System.Linq;

namespace Diags.Logging
{

    public static class LoggingHelpers
    {
        private static string _highlightprefix = "**";

        public static void AddLoggingHandler(NewLog handler)
        {
            if (handler == null) { return; }

            var receivers = NLog.LogManager.Configuration.AllTargets.Where(t => t is UserUITarget).Cast<UserUITarget>();
            foreach (UserUITarget receiver in receivers)
            { receiver.NewLogMessage += handler; }
        }

        public static void InitLogging()
        {
            NLog.Config.ConfigurationItemFactory.Default.Targets.RegisterDefinition("UserUITarget", typeof(UserUITarget));
        }

        public static void SetLoggingLevel(LogLevel loglevel)
        {
            LoggingRule rule = LogManager.Configuration.FindRuleByName("outputPane");
            rule?.SetLoggingLevels(loglevel, LogLevel.Fatal);
            LogManager.ReconfigExistingLoggers();
        }

        public static string Highlight(string message)
        {
            return _highlightprefix + message;
        }

        public static bool IsHighlighted(string message, out string unhighlighted)
        {
            if (message.StartsWith(_highlightprefix))
            {
                if (message.Length == _highlightprefix.Length) { unhighlighted = string.Empty; }
                else { unhighlighted = message.Substring(_highlightprefix.Length); }
                return true;
            }
            else
            {
                unhighlighted = message;
                return false;
            }
        }
    }
}
