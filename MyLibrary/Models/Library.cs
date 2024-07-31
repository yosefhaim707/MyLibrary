using System.ComponentModel.DataAnnotations.Schema;

namespace MyLibrary.Models
{
    [Table("Libraries")]
    public class Library
    {
        public int Id { get; set; }
        public string Genre { get; set; }
        public List<Shelf>? Shelves { get; set; }

        
    }
}
