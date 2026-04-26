using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Reflection;

namespace SwoopMarketplaceProjectTests
{
    [TestClass]
    public class BackendControllerAttributeTests
    {
        // Ensure that critical backend controllers define expected route and api metadata
        [TestMethod]
        public void ListingsController_ShouldHave_ApiController_And_Route()
        {
            var asm = Assembly.Load("SwoopMarketplaceProjectBackendAPI");
            var type = asm.GetType("SwoopMarketplaceProjectBackendAPI.Controllers.ListingsController");
            Assert.IsNotNull(type, "ListingsController type cannot be found");

            var apiAttr = type.GetCustomAttributes(false).FirstOrDefault(a => a.GetType().Name == "ApiControllerAttribute");
            Assert.IsNotNull(apiAttr, "ListingsController should be annotated with [ApiController]");

            var routeAttr = type.GetCustomAttributes(false).FirstOrDefault(a => a.GetType().Name == "RouteAttribute");
            Assert.IsNotNull(routeAttr, "ListingsController should have a [Route(...)] attribute");
        }

        [TestMethod]
        public void ListingImagesController_ShouldHave_Route()
        {
            var asm = Assembly.Load("SwoopMarketplaceProjectBackendAPI");
            var type = asm.GetType("SwoopMarketplaceProjectBackendAPI.Controllers.ListingImagesController");
            Assert.IsNotNull(type, "ListingImagesController type cannot be found");

            var routeAttr = type.GetCustomAttributes(false).FirstOrDefault(a => a.GetType().Name == "RouteAttribute");
            Assert.IsNotNull(routeAttr, "ListingImagesController should have a [Route(...)] attribute");
        }
    }
}
