// LearnLangs/Models/PronunciationInputModel.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace LearnLangs.Models;

public class PronunciationInputModel
{
    [Required, Display(Name = "Reference text)")]
    public string ReferenceText { get; set; } = "";

    // Nhận file .wav người dùng upload (PCM mono 16kHz khuyến nghị)
    [Required, Display(Name = "Audio file (.wav mono 16kHz)")]
    public IFormFile AudioFile { get; set; } = default!;
}
