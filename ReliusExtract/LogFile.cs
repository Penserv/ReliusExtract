using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliusExtract
{
    public class LogFile
    {
        private string sLogFileName;

        public string FileName
        {
            get
            {
                return sLogFileName;
            }
            set
            {
                if (value.Length > 0)
                    sLogFileName = value;
            }
        }

        public void WriteToLogFile(string sLogMessage)
        {
            string sNewLogMessage = string.Empty;
            bool bAppendToFile = true;          //Append the message to the existing log file.

            try
            {
                FileInfo fInfo = new FileInfo(sLogFileName);
                if (fInfo.Exists)
                {
                    if (fInfo.Length >= 262144)        //01/27/2014 KAM - Was 1048576.
                        bAppendToFile = false;
                }
                sNewLogMessage = string.Format("{0}: {1}", DateTime.Now, sLogMessage);
                using (StreamWriter sw = new StreamWriter(sLogFileName, bAppendToFile))
                {
                    sw.WriteLine(sNewLogMessage);
                    sw.Close();
                }
            }
            catch (IOException e)
            {
                Console.WriteLine(e.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }

    }
}
