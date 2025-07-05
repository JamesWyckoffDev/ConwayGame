using System.ComponentModel.DataAnnotations;

namespace ConwayGame.Models
{
    public class Board
    {
        /// <summary>
        /// Unique identifier for the board.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// The JSON string representation of the 2D boolean array (the board state).
        /// </summary>
        [Required]
        public string StateJson { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp when the board was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Navigation property for BoardHistory (if implemented for auditing/tracking generations).
        /// Not strictly required for the current functional requirements but good for extensibility.
        /// </summary>
        public ICollection<BoardHistory>? History { get; set; }
    }
}
