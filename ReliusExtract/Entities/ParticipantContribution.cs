using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliusExtract.Entities
{
    internal class ParticipantContribution
    {
        public string SSN { get; set; } = string.Empty;
        public string PlanID { get; set; } = string.Empty;
        public string FundID { get; set; } = string.Empty;
        public double TotalDollarAmount { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
}
