using System;

namespace NICE.Registration.Models
{
	public class RegistrationSubmission
    {
	    public RegistrationSubmission() {}

	    public string Id { get; set; }
        public string UserNameIdentifier { get; set; }

		public Interest[] Interests { get; set; } //= new List<Interest>();

		public DateTime CreatedTimestampUTC { get; set; } = DateTime.UtcNow;
    }
}
