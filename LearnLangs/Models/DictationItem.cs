namespace LearnLangs.Models.Dictation
{
    public class DictationItem
    {
        public int Id { get; set; }

        
        public int SetId { get; set; }
        public DictationSet? Set { get; set; }

        public int OrderIndex { get; set; } = 0;

        public string Transcript { get; set; } = "";
        public string AudioUrl { get; set; } = ""; 

        public string? Hint { get; set; }
    }
}
