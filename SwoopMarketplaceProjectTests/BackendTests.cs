using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Reflection;

namespace SwoopMarketplaceProjectTests
{
    [TestClass]
    public class BackendTests
    {
        // Ensure ListingsController has ApiController and Route attributes
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

        // Ensure ListingImagesController defines a Route attribute
        [TestMethod]
        public void ListingImagesController_ShouldHave_Route()
        {
            var asm = Assembly.Load("SwoopMarketplaceProjectBackendAPI");
            var type = asm.GetType("SwoopMarketplaceProjectBackendAPI.Controllers.ListingImagesController");
            Assert.IsNotNull(type, "ListingImagesController type cannot be found");

            var routeAttr = type.GetCustomAttributes(false).FirstOrDefault(a => a.GetType().Name == "RouteAttribute");
            Assert.IsNotNull(routeAttr, "ListingImagesController should have a [Route(...)] attribute");
        }

        // Ensure CategoriesController defines a Route attribute
        [TestMethod]
        public void CategoriesController_ShouldHave_Route()
        {
            var asm = Assembly.Load("SwoopMarketplaceProjectBackendAPI");
            var type = asm.GetType("SwoopMarketplaceProjectBackendAPI.Controllers.CategoriesController");
            Assert.IsNotNull(type, "CategoriesController type cannot be found");

            var routeAttr = type.GetCustomAttributes(false).FirstOrDefault(a => a.GetType().Name == "RouteAttribute");
            Assert.IsNotNull(routeAttr, "CategoriesController should have a [Route(...)] attribute");
        }

        // Ensure ListingViewsController is annotated as ApiController
        [TestMethod]
        public void ListingViewsController_ShouldHave_ApiController()
        {
            var asm = Assembly.Load("SwoopMarketplaceProjectBackendAPI");
            var type = asm.GetType("SwoopMarketplaceProjectBackendAPI.Controllers.ListingViewsController");
            Assert.IsNotNull(type, "ListingViewsController type cannot be found");

            var apiAttr = type.GetCustomAttributes(false).FirstOrDefault(a => a.GetType().Name == "ApiControllerAttribute");
            Assert.IsNotNull(apiAttr, "ListingViewsController should be annotated with [ApiController]");
        }

        [TestMethod]
        public void AuthAndUsersAndMessagesControllers_Exist()
        {
            var asm = Assembly.Load("SwoopMarketplaceProjectBackendAPI");
            var auth = asm.GetType("SwoopMarketplaceProjectBackendAPI.Controllers.AuthController");
            var users = asm.GetType("SwoopMarketplaceProjectBackendAPI.Controllers.UsersController");
            var msgs = asm.GetType("SwoopMarketplaceProjectBackendAPI.Controllers.MessagesController");

            Assert.IsNotNull(auth, "AuthController missing");
            Assert.IsNotNull(users, "UsersController missing");
            Assert.IsNotNull(msgs, "MessagesController missing");
        }

        [TestMethod]
        public void ListingsController_Contains_CommonActionNames()
        {
            var asm = Assembly.Load("SwoopMarketplaceProjectBackendAPI");
            var type = asm.GetType("SwoopMarketplaceProjectBackendAPI.Controllers.ListingsController");
            Assert.IsNotNull(type, "ListingsController type cannot be found");

            var names = type.GetMethods(BindingFlags.Public | BindingFlags.Instance).Select(m => m.Name).ToList();
            Assert.IsTrue(names.Contains("GetListings"), "GetListings action missing");
            Assert.IsTrue(names.Contains("GetListing"), "GetListing action missing");
            Assert.IsTrue(names.Contains("PostListing") || names.Contains("PostListing"), "PostListing action missing");
            Assert.IsTrue(names.Contains("PutListing"), "PutListing action missing");
            Assert.IsTrue(names.Contains("DeleteListing"), "DeleteListing action missing");
        }

        [TestMethod]
        public void ListingImagesController_HasUploadAndSetPrimary()
        {
            var asm = Assembly.Load("SwoopMarketplaceProjectBackendAPI");
            var type = asm.GetType("SwoopMarketplaceProjectBackendAPI.Controllers.ListingImagesController");
            Assert.IsNotNull(type, "ListingImagesController type cannot be found");

            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance).Select(m => m.Name).ToList();
            Assert.IsTrue(methods.Contains("UploadListingImage") || methods.Contains("UploadAsync") || methods.Contains("Upload"), "Upload endpoint method not found");
            Assert.IsTrue(methods.Contains("SetPrimary"), "SetPrimary endpoint not found");
        }

        [TestMethod]
        public void Models_Contain_ExpectedProperties()
        {
            var modelAsm = Assembly.Load("SwoopMarketplaceProject");
            var listing = modelAsm.GetType("SwoopMarketplaceProject.Models.Listing");
            var listingImage = modelAsm.GetType("SwoopMarketplaceProject.Models.ListingImage");

            Assert.IsNotNull(listing, "Listing model missing");
            Assert.IsNotNull(listingImage, "ListingImage model missing");

            var prop = listing.GetProperty("ListingImages");
            Assert.IsNotNull(prop, "Listing.ListingImages property missing");

            var idProp = listingImage.GetProperty("ImageUrl");
            Assert.IsNotNull(idProp, "ListingImage.ImageUrl property missing");

            // context existence
            var ctx = Assembly.Load("SwoopMarketplaceProject").GetType("SwoopMarketplaceProject.Models.SwoopContext");
            Assert.IsNotNull(ctx, "SwoopContext missing");
        }
    }
}
