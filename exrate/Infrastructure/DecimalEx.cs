using System;
using System.Globalization;

namespace Exrate.Infrastructure
{
    public static class DecimalEx
    {
        public static decimal Parse(string str)
        {
            str = str.Replace(',', '.');
            return Decimal.Parse(str, NumberStyles.Any, CultureInfo.InvariantCulture);
        }
    }
}