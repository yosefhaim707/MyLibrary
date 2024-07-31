﻿namespace MyLibrary.Models
{
    public class BookSet
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Shelf Shelf { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
