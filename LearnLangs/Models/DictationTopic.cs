namespace LearnLangs.Models.Dictation
{
    public class DictationTopic
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string? CoverUrl { get; set; }          // ảnh minh họa nhỏ
        public string? Description { get; set; }

        public ICollection<DictationSet> Sets { get; set; } = new List<DictationSet>();
    }
}
