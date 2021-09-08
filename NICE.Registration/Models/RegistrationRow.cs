using System;

namespace NICE.Registration.Models
{
	public class RegistrationRow
	{
		private readonly RegistrationSubmission _registrationSubmission;
		private readonly Project _project;

		public RegistrationRow(RegistrationSubmission registrationSubmission, Project project)
		{
			_registrationSubmission = registrationSubmission;
			_project = project;
		}

		public string Id => "TODO"; // $"{_registrationSubmission.Id}:{_project.Id}";
		public string Title => _project.Title;
		public string Status => "Pending";
		public string DateSubmitted => "TODO"; // _registrationSubmission.CreatedTimestampUTC.ToString("d");
	}
}