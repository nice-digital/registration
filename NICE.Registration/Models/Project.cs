using System.Text.Json.Serialization;

namespace NICE.Registration.Models
{
	public class Project
	{
		[JsonPropertyName("id")]
		public string Id { get; set; } //e.g. PH24 etc

		[JsonPropertyName("title")]
		public string Title { get; set; }

		[JsonPropertyName("productTypeName")]
		public string ProductTypeName { get; set; } 
	}
}