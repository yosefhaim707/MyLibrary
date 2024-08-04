using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using MyLibrary.Data;
using MyLibrary.Models;
using MyLibrary.ViewModels;

namespace MyLibrary.Controllers
{
    public class BooksController : Controller
    {
        private readonly MyLibraryContext _context;

        public BooksController(MyLibraryContext context)
        {
            _context = context;
        }

        // GET: Books
        public async Task<IActionResult> Index()
        {
            return View(await _context.Book.ToListAsync());
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // GET: Books/Create
        public IActionResult Create()
        {
            BookViewModel bookViewModel = new BookViewModel();
            // Get all of the liberies into a list
            List<Library> libraries = new List<Library>();
            libraries = _context.Library.ToList();
            bookViewModel.Libraries = libraries;
            ViewData["List"] = libraries;
            return View(bookViewModel);
        }

        // POST: Books/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookViewModel bookViewModel)
        {
            if (!IsShelves(bookViewModel))
            {
                ViewData["Error"] = "There is no shelves in the library";
            }
            else if (!IsHighEnough(bookViewModel))
            {
                ViewData["Error"] = "There is no shelves that are high enough";
            }
            else if (!IsSpace(bookViewModel))
            {
                ViewData["Error"] = "There is no space in the exsisting shelves";
            }
            else
            {
                int shelfId;
                if (IsLessThenTen(bookViewModel))
                {
                    shelfId = GetCompactShelfId(bookViewModel);
                }
                else
                {
                    shelfId = GetShelfId(bookViewModel);
                    ViewData["Error"] = "There is no shelves that have less than 10 cm of space in comparison to the book. However, we inserted the book into a non optimal shelf";
                }
                bookViewModel.Book.ShelfId = shelfId;
                _context.Add(bookViewModel.Book);
                var shelf = _context.Shelf.Find(shelfId);
                shelf.FreeSpace -= bookViewModel.Book.Width;
                _context.Update(shelf);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["List"] = _context.Library.ToList();
            return View(bookViewModel);
        }

        // A function to check if there is any shelves in the library
        private bool IsShelves(BookViewModel bookViewModel)
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
        private bool IsHighEnough(BookViewModel bookViewModel)
        {
            bool highEnough = false;
            List<Shelf> shelves = _context.Shelf
                .Where(s => s.LibraryId == bookViewModel.LibraryId)
                .ToList();
            foreach (Shelf shelf in shelves)
            {
                if (shelf.Height >= bookViewModel.Book.Height)
                {
                    highEnough = true;
                    break;
                }
            }
            return highEnough;
        }

        // A function to check if there is any space in the exsisting shelves
        private bool IsSpace(BookViewModel bookSetViewModel)
        {
            bool space = false;
            List<Shelf> shelves = _context.Shelf
                .Where(s => s.LibraryId == bookSetViewModel.LibraryId)
                .ToList();
            foreach(Shelf shelf in shelves)
            {
                if (shelf.FreeSpace >= bookSetViewModel.Book.Width)
                {
                    space = true;
                    break;
                }
            }
            return space;
        }

        // A function that checks if there is any shelves that have less than 10 cm of space in comparison to the book
        private bool IsLessThenTen(BookViewModel bookViewModel)
        {
            bool tenLess = false;
            List<Shelf> shelves = _context.Shelf
                .Where(s => s.LibraryId == bookViewModel.LibraryId)
                .ToList();
            foreach (Shelf shelf in shelves)
            {
                if (bookViewModel.Book.Height + 10 > shelf.Height)
                {
                    tenLess = true;
                }
            }
            return tenLess;
        }

        // A function that returns the id of the first shelf that has enough space
        private int GetShelfId(BookViewModel bookViewModel)
        {
            int shelfId = 0;
            List<Shelf> shelves = _context.Shelf
                .Where(s => s.LibraryId == bookViewModel.LibraryId)
                .ToList();
            foreach (Shelf shelf in shelves)
            {
                if (shelf.FreeSpace > bookViewModel.Book.Width)
                {
                    shelfId = shelf.Id;
                    break;
                }
            }
            return shelfId;
        }

        // A function that returns the id of the first shelf that has less than 10 cm of space in comparison to the book
        private int GetCompactShelfId(BookViewModel bookViewModel)
        {
            int shelfId = 0;
            List<Shelf> shelves = _context.Shelf
                .Where(s => s.LibraryId == bookViewModel.LibraryId)
                .ToList();
            foreach (Shelf shelf in shelves)
            {
                if (bookViewModel.Book.Height + 10 > shelf.Height && shelf.FreeSpace > bookViewModel.Book.Width)
                {
                    shelfId = shelf.Id;
                    break;
                }
            }
            return shelfId;
        }



        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }

        // POST: Books/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Width,Height")] Book book)
        {
            if (id != book.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.Id))
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
            return View(book);
        }

        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Book.FindAsync(id);
            if (book != null)
            {
                var shelf = _context.Shelf.Find(book.ShelfId);
                shelf.FreeSpace += book.Width;
                _context.Update(shelf);
                _context.Book.Remove(book);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(int id)
        {
            return _context.Book.Any(e => e.Id == id);
        }
    }
}
