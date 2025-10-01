using System.ComponentModel.DataAnnotations;

namespace LearnLangs.Models.Flashcards;

public class Flashcard
{
    public int Id { get; set; }
    public int DeckId { get; set; }

    [Required] public string Front { get; set; } = ""; // mặt trước (từ/cụm từ)
    [Required] public string Back { get; set; } = "";  // nghĩa/ghi chú
    public string? Example { get; set; }               // ví dụ
    public string? AudioUrl { get; set; }              // (tuỳ) url audio
    public string? ImageUrl { get; set; }              // (tuỳ) url ảnh

    // spaced-repetition đơn giản
    public DateTime? Due { get; set; }
    public int Repetitions { get; set; }
    public int IntervalDays { get; set; }
    public double Ease { get; set; } = 2.5;

    // nav
    public FlashcardDeck Deck { get; set; } = default!;
}
