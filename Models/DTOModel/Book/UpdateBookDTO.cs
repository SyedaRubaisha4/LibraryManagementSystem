﻿using System.ComponentModel.DataAnnotations;

namespace Models.DTOModel.Book
{
    public class UpdateBookDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public string Author { get; set; }

        public IFormFile? Image { get; set; }
        public IFormFile? PdfFile { get; set; }
        public bool RemoveImage { get; set; } = false;
        public bool RemovePdf { get; set; } = false;


    }
}
