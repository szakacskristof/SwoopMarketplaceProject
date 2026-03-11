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
    public class ListingViewsController : ControllerBase
    {
        private readonly SwoopContext _context;

        public ListingViewsController(SwoopContext context)
        {
            _context = context;
        }

        // GET: api/ListingViews
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ListingView>>> GetListingViews()
        {
            return await _context.ListingViews.ToListAsync();
        }

        // GET: api/ListingViews/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ListingView>> GetListingView(int id)
        {
            var listingView = await _context.ListingViews.FindAsync(id);

            if (listingView == null)
            {
                return NotFound();
            }

            return listingView;
        }

        // PUT: api/ListingViews/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutListingView(int id, ListingView listingView)
        {
            if (id != listingView.Id)
            {
                return BadRequest();
            }

            _context.Entry(listingView).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ListingViewExists(id))
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

        // POST: api/ListingViews
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ListingView>> PostListingView(ListingView listingView)
        {
            _context.ListingViews.Add(listingView);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ListingViewExists(listingView.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetListingView", new { id = listingView.Id }, listingView);
        }

        // DELETE: api/ListingViews/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteListingView(int id)
        {
            var listingView = await _context.ListingViews.FindAsync(id);
            if (listingView == null)
            {
                return NotFound();
            }

            _context.ListingViews.Remove(listingView);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ListingViewExists(int id)
        {
            return _context.ListingViews.Any(e => e.Id == id);
        }
    }
}
