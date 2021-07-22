using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NICE.Registration
{    
    public class Interest
	{
		public Interest() {}

		public string ProjectID { get; set; } //e.g. PH24 etc
		public string ProjectTitle { get; set; }
		public string ProgrammeID { get; set; } //e.g. CG, QS etc
	}

    public class Registration
    {
	    public Registration() {}

	    public string Id { get; set; }
        public string UserNameIdentifier { get; set; }

		public Interest[] Interests { get; set; } //= new List<Interest>();

		public DateTime CreatedTimestampUTC { get; set; } = DateTime.UtcNow;
    }
}
