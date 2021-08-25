using System;

namespace NICE.Registration.Models
{
	public class Registration
	{
		private readonly RegistrationSubmission _registrationSubmission;
		private readonly Interest _interest;

		public Registration(RegistrationSubmission registrationSubmission, Interest interest)
		{
			_registrationSubmission = registrationSubmission;
			_interest = interest;
		}

		public string Id => _registrationSubmission.Id;
		public string Title => _interest.ProjectTitle;
		public string Status => "Pending";
		public DateTime CreatedTimestampUTC => _registrationSubmission.CreatedTimestampUTC;
	}
}