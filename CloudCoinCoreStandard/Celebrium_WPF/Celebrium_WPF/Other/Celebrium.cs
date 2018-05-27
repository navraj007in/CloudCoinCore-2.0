using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Celebrium_WPF.Other
{
    public class Celebrium
    {
        [JsonProperty("server")]
        public string Server { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("jpeg")]
        public string Base64 { get; set; }

        [JsonProperty("exp_date")]
        public string ExpiryDate { get; set; }

        [JsonProperty("time")]
        public string Time { get; set; }

        [JsonProperty("meta")]
        public CelebriumMeta meta { get; set; }
    }

    public class CelebriumMeta
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("dbf_id")]
        public string dbf_id { get; set; }

        [JsonProperty("series_name")]
        public string SeriesName { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("genre")]
        public string Genre { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("artist")]
        public string Artist { get; set; }

        [JsonProperty("number")]
        public string number { get; set; }

        [JsonProperty("of_total")]
        public string OfTotal { get; set; }

        [JsonProperty("issuer")]
        public string Issuer { get; set; }

        [JsonProperty("date_of_creation")]
        public string CreationDate { get; set; }

        [JsonProperty("date_of_issue")]
        public string IssueDate { get; set; }

        [JsonProperty("date_modified")]
        public string ModifyDate { get; set; }

       


    }
}
