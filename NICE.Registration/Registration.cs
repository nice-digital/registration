using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NICE.Registration
{
    public class Registration
    {
        public string Id { get; set; }
        public string NameIdentifier { get; set; }

        public string Description { get; set; }
        public DateTime CreatedTimestamp { get; set; }
    }
}
