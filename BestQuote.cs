using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synapse.Crypto.FastTrader
{
    public readonly record struct BestQuote(long Time, double Ask, double Bid, double WAAsk, double WABid) 
    {
        public override string ToString()
        {
            return $"{Time};{Ask};{Bid};{WAAsk};{WABid}"; 
        }

        public static BestQuote? Parse(string line)
        {
            if (string.IsNullOrEmpty(line)) return null;
            var arr = line.Split(";");
            if (arr.Length != 5) return null;
            return new BestQuote(long.Parse(arr[0]), double.Parse(arr[1]), double.Parse(arr[2]), double.Parse(arr[3]), double.Parse(arr[4]));
        }

        public BestQuote Copy()
        {
            return new(this.Time, this.Ask, this.Bid, this.WAAsk, this.WABid); 
        }

    }

    public readonly record struct Spread(DateTime Time, double Sell, double Buy, double WASell, double WABuy)
    {
        public override string ToString()
        {
            return $"{Time};{Sell};{Buy};{WASell};{WABuy}";
        }

        public static Spread? Parse(string line)
        {
            if (string.IsNullOrEmpty(line)) return null;
            var arr = line.Split(";");
            if (arr.Length != 5) return null;
            return new Spread(DateTime.Parse(arr[0]), double.Parse(arr[1]), double.Parse(arr[2]), double.Parse(arr[3]), double.Parse(arr[4]));
        }

        public Spread Copy()
        {
            return new(this.Time, this.Sell, this.Buy, this.WASell, this.WABuy);
        }

    }


}
