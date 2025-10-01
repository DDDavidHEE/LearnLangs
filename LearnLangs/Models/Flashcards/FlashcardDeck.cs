namespace LearnLangs.Models.Flashcards;

public class FlashcardDeck
{
    public int Id { get; set; }
    public int? CourseId { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }

    // NEW
    public string? OwnerUserId { get; set; }   // deck của ai (null = do admin tạo, có thể vẫn đánh dấu System)
    public bool IsSystem { get; set; }         // deck mặc định của hệ thống (chỉ admin xoá)

    // nav
    public Course? Course { get; set; }
    public ICollection<Flashcard> Cards { get; set; } = new List<Flashcard>();
}
