using System;
using System.Collections.Generic;
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

        // DELETE: api/ListingImages/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteListingImage(long id)
        {
            var listingImage = await _context.ListingImages.FindAsync(id);
            if (listingImage == null)
            {
                return NotFound();
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
