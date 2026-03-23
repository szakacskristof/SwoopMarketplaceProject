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
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
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
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ListingImage>> PostListingImage(ListingImage listingImage)
        {
            _context.ListingImages.Add(listingImage);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetListingImage", new { id = listingImage.Id }, listingImage);
        }

        // New: POST: api/ListingImages/upload
        // Accepts multipart/form-data with 'file' and 'listingId' -> saves file to wwwroot/images and stores relative URL in DB
        [HttpPost("upload")]
        public async Task<ActionResult<ListingImage>> UploadListingImage([FromForm] long listingId, [FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            // ensure the listing exists
            var listing = await _context.Listings.FindAsync(listingId);
            if (listing == null)
                return BadRequest("Listing not found.");

            // prepare images folder under wwwroot/images
            var wwwroot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var imagesDir = Path.Combine(wwwroot, "images");
            if (!Directory.Exists(imagesDir))
                Directory.CreateDirectory(imagesDir);

            // sanitize and generate unique file name
            var ext = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid():N}{(string.IsNullOrEmpty(ext) ? "" : ext)}";
            var filePath = Path.Combine(imagesDir, fileName);

            // save file to disk
            await using (var stream = System.IO.File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }

            // build a relative URL that can be used by the frontend
            var relativeUrl = $"/images/{fileName}";

            var listingImage = new ListingImage
            {
                ListingId = listingId,
                ImageUrl = relativeUrl,
                IsPrimary = null
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
                var path = Path.Combine(wwwroot, listingImage.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
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
