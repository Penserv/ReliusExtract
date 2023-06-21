using Dapper;
using Microsoft.VisualBasic;
using ReliusExtract.Entities;
using System.Configuration;
using System.Data;
using System.Data.OracleClient;
using System.Data.SqlClient;
using System.Text;
using System.Xml;

namespace ReliusExtract
{
    class Program
    {
        static void Main(string[] args)
        {
            ReliusExtract extract = new ReliusExtract();
            extract.RunExtract(args);
        }
    }
    public class ReliusExtract
    {
        string sLogFile = "ReliusExtract.log";
        LogFile prtLogFile = new LogFile();
        string sLogFileEntry = string.Empty;
        public void RunExtract(string[] args)
        {
            prtLogFile.FileName = sLogFile;
            if (!File.Exists(sLogFile))
            {
                File.Create(sLogFile);
            }
            if(args.Length > 0)
            {
                string month = args[0].ToString();
                string year = args[1].ToString();
                Console.WriteLine("Relius Extract Started at: " + DateTime.Now.ToString());
                sLogFileEntry = ("Relius Extract Started at: " + DateTime.Now.ToString());
                prtLogFile.WriteToLogFile(sLogFileEntry);
                //var vanData = GetVangargAOPCensusData();
                //CreateVanguardAOPCensusFile(vanData);
                Console.WriteLine("Particpant Contribution Update Started at: " + DateTime.Now.ToString());
                sLogFileEntry = ("Particpant Contribution Update Started at: " + DateTime.Now.ToString());
                var pc = GetParticpantContributions(month, year).Except(GetParticpantContributionsBilling());
                int count = UpdateParticipantContributions(pc.ToList());
                Console.WriteLine(count.ToString() + " Participant Records Added At " + DateTime.Now.ToString());
                sLogFileEntry = (count.ToString() + " Participant Records Added At " + DateTime.Now.ToString());
                Console.WriteLine("Relius Extract Completed at: " + DateTime.Now.ToString());
                sLogFileEntry = ("Relius Extract Completed at: " + DateTime.Now.ToString());
                prtLogFile.WriteToLogFile(sLogFileEntry);
            }
            else
            {
                Console.WriteLine("Relius Extract Started at: " + DateTime.Now.ToString());
                sLogFileEntry = ("Relius Extract Started at: " + DateTime.Now.ToString());
                prtLogFile.WriteToLogFile(sLogFileEntry);
                //var vanData = GetVangargAOPCensusData();
                //CreateVanguardAOPCensusFile(vanData);
                Console.WriteLine("Particpant Contribution Update Started at: " + DateTime.Now.ToString());
                sLogFileEntry = ("Particpant Contribution Update Started at: " + DateTime.Now.ToString());
                var pc = GetParticpantContributions().Except(GetParticpantContributionsBilling());
                int count = UpdateParticipantContributions(pc.ToList());
                Console.WriteLine(count.ToString()+" Participant Records Added At " + DateTime.Now.ToString());
                sLogFileEntry = (count.ToString() + " Participant Records Added At " + DateTime.Now.ToString());
                Console.WriteLine("Relius Extract Completed at: " + DateTime.Now.ToString());
                sLogFileEntry = ("Relius Extract Completed at: " + DateTime.Now.ToString());
                prtLogFile.WriteToLogFile(sLogFileEntry);
            }

        }

