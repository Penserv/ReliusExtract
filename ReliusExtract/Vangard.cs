using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliusExtract
{
    internal class Vangard
    {
        public string PlanID { get; set; } = string.Empty;
        [Required, StringLength(9, ErrorMessage = "SSN must be 9 numbers.", MinimumLength = 9)]
        public string SSN { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public string StreetAddress1 { get; set; } = string.Empty;
        public string StreetAddress2 { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;

        [StringLength(2, ErrorMessage = "Please use 2 character state abbreviation(ex. KY).", MinimumLength = 2)]
        [RegularExpression("^([Aa][LKSZRAEPlkszraep]|[Cc][AOTaot]|[Dd][ECec]|[Ff][LMlm]|[Gg][AUau]|[Hh][Ii]|[Ii][ADLNadln]|[Kk][SYsy]|[Ll][Aa]|[Mm][ADEHINOPSTadehinopst]|[Nn][CDEHJMVYcdehjmvy]|[Oo][HKRhkr]|[Pp][ARWarw]|[Rr][Ii]|[Ss][CDcd]|[Tt][NXnx]|[Uu][Tt]|[Vv][AITait]|[Ww][AIVYaivy])$", ErrorMessage = "Invalid State Code.")]
        public string StateAbbr { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public string EENumber { get; set; } = string.Empty;
        public DateTime? BirthDate { get; set; }
        public DateTime? HireDate { get; set; }
        public DateTime? TerminationDate { get; set; }
        public DateTime? RehireDate { get; set; }
        public string ActualPaymentFrequencyCode { get; set; } = string.Empty;
        public string EEPlanStatusCode { get; set; } = string.Empty;
    }
}
