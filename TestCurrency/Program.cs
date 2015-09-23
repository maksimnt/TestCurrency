using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TestCurrency
{
    class Program
    {
        static void Main(string[] args)
        {
            DateTime date = new DateTime(2015, 09, 21);
            DateTime dateEnd = new DateTime(2015, 09, 24);

            while (date < dateEnd)
            {
                RequestToSite(date);
                date = date.AddDays(1);
            }

            Console.ReadKey();
        }

        public static void InsertToDB(string name, string buy, string sell, DateTime date)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["db"].ToString();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand
                    {
                        CommandType = CommandType.Text,
                        Connection = connection
                    };
                    
                    cmd.CommandText =
                        "Insert Into Currency(Name, Buy, Sell, Date)" +
                        "Values(@P1, @P2, @P3, @P4)";
                    
                    cmd.Parameters.AddWithValue("@P1", name);
                    cmd.Parameters.AddWithValue("@P2", Convert.ToDecimal(buy.Replace('.',',')));
                    cmd.Parameters.AddWithValue("@P3", Convert.ToDecimal(sell.Replace('.',',')));
                    cmd.Parameters.AddWithValue("@P4", date);

                    connection.Open();
                    cmd.ExecuteNonQuery();
                    connection.Close();

                }
                catch (SqlException ex)
                {
                    Console.WriteLine(ex.Message);

                }

            }
        }

        public static void RequestToSite(DateTime date)
        {
            try
            {
                // Create a request for the URL. 
                string url = "http://minfin.com.ua/currency/banks/usd/";
                url = url + date.ToString("yyyy-MM-dd") + "/";
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest; //
                // If required by the server, set the credentials.
                request.Credentials = CredentialCache.DefaultCredentials;
                // Get the response.
                request.UserAgent = "Foo";
                request.Accept = "*/*";
                WebResponse response = request.GetResponse();
                // Display the status.
                Console.WriteLine(((HttpWebResponse)response).StatusDescription);
                // Get the stream containing content returned by the server.
                Stream dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                string responseFromServer = reader.ReadToEnd();
                string tableStr = responseFromServer.Substring(responseFromServer.IndexOf("<tbody class=\"list\">"));
                tableStr = tableStr.Substring(0, tableStr.IndexOf("tbody>"));
                // Display the content.
                //Console.WriteLine(responseFromServer);
                // Clean up the streams and the response.
                reader.Close();
                response.Close();

                //Dictionary<string, string> dic = new Dictionary<string, string>();
                int posTr = tableStr.IndexOf("<tr>");
                do
                {
                    string name = tableStr.Substring(tableStr.IndexOf("</span>"), tableStr.IndexOf("</a>") - tableStr.IndexOf("</span>"));
                    name = name.Substring(name.IndexOf(">") + 1);

                    tableStr = tableStr.Substring(tableStr.IndexOf("</td>") + 4);

                    string kursBuy = tableStr.Substring(tableStr.IndexOf("mfm-pr0\">"), tableStr.IndexOf("</td>") - tableStr.IndexOf("mfm-pr0\">"));
                    kursBuy = kursBuy.Substring(kursBuy.IndexOf(">") + 1);

                    tableStr = tableStr.Substring(tableStr.IndexOf("</td>") + 4);
                    tableStr = tableStr.Substring(tableStr.IndexOf("</td>") + 4);

                    string kursSell = tableStr.Substring(tableStr.IndexOf("mfm-pl0\">"), tableStr.IndexOf("</td>") - tableStr.IndexOf("mfm-pl0\">"));
                    kursSell = kursSell.Substring(kursSell.IndexOf(">") + 1);

                    InsertToDB(name, kursBuy, kursSell, date);

                    //dic.Add(name, kursBuy);
                    tableStr = tableStr.Substring(tableStr.IndexOf("</tr>") + 4);
                    posTr = tableStr.IndexOf("<tr>");
                } while (posTr > 0);


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

    }
}
