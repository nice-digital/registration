using System;
using System.Text.Json.Serialization;

namespace NICE.Registration.Models
{
	/// <summary>
	/// This is the model that's saved to the database.
	/// </summary>
	public class RegistrationSubmission
    {
	    public string Id { get; set; }
        public string UserNameIdentifier { get; set; } //this field is set automatically

		public DateTime CreatedTimestampUTC { get; set; } = DateTime.UtcNow;

		#region Fields from the front-end.

		[JsonPropertyName("projects")]
		public Project[] Projects { get; set; }

		[JsonPropertyName("registeringAs")] 
		public string RegisteringAs { get; set; } //todo: make an enum. problem is we need .net core 5: https://stackoverflow.com/questions/59059989/system-text-json-how-do-i-specify-a-custom-name-for-an-enum-value/59061296#59061296

		[JsonPropertyName("organisationName")]
		public string OrganisationName { get; set; }

        [JsonPropertyName("organisationType")]
        public string OrganisationType { get; set; }

		[JsonPropertyName("addressLine1")]
		public string AddressLine1 { get; set; }

		[JsonPropertyName("addressLine2")]
		public string AddressLine2 { get; set; }

		[JsonPropertyName("townOrCity")]
		public string TownOrCity { get; set; }

		[JsonPropertyName("county")]
		public string County { get; set; }

		[JsonPropertyName("postcode")]
		public string Postcode { get; set; }

		[JsonPropertyName("country")]
		public string Country { get; set; }

		#endregion Fields from the front-end.
	}
}
