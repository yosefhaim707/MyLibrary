using MyLibrary.Models;

namespace MyLibrary.ViewModels
{
    public class BookViewModel
    {
        public Book? Book { get; set; }
        public List<Library>? Libraries { get; set; }
        public Library? Library { get; set; }
        public List<Shelf>? Shelves { get; set; }
        public Shelf? Shelf { get; set; }
        public int? LibraryId { get; set; }

        
    }
}
