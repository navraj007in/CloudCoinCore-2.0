using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Diagnostics;

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
                    Debug.WriteLine(data);
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
