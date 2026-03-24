using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwoopMarketplaceProject.Models;

namespace SwoopMarketplaceProjectBackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ListingImagesController : ControllerBase
    {
        private readonly SwoopContext _context;

        public ListingImagesController(SwoopContext context)
        {
            _context = context;
        }

        // GET: api/ListingImages
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ListingImage>>> GetListingImages()
        {
            return await _context.ListingImages.ToListAsync();
        }

        // GET: api/ListingImages/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ListingImage>> GetListingImage(long id)
        {
            var listingImage = await _context.ListingImages.FindAsync(id);

            if (listingImage == null)
            {
                return NotFound();
            }

            return listingImage;
        }

        // PUT: api/ListingImages/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutListingImage(long id, ListingImage listingImage)
        {
            if (id != listingImage.Id)
            {
                return BadRequest();
            }

            _context.Entry(listingImage).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ListingImageExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/ListingImages
        [HttpPost]
        public async Task<ActionResult<ListingImage>> PostListingImage(ListingImage listingImage)
        {
            _context.ListingImages.Add(listingImage);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetListingImage", new { id = listingImage.Id }, listingImage);
        }

        public class UploadListingImageRequest
        {
            public long ListingId { get; set; }
            public IFormFile File { get; set; } = null!;
        }

        // POST: api/ListingImages/upload
        // Accepts multipart/form-data with 'file' and 'listingId' -> saves file to wwwroot/images and stores relative URL in DB
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ListingImage>> UploadListingImage([FromForm] UploadListingImageRequest request)
        {
            if (request.File == null || request.File.Length == 0)
                return BadRequest("No file uploaded.");

            var listing = await _context.Listings.FindAsync(request.ListingId);
            if (listing == null)
                return BadRequest("Listing not found.");

            var wwwroot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var imagesDir = Path.Combine(wwwroot, "images");
            if (!Directory.Exists(imagesDir))
                Directory.CreateDirectory(imagesDir);

            var ext = Path.GetExtension(request.File.FileName);
            var fileName = $"{Guid.NewGuid():N}{(string.IsNullOrEmpty(ext) ? "" : ext)}";
            var filePath = Path.Combine(imagesDir, fileName);

            await using (var stream = System.IO.File.Create(filePath))
            {
                await request.File.CopyToAsync(stream);
            }

            // IMPORTANT: store a web-friendly relative URL (no "wwwroot" or physical path).
            // If your app is hosted under a path base, the browser will resolve relative URLs properly.
            var relativeUrl = $"/images/{fileName}";

            // Determine primary image: if this listing has no images yet, mark primary = true
            var hasAny = await _context.ListingImages.AnyAsync(li => li.ListingId == request.ListingId);
            var listingImage = new ListingImage
            {
                ListingId = request.ListingId,
                ImageUrl = relativeUrl,
                IsPrimary = hasAny ? false : true
            };

            _context.ListingImages.Add(listingImage);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetListingImage", new { id = listingImage.Id }, listingImage);
        }

        // DELETE: api/ListingImages/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteListingImage(long id)
        {
            var listingImage = await _context.ListingImages.FindAsync(id);
            if (listingImage == null)
            {
                return NotFound();
            }

            // optionally delete file from disk
            try
            {
                var wwwroot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                string localPath = listingImage.ImageUrl ?? string.Empty;

                // If the stored URL is absolute, extract the local path part
                if (Uri.TryCreate(localPath, UriKind.Absolute, out var uri))
                {
                    localPath = uri.LocalPath;
                }

                // Ensure we remove any leading application segment if present, then map to wwwroot
                // localPath now looks like "/images/xxx" (or may include app base, but LocalPath handles it)
                var path = Path.Combine(wwwroot, localPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
            }
            catch
            {
                // ignore file deletion errors
            }

            _context.ListingImages.Remove(listingImage);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ListingImageExists(long id)
        {
            return _context.ListingImages.Any(e => e.Id == id);
        }
    }
}