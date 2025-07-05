namespace ConwayGame.Services
{
    public interface IGameOfLifeService
    {
        /// <summary>
        /// Calculates the next generation of a given board state.
        /// </summary>
        /// <param name="currentBoard">The current 2D boolean grid representing the board.</param>
        /// <returns>The 2D boolean grid representing the next generation.</returns>
        bool[][] GetNextGeneration(bool[][] currentBoard);

        /// <summary>
        /// Calculates the board state after N generations.
        /// </summary>
        /// <param name="initialBoard">The initial 2D boolean grid.</param>
        /// <param name="n">The number of generations to advance.</param>
        /// <returns>The 2D boolean grid after N generations.</returns>
        bool[][] GetNGenerationsAhead(bool[][] initialBoard, int n);

        /// <summary>
        /// Calculates the final stable state of a board or returns an error if it doesn't stabilize within limits.
        /// </summary>
        /// <param name="initialBoard">The initial 2D boolean grid.</param>
        /// <param name="maxIterations">Maximum iterations to attempt before considering it unstable.</param>
        /// <returns>The 2D boolean grid representing the final stable state.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the board does not reach a stable conclusion within maxIterations.</exception>
        bool[][] GetFinalStableState(bool[][] initialBoard, int maxIterations = 1000);

        /// <summary>
        /// Validates if a given board grid is well-formed (rectangular, not empty).
        /// </summary>
        /// <param name="grid">The 2D boolean grid to validate.</param>
        /// <returns>True if the grid is valid, false otherwise.</returns>
        bool IsValidBoard(bool[][] grid);
    }
}
