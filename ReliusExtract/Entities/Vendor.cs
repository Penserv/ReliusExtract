using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliusExtract.Entities
{
    internal class Vendor
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address1 { get; set; } = string.Empty;
        public string Address2 { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Zip { get; set; } = string.Empty;
        public string QuickBooksName { get; set; } = string.Empty;
        public string SparkName { get; set; } = string.Empty;
        public string SparkId { get; set; } = string.Empty;
        public string FundId { get; set; } = string.Empty;
        public string DataSource { get; set; } = string.Empty;
    }
}
