using MyLibrary.Models;

namespace MyLibrary.ViewModels
{
    public class ShelfViewModel
    {
        public Shelf Shelf { get; set; }
        public List<Library> libraries { get; set; }
    }
}
