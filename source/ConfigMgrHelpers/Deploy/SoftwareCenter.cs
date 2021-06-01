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
using Core;
using Core.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using WindowsHelpers;

namespace ConfigMgrHelpers.Deploy
{
    public class SoftwareCenter: ViewModelBase
    {
		private bool _applicationsLoading = false;
		public bool ApplicationsLoading
		{
			get { return this._applicationsLoading; }
			set { this._applicationsLoading = value; this.OnPropertyChanged(this, "ApplicationsLoading"); }
		}

		private bool _updatesLoading = false;
		public bool UpdatesLoading
		{
			get { return this._updatesLoading; }
			set { this._updatesLoading = value; this.OnPropertyChanged(this, "UpdatesLoading"); }
		}

		private bool _tasksequencesloading = false;
		public bool TaskSequencesLoading
		{
			get { return this._tasksequencesloading; }
			set { this._tasksequencesloading = value; this.OnPropertyChanged(this, "TaskSequencesLoading"); }
		}

		public ObservableCollection<Application> Applications { get; set; } = new ObservableCollection<Application>();
		public ObservableCollection<Update> SoftwareUpdates { get; set; } = new ObservableCollection<Update>();
		public ObservableCollection<TaskSequence> TaskSequences { get; set; } = new ObservableCollection<TaskSequence>();

		public async Task QueryApplicationsAsync()
		{
			if (CmClient.Current.ClientInstalled)
			{
				this.ApplicationsLoading = true;
				string command = Application.GetterCommand;

				Log.Info("Gathering Software Center applications");
				this.Applications.Clear();
				var posh = PoshHandler.GetRunner(command, RemoteSystem.Current);
				var result = await PoshHandler.InvokeRunnerAsync(posh);

				if (result.Count > 0)
				{
					foreach (var poshObj in result)
					{
						this.Applications.Add(Application.New(poshObj));
					}

					Log.Info("Finished gathering applications");
				}
				this.ApplicationsLoading = false;
			}
		}

		public async Task QueryUpdatesAsync()
		{
			if (CmClient.Current.ClientInstalled)
			{
				this.UpdatesLoading = true;
				string command = Update.GetterCommand;

				Log.Info("Gathering Software Center updates");
				this.SoftwareUpdates.Clear();
				var posh = PoshHandler.GetRunner(command, RemoteSystem.Current);
				var result = await PoshHandler.InvokeRunnerAsync(posh);

				if (result.Count > 0)
				{
					foreach (var poshObj in result)
					{
						this.SoftwareUpdates.Add(Update.New(poshObj));
					}

					Log.Info("Finished gathering updates");
				}
				this.UpdatesLoading = false;
			}
		}

		public async Task QueryTaskSequencesAsync()
		{
			if (CmClient.Current.ClientInstalled)
			{
				this.TaskSequencesLoading = true;
				string command = TaskSequence.GetterCommand;
				Log.Info("Gathering Software Center Task Sequences");
				this.TaskSequences.Clear();
				var posh = PoshHandler.GetRunner(command, RemoteSystem.Current);
				var result = await PoshHandler.InvokeRunnerAsync(posh);

				if (result.Count > 0)
				{
					foreach (var poshObj in result)
					{
						this.TaskSequences.Add(TaskSequence.New(poshObj));
					}

					Log.Info("Finished gathering task sequences");
				}
				this.TaskSequencesLoading = false;
			}
		}
	}
}
