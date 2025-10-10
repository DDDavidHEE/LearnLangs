namespace LearnLangs.Models.Flashcards
{
    public class FlashcardDeck
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Mode { get; set; } = "basic"; // basic|ielts|hsk
        public string? CoverUrl { get; set; }
        public int OrderIndex { get; set; } = 1;

        public ICollection<FlashcardCard> Cards { get; set; } = new List<FlashcardCard>();
    }
}
