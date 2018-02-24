using Newtonsoft.Json;

namespace Founders
{
    public class Stack
    {
        [JsonProperty("cloudcoin")]
        public CloudCoinCore.CloudCoin[] cc { get; set; }
    }
}
