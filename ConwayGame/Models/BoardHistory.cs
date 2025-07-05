using System.ComponentModel.DataAnnotations;

namespace ConwayGame.Models
{
    public class BoardHistory
    {
        [Key]
        public int Id { get; set; }
        public Guid BoardId { get; set; }
        public Board Board { get; set; } = null!;
        public string StateJson { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public int Generation { get; set; }
    }
}
