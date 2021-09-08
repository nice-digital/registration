using System.Collections.Generic;
using System.Linq;

namespace NICE.Registration.Models
{
	/// <summary>
	/// The front-end shows a line in the table for every interest registered for. However the database stores a single registration per submission. This model resolves that.
	/// </summary>
	public class RegistrationsTable
	{
		public RegistrationsTable(IEnumerable<RegistrationSubmission> registrations)
		{
			AllRegistrations =  from registration in registrations 
				from interest in registration.Projects
				select new RegistrationRow(registration, interest);
		}

		public IEnumerable<RegistrationRow> AllRegistrations { get; set; }
	}
}