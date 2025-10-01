using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace LearnLangs.Models.Flashcards
{
    public class FlashcardImportViewModel
    {
        [Required]
        public IFormFile File { get; set; } = default!;
    }
}
