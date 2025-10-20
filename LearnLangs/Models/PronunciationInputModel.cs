using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace LearnLangs.Models
{
    public class PronunciationInputModel
    {
        public IFormFile? AudioFile { get; set; }

        [Required]
        public string ReferenceText { get; set; } = "Hello world";

        // Phụ đề trực tiếp (nhận từ Web Speech API ở client)
        public string? SpokenText { get; set; }
    }
}
