using LearnLangs.Models;

public enum CardStatus { New, Learning, Mastered }

public class FlashcardProgress
{
    public int Id { get; set; }
    public string UserId { get; set; } 
    public int FlashcardCardId { get; set; } 

    public CardStatus Status { get; set; } = CardStatus.New;
    public DateTime NextReviewDate { get; set; } = DateTime.UtcNow; 
    public int IntervalDays { get; set; } = 1; 

    public ApplicationUser User { get; set; }

}