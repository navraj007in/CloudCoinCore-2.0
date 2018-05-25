using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudCoinCore;
using Newtonsoft.Json;

namespace Celebrium_WPF.CoreClasses
{
    class CelebriumCoin : CloudCoin
    {

        [JsonIgnore]
        public new string FileName
        {
            get
            {
                return this.getDenomination() + ".Celebrium." + nn + "." + sn + ".";
            }
        }
    }
}
