using Newtonsoft.Json;
namespace CloudCoinCore
{
    public class Stack
    {
        public Stack()
        {

        }
        public Stack(CloudCoin coin)
        {
            cc = new CloudCoin[1];
            cc[0] = coin;
        }
        [JsonProperty("cloudcoin")]
        public CloudCoin[] cc { get; set; }
    }
}
