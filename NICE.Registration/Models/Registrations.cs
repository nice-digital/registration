using System.Collections.Generic;
using System.Linq;

namespace NICE.Registration.Models
{
	/// <summary>
	/// The front-end shows a line in the table for every interest registered for. However the database stores a single registration per submission. This model resolves that.
	/// </summary>
	public class Registrations
	{
		public Registrations(IEnumerable<RegistrationSubmission> registrations)
		{
			AllRegistrations =  from registration in registrations 
				from interest in registration.Interests 
				select new Registration(registration, interest);
		}

		public IEnumerable<Registration> AllRegistrations { get; set; }
	}
}