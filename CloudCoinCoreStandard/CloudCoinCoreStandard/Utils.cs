using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Linq;

namespace CloudCoinCore
{
    public class Utils
    {
        public static CloudCoin[] LoadJson(string filename)
        {
            try
            {
                using (StreamReader r = (File.OpenText(filename)))
                {
                    string json = r.ReadToEnd();
                    Stack coins = JsonConvert.DeserializeObject<Stack>(json);
                    return coins.cc;
                }
            }
            catch(Exception e)
            {
                return null;
            }
        }


        public static string WriteObjectToString()
        {
            MemoryStream ms = new MemoryStream();

            // Serializer the User object to the stream.  
            return "";
        }
        private static Random random = new Random();

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static async Task<String> GetHtmlFromURL(String urlAddress)
        {
            string data = "";
            try
            {
                using (var cli = new HttpClient())
                {
                    HttpResponseMessage response = await cli.GetAsync(urlAddress);
                    if (response.IsSuccessStatusCode)
                        data = await response.Content.ReadAsStringAsync();
                    //Debug.WriteLine(data);
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return data;
        }//end get HTML

    }
}
