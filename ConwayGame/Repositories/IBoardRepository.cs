using ConwayGame.Models;

namespace ConwayGame.Repositories
{
    public interface IBoardRepository
    {
        Task<Board?> GetBoardAsync(Guid id);
        Task<Board> AddBoardAsync(Board board);
        Task<Board> UpdateBoardAsync(Board board);
        Task<bool> DeleteBoardAsync(Guid id);
    }
}
