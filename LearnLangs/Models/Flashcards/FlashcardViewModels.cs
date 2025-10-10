namespace LearnLangs.Models.Flashcards
{
    public record FlashDeckVM
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string? CoverUrl { get; set; }
        public string Mode { get; set; } = "basic";
        public int Count { get; set; }
    }

    public record FlashCardVM
    {
        public int Id { get; set; }
        public string Word { get; set; } = "";
        public string? Pos { get; set; }
        public string? Ipa { get; set; }
        public string? Phonetic { get; set; }
        public string? MeaningVi { get; set; }
        public string? ExampleEn { get; set; }
        public string? ExampleVi { get; set; }
        public string? ImageUrl { get; set; }
    }

    public record PlayDeckVM
    {
        public int DeckId { get; set; }
        public string DeckTitle { get; set; } = "";
        public string Mode { get; set; } = "basic";
        public string? CoverUrl { get; set; }
        public int Part { get; set; }
        public int TotalParts { get; set; }
        public int TotalCards { get; set; }
        public List<FlashCardVM> Cards { get; set; } = new();
    }
}
