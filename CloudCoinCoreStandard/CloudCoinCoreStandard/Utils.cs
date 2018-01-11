using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CloudCoinCore
{
    public class Utils
    {
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
