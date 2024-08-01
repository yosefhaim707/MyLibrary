using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyLibrary.Data;
using MyLibrary.Models;
using MyLibrary.ViewModels;

namespace MyLibrary.Controllers
{
    public class LibrariesController : Controller
    {
        private readonly MyLibraryContext _context;

        public LibrariesController(MyLibraryContext context)
        {
            _context = context;
        }

        // GET: Libraries
        public async Task<IActionResult> Index()
        {
            return View(await _context.Library.ToListAsync());
        }

        // GET: Libraries/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            // Create a new LibraryViewModel object
            Library? library = await _context.Library
                .FirstOrDefaultAsync(m => m.Id == id);
            List<Shelf>? shelves;
            // Checks if the shelves list in the library object is not null
            if (library.Shelves != null)
            {
                shelves = library.Shelves;
            }
            else
            {
                shelves = new List<Shelf>();
                // Get all the shelves that belong to the library with the given id
                shelves = await _context.Shelf.Where(s => s.LibraryId == id).ToListAsync();
            }
            var viewModel = new LibraryViewModel
            {
                Library = library,
                Shelves = shelves
            };

            if (library == null)
            {
                return NotFound();
            }

            return View(viewModel);
        }

        // GET: Libraries/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Libraries/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Genre")] Library library)
        {
            if (ModelState.IsValid)
            {
                _context.Add(library);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(library);
        }

        // GET: Libraries/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var library = await _context.Library.FindAsync(id);
            if (library == null)
            {
                return NotFound();
            }
            return View(library);
        }

        // POST: Libraries/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Genre")] Library library)
        {
            if (id != library.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(library);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LibraryExists(library.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(library);
        }

        // GET: Libraries/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var library = await _context.Library
                .FirstOrDefaultAsync(m => m.Id == id);
            if (library == null)
            {
                return NotFound();
            }

            return View(library);
        }

        // POST: Libraries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var library = await _context.Library.FindAsync(id);
            if (library != null)
            {
                _context.Library.Remove(library);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Libraries/CreateShelf
        public async Task<IActionResult> CreateShelves(int Id)
        {
            LibraryViewModel libraryViewModel = new LibraryViewModel();
            libraryViewModel.Library = await _context.Library.FirstOrDefaultAsync(m => m.Id == Id);
            return View(libraryViewModel);
        }


        // POST: Libraries/CreateShelf
        // Create a new shelf and add it to the library that has the given libraryId
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateShelf(LibraryViewModel libraryViewModel)
        {
            if (!ModelState.IsValid)
            {
                Shelf shelf = libraryViewModel.Shelf;
                shelf.FreeSpace = shelf.Width;
                _context.Add(shelf);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = libraryViewModel.Shelf.LibraryId });
            }
            return View();
        }


        private bool LibraryExists(int id)
        {
            return _context.Library.Any(e => e.Id == id);
        }
    }
}