        private static List<Vendor> GetVendors()
        {
            string cmdtxt = "SELECT * From Vendor";
            List<Vendor> vendors = new List<Vendor>();
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["BillingConnectionString"].ToString()))
            using (var command = connection.CreateCommand())
            {
                vendors = (List<Vendor>)connection.Query<Vendor>(cmdtxt);
            }
            return vendors;
        }

        private static int UpdateParticipantContributions(List<ParticipantContribution> pc)
        {
            var vendors = GetVendors();
            var partCont = pc.Where(pc => vendors.Any(c => c.FundId.Equals(pc.FundID))).ToList();
            
            int count = 0;
            string cmdtxt = "create table #tempTable (id varchar(15))\r\ndeclare @count as int\r\nset @count=0\r\n";
            foreach (var c in partCont)
            {
                cmdtxt += "if not exists (select SSN, PlanId, FundId, [Month], [Year] \r\n" +
                    "from ParticipantContributions\r\n" +
                    "where SSN = '" + c.SSN + "' and PlanId = '" + c.PlanID + "' and FundId = '" + c.FundID + "' and Month = '" + c.Month.ToString("00") + "' and Year = '" + c.Year.ToString() + "')\r\n" +
                    "begin\r\n  insert into ParticipantContributions (SSN, PlanId, FundId, TotalDollarAmount, Month, Year, FirstName, LastName) values\r\n" +
                    "  ('" + c.SSN + "', '" + c.PlanID + "', '" + c.FundID + "', " + c.TotalDollarAmount.ToString() + ", '" + c.Month.ToString("00") + "', '" + c.Year.ToString() + "', '"+c.FirstName.Replace("'", "''") + "', '" + c.LastName.Replace("'", "''") + "')\r\n" +
                    "  insert into #tempTable values ('" + c.SSN + "')\r\n" +
                    "end;\r\n";
            }
            cmdtxt += "set @count = (select count(*) from #tempTable)\r\ndrop table #tempTable\r\nselect @count as \"count\";\r\n";
            if(pc.Count() > 0)
            {
                try
                {
                    using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["BillingConnectionString"].ToString()))
                    using (var command = connection.CreateCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = cmdtxt;
                        command.CommandTimeout = 0;
                        SqlDataAdapter adapter = new SqlDataAdapter();
                        command.Connection.Open();
                        try
                        {
                            adapter = new SqlDataAdapter(command);
                        }
                        catch (SqlException ex)
                        {
                            throw new Exception(ex.ToString());
                        }
                        DataSet ds = new DataSet();
                        adapter.Fill(ds);
                        command.Connection.Close();
                        connection.Close();
                        int rowCount = 0;
                        foreach (DataTable table in ds.Tables)
                        {
                            rowCount = table.Rows[0].Field<int>("count");
                        }
                        count = rowCount;
                        //var participantContributions = connection.QueryFirstOrDefault<int>(cmdtxt);
                        //return participantContributions;
                    }
                    return count;
                }
                catch(SqlException ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            return 0;
            
        }

        private void CreateVanguardAOPCensusFile(List<Vangard> vanData)
        {
            string planId = "094572";
            string fileName = ConfigurationManager.AppSettings["VanguardCensusFileName"].ToString() + "_" + DateTime.Now.ToString("yyyyMMdd")
                + "_" + DateTime.Now.TimeOfDay.ToString("hhmmss") + "_001.csv";
            File.Create(fileName);
            string header = CreateHeader();
            string sb = string.Empty;
            sb += header + "\n";
            string padding = string.Empty;
            foreach(var van in vanData)
            {
                const int maxNameLength = 30;
                string lastName = van.LastName.Replace(",", "").Replace("#", "").Replace(".", "").Replace("\\", "").Replace("/", "").Replace("&", "").Replace("*", "").Replace("\"", "").Replace(Strings.ChrW(164).ToString(), "n").Replace(Strings.ChrW(199).ToString(), "n").Replace(Strings.ChrW(241).ToString(), "n");
                string address1 = ScrubAddress(van.StreetAddress1);
                string address2 = ScrubAddress(van.StreetAddress2);
                string city = ScrubCity(van.City);
                //string address = string.Empty;
                if (address2.Contains("po box", StringComparison.CurrentCultureIgnoreCase) || address1.Contains("po box", StringComparison.CurrentCultureIgnoreCase))
                {
                    address2 = string.Empty;
                }
                int combinedAddLength = address1.Length + address2.Length;
                address1 = SetAddress1(van, address1, address2, combinedAddLength).PadRight(30);
                if (address1.Length > maxNameLength) { address1 =  address1.Substring(0, maxNameLength); }
                address2 = SetAddress2(van, address1, address2, combinedAddLength).PadRight(30);
                if (address2.Length > maxNameLength) { address2 = address2.Substring(0, maxNameLength); }
                string name = lastName + ", " + van.FirstName + " " + van.MiddleName;
                if (name.Length > maxNameLength) { name = name.Substring(0, maxNameLength); }
                if(city.Length > 18) { city = city.Substring(0, 18); }
                sb += planId + van.SSN.PadLeft(9, '0') + "A" + padding.PadRight(24);
                sb += name.PadRight(maxNameLength) + padding.PadRight(2);
                sb += address1 + address2 + city.PadRight(18);
                sb += "\n";

                

            }
            File.WriteAllText(fileName, sb.ToString());

        }
        private string ScrubCity(string city)
        {
            return city.Replace(",", "").Replace("#", "").Replace(".", "").Replace("-", "").Replace("\\", "").Replace("/", "").Replace("&", "").Replace("*", "").Replace("\"", "").Replace("\'", "");
        }

        private string SetAddress1(Vangard van, string address1, string address2, int combinedAddLength)
        {
            string address;
            string[] apt = new string[] { "Apartment", "apt" };

            if (combinedAddLength <= 29 && StartsWithString(van.StreetAddress1, apt))
            {
                address = address2 + " " + address1;
            }
            else if (combinedAddLength > 29 && StartsWithString(van.StreetAddress1, apt))
            {
                address = address2;
            }
            else if (combinedAddLength <= 29)
            {
                address = address1 + " " + address2;
            }
            else
            {
                address = address1;
            }

            return address;
        }

        private string SetAddress2(Vangard van, string address1, string address2, int combinedAddLength)
        {
            string address;
            string[] apt = new string[] { "Apartment", "apt" };

            if (combinedAddLength > 29 && StartsWithString(van.StreetAddress1, apt))
            {
                address = address1;
            }
            else if (combinedAddLength > 29)
            {
                address = address2;
            }
            else if (combinedAddLength <= 29)
            {
                address = string.Empty;
            }
            else
            {
                address = address2;
            }

            return address;
        }

        private bool StartsWithString(string s, string[] s2)
        {
            foreach(var s3 in s2)
            {
                if(s.StartsWith(s3, StringComparison.CurrentCultureIgnoreCase))
                { return true; }

            }
            return false;
        }

        private string ScrubAddress(string streetAddress)
        {
            return streetAddress.Replace(",", "").Replace("#", "").Replace(".", "").Replace("-", "").Replace("\\", "").Replace("/", "").Replace("&", "").Replace("*", "").Replace("\"", "").Replace(Strings.ChrW(39).ToString(), "");
        }

        private string CreateHeader()
        {
            return "00000000000HDRSFF     094572" + DateTime.Now.ToString("yyyyMMdd") + DateTime.Now.ToString("hhmmss").PadRight(38);
        }

        private List<Vangard> GetVangargAOPCensusData()
        {
            string cmdTxt = " SELECT \"RPTEE\".\"SSNUM\", \"RPTEE\".\"LASTNAM\", \"RPTEE\".\"FIRSTNAM\", " +
                "\"RPTEE\".\"MIDINITNAM\", \"PERSON\".\"STREET1ADDR\", \"PERSON\".\"STREET2ADDR\", \r\n " +
                "\"PERSON\".\"CITYADDR\", \"PERSON\".\"STATEADDR\", \"PERSON\".\"ZIPADDR\", \"RPTEE\".\"EENUM\", " +
                "\"RPTEE\".\"BIRTHDATE\", \"RPTEE\".\"HIREDATE\", \"RPTEE\".\"TERMDATE\", \r\n" +
                " \"PLANEE2\".\"ACTLPAYFREQCD\", \"UDFDATA\".\"ALPHA023\", \"RPTEE\".\"EEPLANSTATCD\", " +
                "\"RPTEE\".\"PLANID\"\r\n FROM   ((\"SYSADM\".\"RPTEE\" \"RPTEE\" INNER JOIN " +
                "\"SYSADM\".\"PERSON\" \"PERSON\" ON \"RPTEE\".\"SSNUM\"=\"PERSON\".\"SSNUM\") INNER JOIN \r\n" +
                " \"SYSADM\".\"PLANEE2\" \"PLANEE2\" ON ((\"RPTEE\".\"YRENDDATE\"=\"PLANEE2\".\"YRENDDATE\") AND " +
                "(\"RPTEE\".\"SSNUM\"=\"PLANEE2\".\"SSNUM\")) AND \r\n" +
                " (\"RPTEE\".\"PLANID\"=\"PLANEE2\".\"PLANID\")) LEFT OUTER JOIN " +
                "\"SYSADM\".\"UDFDATA\" \"UDFDATA\" ON \"PLANEE2\".\"UDFDATAID\"=\"UDFDATA\".\"UDFDATAID\"\r\n " +
                "ORDER BY \"RPTEE\".\"SSNUM\", \"RPTEE\".\"EEPLANSTATCD\" DESC";
            try
            {
                using (OracleConnection con = new OracleConnection(ConfigurationManager.ConnectionStrings["ReliusConnectionString"].ToString()))
                using (OracleCommand cmd = con.CreateCommand())
                {
                    cmd.Connection = con;
                    cmd.CommandText = cmdTxt;
                    OracleDataAdapter adapter = new OracleDataAdapter();
                    try
                    {
                        adapter = new OracleDataAdapter(cmd);
                    }
                    catch (OracleException ex)
                    {
                        Console.WriteLine("Inner SQL Exception: " + ex.Message);
                        sLogFileEntry = ("Inner SQL Exception: " + ex.Message);
                        prtLogFile.WriteToLogFile(sLogFileEntry);
                    }
                    DataSet ds = new DataSet();
                    adapter.Fill(ds);
                    con.Close();
                    List<Vangard> vans = ds.Tables[0].AsEnumerable().Select(m => new Vangard()
                    {
                        PlanID = m.Field<string>("PLANID") ?? string.Empty,
                        SSN = m.Field<string>("SSNUM") ?? string.Empty,
                        FirstName = m.Field<string>("FIRSTNAM") ?? string.Empty,
                        MiddleName = m.Field<string>("MIDINITNAM") ?? string.Empty,
                        LastName = m.Field<string>("LASTNAM") ?? string.Empty,
                        StreetAddress1 = m.Field<string>("STREET1ADDR") ?? string.Empty,
                        StreetAddress2 = m.Field<string>("STREET2ADDR") ?? string.Empty,
                        City = m.Field<string>("CITYADDR") ?? string.Empty,
                        StateAbbr = m.Field<string>("STATEADDR") ?? string.Empty,
                        ZipCode = m.Field<string>("ZIPADDR") ?? string.Empty,
                        EENumber = m.Field<string>("EENUM") ?? string.Empty,
                        BirthDate = m.Field<DateTime?>("BIRTHDATE"),
                        HireDate = m.Field<DateTime?>("HIREDATE"),
                        TerminationDate = m.Field<DateTime?>("TERMDATE"),
                        EEPlanStatusCode = m.Field<string>("EEPLANSTATCD") ?? string.Empty,
                        ActualPaymentFrequencyCode = m.Field<string>("ACTLPAYFREQCD") ?? string.Empty
                    }).ToList();

                    return vans;
                }
            }
            catch(Exception ex)
            { 
                Console.WriteLine(ex.Message); 
                throw new Exception(ex.Message, ex);
            }
        }

        private List<ParticipantContribution> GetParticpantContributions()
        {
            string cmdtxt = "SELECT tl.ssnum as \"SSN\", tl.planid as \"PlanID\", pa.fundid as \"FundID\", sum(tl.totaldolamt) as \"TotalDollarAmount\"," +
                "TO_CHAR(tl.transeffdate, 'MM') as Month, TO_CHAR(tl.transeffdate, 'yyyy') as Year, p.firstnam as \"FirstName\", p.lastnam as \"LastName\"" +
                "from transled tl\r\n" +
                "left outer join trans t on (t.planid = tl.planid) and (t.transid=tl.transid)\r\n" +
                "left outer join planacct pa on (pa.planid = tl.planid) and (pa.acctid = tl.acctid) and " +
                "(pa.yrenddate=t.yrenddate)\r\n" +
                "left outer join person p on tl.ssnum = p.ssnum " +
                "where (tl.transeffdate between TO_DATE('" + DateTime.Now.AddDays(-5).ToString("dd-MM-yyyy") + "', 'DD-MM-YYYY') AND " +
                "TO_DATE('" + DateTime.Now.ToString("dd-MM-yyyy") + "', 'DD-MM-YYYY')) AND (t.transstatcd='P')\r\n" +
                "group by tl.ssnum, tl.planid, pa.fundid, TO_CHAR(tl.transeffdate, 'MM'), TO_CHAR(tl.transeffdate, 'yyyy'), p.lastnam, p.firstnam\r\n" +
                "having sum(tl.totaldolamt) > 0";
            //string cmdtxt = "SELECT tl.ssnum as \"SSN\", tl.planid as \"PlanID\", pa.fundid as \"FundID\", sum(tl.totaldolamt) as \"TotalDollarAmount\"," +
            //    "TO_CHAR(tl.transeffdate, 'MM') as Month, TO_CHAR(tl.transeffdate, 'yyyy') as Year " +
            //    "from transled tl\r\n" +
            //    "left outer join trans t on (t.planid = tl.planid) and (t.transid=tl.transid)\r\n" +
            //    "left outer join planacct pa on (pa.planid = tl.planid) and (pa.acctid = tl.acctid) and " +
            //    "(pa.yrenddate=t.yrenddate)\r\n" +
            //    "where (tl.transeffdate between TO_DATE('01-05-2023', 'DD-MM-YYYY') AND " +
            //    "TO_DATE('" + DateTime.Now.ToString("dd-MM-yyyy") + "', 'DD-MM-YYYY')) AND (t.transstatcd='P')\r\n" +
            //    "group by tl.ssnum, tl.planid, pa.fundid, TO_CHAR(tl.transeffdate, 'MM'), TO_CHAR(tl.transeffdate, 'yyyy')\r\n" +
            //    "having sum(tl.totaldolamt) > 0";

            using (var connection = new OracleConnection(ConfigurationManager.ConnectionStrings["ReliusConnectionString"].ToString()))
            using (var command = connection.CreateCommand())
            {
                var participantContributions = connection.Query<ParticipantContribution>(cmdtxt);
                var part = participantContributions.ToList();
                return part;
            }

        }

        private List<ParticipantContribution> GetParticpantContributions(string month, string year)
        {
            DateTime startDate = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), 1);
            DateTime endDate = startDate.AddMonths(1).AddTicks(-1);
            string cmdtxt = "SELECT tl.ssnum as \"SSN\", tl.planid as \"PlanID\", pa.fundid as \"FundID\", sum(abs(tl.totaldolamt)) as \"TotalDollarAmount\"," +
                "TO_CHAR(tl.transeffdate, 'MM') as Month, TO_CHAR(tl.transeffdate, 'yyyy') as Year, p.firstnam as \"FirstName\", p.lastnam as \"LastName\"" +
                "from transled tl\r\n" +
                "left outer join trans t on (t.planid = tl.planid) and (t.transid=tl.transid)\r\n" +
                "left outer join planacct pa on (pa.planid = tl.planid) and (pa.acctid = tl.acctid) and " +
                "(pa.yrenddate=t.yrenddate)\r\n" +
                "left outer join person p on tl.ssnum = p.ssnum " +
                "where (tl.transeffdate between TO_DATE('" + startDate.ToString("dd-MM-yyyy") + "', 'DD-MM-YYYY') AND " +
                "TO_DATE('" + endDate.ToString("dd-MM-yyyy") + "', 'DD-MM-YYYY')) AND (t.transstatcd='P')\r\n" +
                "group by tl.ssnum, tl.planid, pa.fundid, TO_CHAR(tl.transeffdate, 'MM'), TO_CHAR(tl.transeffdate, 'yyyy'), p.lastnam, p.firstnam\r\n" +
                "having sum(abs(tl.totaldolamt)) > 0";

            using (var connection = new OracleConnection(ConfigurationManager.ConnectionStrings["ReliusConnectionString"].ToString()))
            using (var command = connection.CreateCommand())
            {
                var participantContributions = connection.Query<ParticipantContribution>(cmdtxt);
                var part = participantContributions.ToList();
                return part;
            }

        }

        private List<ParticipantContribution> GetParticpantContributionsBilling()
        {
            string cmdtxt = "Select SSN, PlanId, FundId, TotalDollarAmount, Month, Year, FirstName, LastName From ParticipantContributions";

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["BillingConnectionString"].ToString()))
            using (var command = connection.CreateCommand())
            {
                var participantContributions = connection.Query<ParticipantContribution>(cmdtxt);
                var part = participantContributions.ToList();
                return part;
            }

        }

    }
}
