using ConwayGame.Data;
using ConwayGame.Models;

namespace ConwayGame.Repositories
{
    public class BoardRepository : IBoardRepository
    {
        private readonly GameOfLifeDbContext _context;

        public BoardRepository(GameOfLifeDbContext context)
        {
            _context = context;
        }

        public async Task<Board?> GetBoardAsync(Guid id)
        {
            return await _context.Boards.FindAsync(id);
        }

        public async Task<Board> AddBoardAsync(Board board)
        {
            board.Id = Guid.NewGuid(); // Generate a new ID for the board
            board.CreatedAt = DateTime.UtcNow; // Set creation timestamp
            _context.Boards.Add(board);
            await _context.SaveChangesAsync();
            return board;
        }

        public async Task<Board> UpdateBoardAsync(Board board)
        {
            _context.Boards.Update(board);
            await _context.SaveChangesAsync();
            return board;
        }

        public async Task<bool> DeleteBoardAsync(Guid id)
        {
            var board = await _context.Boards.FindAsync(id);
            if (board == null)
            {
                return false;
            }

            _context.Boards.Remove(board);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
