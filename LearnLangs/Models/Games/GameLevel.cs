using System.ComponentModel.DataAnnotations;

namespace LearnLangs.Models.Games
{
    public class GameLevel
    {
        public int Id { get; set; }

        [Required, MaxLength(64)]
        public string Name { get; set; } = default!;

        [MaxLength(256)]
        public string? Description { get; set; }

        public GameType Type { get; set; }
        public int Order { get; set; } = 1;
        public bool IsActive { get; set; } = true;

        public ICollection<GameQuestion> Questions { get; set; } = new List<GameQuestion>();
    }
}
