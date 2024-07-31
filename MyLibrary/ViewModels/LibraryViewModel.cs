using MyLibrary.Models;

namespace MyLibrary.ViewModels
{
    public class LibraryViewModel
    {
        public Library? Library { get; set; }
        public List<Shelf> Shelves { get; set; }
    }
}
