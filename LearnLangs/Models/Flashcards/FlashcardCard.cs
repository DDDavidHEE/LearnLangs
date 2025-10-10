namespace LearnLangs.Models.Flashcards
{
    public class FlashcardCard
    {
        public int Id { get; set; }
        public int DeckId { get; set; }
        public int OrderIndex { get; set; } = 1;

        public string FrontWord { get; set; } = "";
        public string? Pos { get; set; }
        public string? Ipa { get; set; }
        public string? Phonetic { get; set; }
        public string? BackMeaningVi { get; set; }
        public string? ExampleEn { get; set; }
        public string? ExampleVi { get; set; }
        public string? ImageUrl { get; set; }

        public FlashcardDeck? Deck { get; set; }
    }
}
