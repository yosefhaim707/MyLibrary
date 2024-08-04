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
    public class BookSetsController : Controller
    {
        private readonly MyLibraryContext _context;

        public BookSetsController(MyLibraryContext context)
        {
            _context = context;
        }

        // GET: BookSets
        public async Task<IActionResult> Index()
        {
            return View(await _context.BookSet.ToListAsync());
        }

        // GET: BookSets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookSet = await _context.BookSet
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bookSet == null)
            {
                return NotFound();
            }

            return View(bookSet);
        }

        // GET: Books/Create
        public IActionResult Create()
        {
            BookSetViewModel bookSetViewModel = new BookSetViewModel();
            // Get all of the liberies into a list
            List<Library> libraries = new List<Library>();
            libraries = _context.Library.ToList();
            bookSetViewModel.Libraries = libraries;
            return View(bookSetViewModel);
        }

        // POST: Books/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookSetViewModel bookSetViewModel)
        {
            if (!IsShelves(bookSetViewModel))
            {
                ViewData["Error"] = "There is no shelves in the library";
            }
            else if (!IsHighEnough(bookSetViewModel))
            {
                ViewData["Error"] = "There is no shelves that are high enough";
            }
            else if (!IsSpace(bookSetViewModel))
            {
                ViewData["Error"] = "There is no space in the exsisting shelves";
            }
            else
            {
                int shelfId;
                if (IsLessThenTen(bookSetViewModel))
                {
                    shelfId = GetCompactShelfId(bookSetViewModel);
                }
                else
                {
                    shelfId = GetShelfId(bookSetViewModel);
                    ViewData["Error"] = "There is no shelves that have less than 10 cm of space in comparison to the book. However, we inserted the book into a non optimal shelf";
                }
                bookSetViewModel.BookSet.ShelfId = shelfId;
                _context.Add(bookSetViewModel.BookSet);
                var shelf = _context.Shelf.Find(shelfId);
                shelf.FreeSpace -= bookSetViewModel.BookSet.Width;
                _context.Update(shelf);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(bookSetViewModel);
        }

        // A function to check if there is any shelves in the library
        private bool IsShelves(BookSetViewModel bookViewModel)
        {
            bool shelves = false;
            int shelvesAmount = _context.Shelf
                .Where(s => s.LibraryId == bookViewModel.LibraryId)
                .Count();
            if (shelvesAmount > 0)
            {
                shelves = true;
            }
            return shelves;
        }

        // A function to check if there is any shelves that are high enough
        private bool IsHighEnough(BookSetViewModel bookSetViewModel)
        {
            bool highEnough = false;
            List<Shelf> shelves = _context.Shelf
                .Where(s => s.LibraryId == bookSetViewModel.LibraryId)
                .ToList();
            foreach (Shelf shelf in shelves)
            {
                if (shelf.Height >= bookSetViewModel.BookSet.Height)
                {
                    highEnough = true;
                    break;
                }
            }
            return highEnough;
        }

        // A function to check if there is any space in the exsisting shelves
        private bool IsSpace(BookSetViewModel bookViewModel)
        {
            bool space = false;
            List<Shelf> shelves = _context.Shelf
                .Where(s => s.LibraryId == bookViewModel.LibraryId)
                .ToList();
            foreach (Shelf shelf in shelves)
            {
                if (shelf.FreeSpace > bookViewModel.BookSet.Width)
                {
                    space = true;
                    break;
                }
            }
            return space;
        }

        // A function that checks if there is any shelves that have less than 10 cm of space in comparison to the book
        private bool IsLessThenTen(BookSetViewModel bookViewModel)
        {
            bool tenLess = false;
            List<Shelf> shelves = _context.Shelf
                .Where(s => s.LibraryId == bookViewModel.LibraryId)
                .ToList();
            foreach (Shelf shelf in shelves)
            {
                if (bookViewModel.BookSet.Height + 10 > shelf.Height)
                {
                    tenLess = true;
                }
            }
            return tenLess;
        }

        // A function that returns the id of the first shelf that has enough space
        private int GetShelfId(BookSetViewModel bookSetViewModel)
        {
            int shelfId = 0;
            List<Shelf> shelves = _context.Shelf
                .Where(s => s.LibraryId == bookSetViewModel.LibraryId)
                .ToList();
            foreach (Shelf shelf in shelves)
            {
                if (shelf.FreeSpace > 0)
                {
                    shelfId = shelf.Id;
                    break;
                }
            }
            return shelfId;
        }

        // A function that returns the id of the first shelf that has less than 10 cm of space in comparison to the book
        private int GetCompactShelfId(BookSetViewModel bookViewModel)
        {
            int shelfId = 0;
            List<Shelf> shelves = _context.Shelf
                .Where(s => s.LibraryId == bookViewModel.LibraryId)
                .ToList();
            foreach (Shelf shelf in shelves)
            {
                if (bookViewModel.BookSet.Height + 10 > shelf.Height)
                {
                    shelfId = shelf.Id;
                    break;
                }
            }
            return shelfId;
        }


        // GET: BookSets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookSet = await _context.BookSet.FindAsync(id);
            if (bookSet == null)
            {
                return NotFound();
            }
            return View(bookSet);
        }

        // POST: BookSets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Width,Height")] BookSet bookSet)
        {
            if (id != bookSet.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bookSet);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookSetExists(bookSet.Id))
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
            return View(bookSet);
        }

        // GET: BookSets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookSet = await _context.BookSet
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bookSet == null)
            {
                return NotFound();
            }

            return View(bookSet);
        }

        // POST: BookSets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bookSet = await _context.BookSet.FindAsync(id);
            if (bookSet != null)
            {
                _context.BookSet.Remove(bookSet);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookSetExists(int id)
        {
            return _context.BookSet.Any(e => e.Id == id);
        }
    }
}
