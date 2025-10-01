namespace LearnLangs.Models.Dictation
{
    public class DictationSet
    {
        public int Id { get; set; }

        // FK
        public int TopicId { get; set; }
        public DictationTopic? Topic { get; set; }

        public string Title { get; set; } = "";
        public string? Level { get; set; }

      
        public int OrderIndex { get; set; } = 0;

        public ICollection<DictationItem> Items { get; set; } = new List<DictationItem>();
    }
}
