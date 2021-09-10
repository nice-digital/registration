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

		public string Id => $"{_registrationSubmission.Id}:{_project.Id}";
		public string Title => _project.Title;
		public string ProjectID => _project.Id;
		public string ProductTypeName => _project.ProductTypeName;
		public string Status => "Pending";
		public string DateSubmitted => _registrationSubmission.CreatedTimestampUTC.ToString("dd/MM/yyyy HH:mm");
		
	}
}