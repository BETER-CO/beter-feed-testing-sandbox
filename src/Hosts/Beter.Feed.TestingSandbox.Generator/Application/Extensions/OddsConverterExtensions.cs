namespace Beter.Feed.TestingSandbox.Generator.Application.Extensions
{
    public static class OddsConverterExtensions
    {
        public static Dictionary<string, object> ToAllFormats(this decimal price, int malaysianDecimals = 2, int indonesianDecimals = 2)
        {
            return new Dictionary<string, object>()
            {
                { PriceFormat.Decimal.ToString("G").ToLower(), price},
                { PriceFormat.American.ToString("G").ToLower(), price.ToAmerican() },
                { PriceFormat.Fractional.ToString("G").ToLower(), price.ToFractional() },
                { PriceFormat.HongKong.ToString("G").ToLower(), price.ToHongKong() },
                { PriceFormat.Indonesian.ToString("G").ToLower(), price.ToIndonesian(indonesianDecimals) },
                { PriceFormat.Malaysian.ToString("G").ToLower(), price.ToMalaysian(malaysianDecimals ) }
            };
        }

        public static Dictionary<string, object> ToAllFormats(this decimal? price, int malaysianDecimals = 2, int indonesianDecimals = 2)
        {
            ArgumentNullException.ThrowIfNull(price);
            return price.Value.ToAllFormats(malaysianDecimals, indonesianDecimals);
        }

        private static int GreatestCommonDivisor(int a, int b)
        {
            return b == 0 ? a : GreatestCommonDivisor(b, a % b);
        }

        public static string ToFractional(this decimal price, int maxDenominator = 100)
        {
            if (price <= 1M)
            {
                return "0/1";
            }
            price -= 1;
            var wholePart = (int)price;
            var fractionalPart = price - wholePart;
            var denominator = 1;

            do
            {
                if (fractionalPart * denominator % 1 == 0)
                {
                    break;
                }

                if (denominator >= maxDenominator)
                {
                    break;
                }

                denominator *= 10;
            }
            while (true);

            var gcd = GreatestCommonDivisor((int)(fractionalPart * denominator), denominator);
            var bottom = denominator / gcd;
            var top = (int)(fractionalPart * denominator / gcd);

            var result = $"{wholePart * bottom + top}/{bottom}";
            return result;
        }

        public static string ToFractional(this decimal? price, int maxDenominator = 100)
        {
            ArgumentNullException.ThrowIfNull(price);
            return price.Value.ToFractional(maxDenominator);
        }


        public static string ToAmerican(this decimal price)
        {
            var netPayoutMultiplier = price - 1;
            if (netPayoutMultiplier == 0)
            {
                return "0";
            }

            var isPositive = price >= 2;
            decimal AmericanOdd;

            if (isPositive)
            {
                AmericanOdd = netPayoutMultiplier * 100;

                return $"+{(int)AmericanOdd}";
            }

            AmericanOdd = 100 / netPayoutMultiplier;

            return $"-{(int)AmericanOdd}";
        }

        public static string ToAmerican(this decimal? price)
        {
            ArgumentNullException.ThrowIfNull(price);
            return price.Value.ToAmerican();
        }

        public static decimal ToHongKong(this decimal price)
        {
            var HongKongOdd = price - 1;
            return Math.Round(HongKongOdd, 2);
        }

        public static decimal ToHongKong(this decimal? price)
        {
            ArgumentNullException.ThrowIfNull(price);
            return price.Value.ToHongKong();
        }

        public static decimal ToMalaysian(this decimal price, int decimals = 2)
        {
            var isPositive = price < 2;
            var netPayoutMultiplier = price - 1;
            decimal MalaysianOdd;

            if (isPositive)
            {
                MalaysianOdd = netPayoutMultiplier;
            }
            else
            {
                MalaysianOdd = -1 / netPayoutMultiplier;
            }

            return Math.Round(MalaysianOdd, decimals);
        }

        public static decimal ToMalaysian(this decimal? price, int decimals = 2)
        {
            ArgumentNullException.ThrowIfNull(price);
            return price.Value.ToMalaysian(decimals);
        }

        public static decimal ToIndonesian(this decimal price, int decimals = 2)
        {
            var netPayoutMultiplier = price - 1;
            if (netPayoutMultiplier == 0)
            {
                return Math.Round(decimal.Zero, decimals);
            }

            var isPositive = price >= 2;
            decimal IndonesianOdd;

            if (isPositive)
            {
                IndonesianOdd = netPayoutMultiplier;
            }
            else
            {
                IndonesianOdd = -1 / netPayoutMultiplier;
            }

            return Math.Round(IndonesianOdd, decimals);
        }

        public static decimal ToIndonesian(this decimal? price, int decimals = 2)
        {
            ArgumentNullException.ThrowIfNull(price);
            return price.Value.ToIndonesian(decimals);
        }
    }

    public enum PriceFormat
    {
        Decimal,
        Fractional,
        American,
        HongKong,
        Malaysian,
        Indonesian
    }
}
