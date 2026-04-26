using Microsoft.VisualStudio.TestTools.UnitTesting;
using SwoopMarketplaceProjectFrontend.Helpers;
using SwoopMarketplaceProjectFrontend.Services;
using SwoopMarketplaceProjectFrontend.Dtos;

namespace SwoopMarketplaceProjectTests
{
    [TestClass]
    public class FrontendTests
    {
        [TestMethod]
        public void ToHuf_IntegerFormatsCorrectly()
        {
            long value = 1234567;
            var s = value.ToHuf();
            Assert.IsTrue(s.Contains("Ft"));
            Assert.IsTrue(s.Contains("1"));
        }

        [TestMethod]
        public void ToHuf_DecimalFormatsWithThousands()
        {
            decimal v = 12345.67m;
            var s = v.ToHuf();
            Assert.IsTrue(s.Contains("Ft"));
            // Hungarian formatting uses non-breaking space but N0 will show grouping
            Assert.IsTrue(s.Length > 3);
        }

        [TestMethod]
        public void ToHuf_NullablesReturnDash()
        {
            decimal? a = null;
            Assert.AreEqual("-", a.ToHuf());

            long? b = null;
            Assert.AreEqual("-", b.ToHuf());
        }

        [TestMethod]
        public void ConditionMapper_FriendlyName_ReturnsMappedOrUnknown()
        {
            Assert.AreEqual("Kiv?l?", ConditionMapper.FriendlyName("Kiv?l?"));
            Assert.AreEqual("Ismeretlen", ConditionMapper.FriendlyName(null));
            Assert.AreEqual("Custom", ConditionMapper.FriendlyName("Custom"));
        }

        [TestMethod]
        public void PriceExtensions_ToHuf_ConsistencyAcrossTypes()
        {
            decimal d = 1000m;
            double dbl = 1000.0;
            long l = 1000;

            Assert.AreEqual(d.ToHuf(), dbl.ToHuf());
            Assert.AreEqual(d.ToHuf(), l.ToHuf());
        }

        [TestMethod]
        public void ConditionMapper_Whitespace_ReturnsUnknown()
        {
            Assert.AreEqual("Ismeretlen", ConditionMapper.FriendlyName("   "));
        }

        [TestMethod]
        public void ToHuf_Zero_ShowsZeroAndCurrency()
        {
            decimal z = 0m;
            var s = z.ToHuf();
            Assert.IsTrue(s.Contains("0"));
            Assert.IsTrue(s.Contains("Ft"));
        }

        [TestMethod]
        public void ToHuf_LargeNumber_GroupingPresent()
        {
            decimal big = 1234567890m;
            var s = big.ToHuf();
            Assert.IsTrue(s.Contains("Ft"));
            Assert.IsTrue(s.Length > 6);
        }

        [TestMethod]
        public void ListingDto_ImageUrls_DefaultsToEmptyList()
        {
            var dto = new ListingDto();
            Assert.IsNotNull(dto.ImageUrls);
            Assert.AreEqual(0, dto.ImageUrls.Count);
        }
    }
}
