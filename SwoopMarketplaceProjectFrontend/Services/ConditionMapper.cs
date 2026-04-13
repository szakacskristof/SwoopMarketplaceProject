using System.Collections.Generic;

namespace SwoopMarketplaceProjectFrontend.Services
{
    public static class ConditionMapper
    {
        private static readonly IReadOnlyDictionary<string, string> Map = new Dictionary<string, string>
        {
            ["Új"] = "Új",
            ["Kiváló"] = "Kiváló",
            ["Kielégítõ"] = "Kielégítõ",
            ["Használt"] = "Használt",
            ["Hibás"] = "Hibás"

        };

        public static string FriendlyName(string? code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return "Ismeretlen";
            return Map.TryGetValue(code, out var name) ? name : code;
        }
    }
}