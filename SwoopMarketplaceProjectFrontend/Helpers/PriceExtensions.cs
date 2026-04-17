using System.Globalization;

namespace SwoopMarketplaceProjectFrontend.Helpers
{
    public static class PriceExtensions
    {
        private static readonly CultureInfo Hu = new("hu-HU");

        public static string ToHuf(this decimal value)
            => string.Format(Hu, "{0:N0} Ft", value);

        public static string ToHuf(this double value)
            => ((decimal)value).ToHuf();

        public static string ToHuf(this long value)
            => ((decimal)value).ToHuf();

        public static string ToHuf(this decimal? value)
            => value.HasValue ? value.Value.ToHuf() : "-";

        public static string ToHuf(this double? value)
            => value.HasValue ? value.Value.ToHuf() : "-";

        public static string ToHuf(this long? value)
            => value.HasValue ? value.Value.ToHuf() : "-";
    }
}