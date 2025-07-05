using ConwayGame.Models;
using Microsoft.EntityFrameworkCore;

namespace ConwayGame.Data
{
    /// <summary>
    /// DbContext for the Game of Life application, managing Board entities.
    /// </summary>
    public class GameOfLifeDbContext : DbContext
    {
        public GameOfLifeDbContext(DbContextOptions<GameOfLifeDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// DbSet for Board entities.
        /// </summary>
        public DbSet<Board> Boards { get; set; } = null!;

        /// <summary>
        /// DbSet for BoardHistory entities (optional).
        /// </summary>
        public DbSet<BoardHistory> BoardHistory { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the Board entity
            modelBuilder.Entity<Board>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.Property(b => b.StateJson)
                      .IsRequired();
                entity.Property(b => b.CreatedAt)
                      .HasDefaultValueSql("CURRENT_TIMESTAMP"); // SQLite specific default value
            });

            // Configure the BoardHistory entity (optional)
            modelBuilder.Entity<BoardHistory>(entity =>
            {
                entity.HasKey(bh => bh.Id);
                entity.HasOne(bh => bh.Board)
                      .WithMany(b => b.History)
                      .HasForeignKey(bh => bh.BoardId);
                entity.Property(bh => bh.StateJson)
                      .IsRequired();
                entity.Property(bh => bh.Timestamp)
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
        }
    }
}
