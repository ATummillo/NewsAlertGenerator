using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsAlertGenerator.Models
{
    class Coin
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
        public string Keyword { get; set; }

        public string Feed { get; set; }

        public Coin(string s)
        {
            var splits = s.Split(',');
            ID = splits[0];
            Name = splits[1];
            Symbol = splits[2];
            Keyword = splits[3];
        }
    }
}
