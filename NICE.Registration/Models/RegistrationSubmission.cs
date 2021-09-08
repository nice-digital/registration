using System;
using System.Text.Json.Serialization;
using Amazon.Lambda.Serialization.Json;

namespace NICE.Registration.Models
{
	/// <summary>
	/// This is the model that's saved to the database.
	/// </summary>
	public class RegistrationSubmission
    {
		//public string Id { get; set; }

		//[JsonIgnore]
		//public string UserNameIdentifier { get; set; } //this field is set automatically

		//[JsonIgnore]
		//public DateTime CreatedTimestampUTC { get; set; } = DateTime.UtcNow;

		//#region Fields from the front-end.

		//[JsonIgnore]
		////[JsonPropertyName("projects")]
		//public Project[] Projects { get; set; }

		//[JsonIgnore]
		////[JsonPropertyName("registeringAs")] 
		//public string RegisteringAs { get; set; } //todo: make an enum. problem is we need .net core 5: https://stackoverflow.com/questions/59059989/system-text-json-how-do-i-specify-a-custom-name-for-an-enum-value/59061296#59061296

		//[JsonIgnore]
		////[JsonPropertyName("organisationName")]
		//public string OrganisationName { get; set; }

		//[JsonIgnore]
		////[JsonPropertyName("addressLine1")]
		//public string AddressLine1 { get; set; }

		//[JsonIgnore]
		////[JsonPropertyName("addressLine2")]
		//public string AddressLine2 { get; set; }

		////[JsonPropertyName("townOrCity")]
		//[JsonIgnore]
		//public string TownOrCity { get; set; }

		//[JsonIgnore]
		////[JsonPropertyName("county")]
		//public string County { get; set; }

		//[JsonIgnore]
		////[JsonPropertyName("postcode")]
		//public string Postcode { get; set; }

		[JsonPropertyName("country")]
		public string Country { get; set; }

		//#endregion Fields from the front-end.
	}
}
