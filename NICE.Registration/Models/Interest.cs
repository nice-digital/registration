namespace NICE.Registration.Models
{
	public class Interest
	{
		public Interest() {}

		public string ProjectID { get; set; } //e.g. PH24 etc
		public string ProjectTitle { get; set; }
		public string ProgrammeID { get; set; } //e.g. CG, QS etc
	}
}