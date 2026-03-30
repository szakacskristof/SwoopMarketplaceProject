using System.Collections.Generic;

namespace SwoopMarketplaceProjectFrontend.Services
{
    public static class ConditionMapper
    {
        private static readonly IReadOnlyDictionary<string, string> Map = new Dictionary<string, string>
        {
            ["fn"] = "Gyári új",
            ["mw"] = "Kevéssé használt",
            ["ft"] = "Harctéren tesztelt",
            ["ww"] = "Viseltes",
            ["bs"] = "Háború tépázta"
        };

        public static string FriendlyName(string? code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return "Ismeretlen";
            return Map.TryGetValue(code, out var name) ? name : code;
        }
    }
}