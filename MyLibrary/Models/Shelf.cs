using System.ComponentModel.DataAnnotations.Schema;

namespace MyLibrary.Models
{
    public class Shelf
    {
        public int Id { get; set; }
        public List<Book>? Books { get; set; }
        public List<BookSet>? BookSets { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Library Library { get; set; }
        [NotMapped]
        public int FreeSpace {  get; set; }
        
    }

    
}
