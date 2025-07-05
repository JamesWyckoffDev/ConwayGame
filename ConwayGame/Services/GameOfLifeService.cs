namespace ConwayGame.Services
{
    public class GameOfLifeService : IGameOfLifeService
    {
        public bool[][] GetNextGeneration(bool[][] currentBoard)
        {
            if (!IsValidBoard(currentBoard))
            {
                throw new ArgumentException("Invalid board state provided. Board must be rectangular and not empty.");
            }

            int rows = currentBoard.Length;
            int cols = currentBoard[0].Length;
            bool[][] nextBoard = new bool[rows][];
            for (int i = 0; i < rows; i++)
            {
                nextBoard[i] = new bool[cols];
            }

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    int liveNeighbors = CountLiveNeighbors(currentBoard, r, c, rows, cols);
                    bool isAlive = currentBoard[r][c];

                    if (isAlive)
                    {
                        // Rule 1: Any live cell with fewer than two live neighbours dies (underpopulation).
                        // Rule 3: Any live cell with more than three live neighbours dies (overpopulation).
                        if (liveNeighbors < 2 || liveNeighbors > 3)
                        {
                            nextBoard[r][c] = false; // Dies
                        }
                        // Rule 2: Any live cell with two or three live neighbours lives on to the next generation.
                        else
                        {
                            nextBoard[r][c] = true; // Lives
                        }
                    }
                    else
                    {
                        // Rule 4: Any dead cell with exactly three live neighbours becomes a live cell (reproduction).
                        if (liveNeighbors == 3)
                        {
                            nextBoard[r][c] = true; // Becomes alive
                        }
                        else
                        {
                            nextBoard[r][c] = false; // Stays dead
                        }
                    }
                }
            }

            return nextBoard;
        }

        public bool[][] GetNGenerationsAhead(bool[][] initialBoard, int n)
        {
            if (n < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(n), "Number of generations (N) cannot be negative.");
            }
            if (!IsValidBoard(initialBoard))
            {
                throw new ArgumentException("Invalid initial board state provided.");
            }

            bool[][] currentBoard = initialBoard;
            for (int i = 0; i < n; i++)
            {
                currentBoard = GetNextGeneration(currentBoard);
            }
            return currentBoard;
        }

        public bool[][] GetFinalStableState(bool[][] initialBoard, int maxIterations = 1000)
        {
            if (!IsValidBoard(initialBoard))
            {
                throw new ArgumentException("Invalid initial board state provided.");
            }
            if (maxIterations <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxIterations), "Max iterations must be a positive number.");
            }

            bool[][] currentBoard = initialBoard;
            // Store previous states as JSON strings to detect cycles efficiently
            var history = new Dictionary<string, int>(); // Key: board state JSON, Value: generation number
            int generation = 0;

            while (generation < maxIterations)
            {
                string currentBoardJson = SerializeBoard(currentBoard);

                // Check for cycle
                if (history.ContainsKey(currentBoardJson))
                {
                    // Cycle detected, the board is stable (cycling)
                    return currentBoard;
                }

                history.Add(currentBoardJson, generation);

                bool[][] nextBoard = GetNextGeneration(currentBoard);
                string nextBoardJson = SerializeBoard(nextBoard);

                // Check for stability (board no longer changes)
                if (currentBoardJson == nextBoardJson)
                {
                    return nextBoard; // Stable state reached
                }

                currentBoard = nextBoard;
                generation++;
            }

            throw new InvalidOperationException($"Board did not reach a stable conclusion within {maxIterations} iterations.");
        }
        public bool IsValidBoard(bool[][] grid)
        {
            if (grid == null || grid.Length == 0 || grid[0] == null || grid[0].Length == 0)
            {
                return false; // Empty or null board
            }

            int firstRowLength = grid[0].Length;
            foreach (var row in grid)
            {
                if (row == null || row.Length != firstRowLength)
                {
                    return false; // Jagged array or null row
                }
            }
            return true;
        }

        /// <summary>
        /// Counts the number of live neighbors for a given cell.
        /// </summary>
        /// <param name="board">The current board state.</param>
        /// <param name="row">The row index of the cell.</param>
        /// <param name="col">The column index of the cell.</param>
        /// <param name="rows">Total number of rows in the board.</param>
        /// <param name="cols">Total number of columns in the board.</param>
        /// <returns>The count of live neighbors (0-8).</returns>
        private int CountLiveNeighbors(bool[][] board, int row, int col, int rows, int cols)
        {
            int liveNeighbors = 0;
            // Define all 8 possible neighbor offsets
            int[] dr = { -1, -1, -1, 0, 0, 1, 1, 1 };
            int[] dc = { -1, 0, 1, -1, 1, -1, 0, 1 };

            for (int i = 0; i < 8; i++)
            {
                int nRow = row + dr[i];
                int nCol = col + dc[i];

                // Check bounds and if the neighbor is alive
                if (nRow >= 0 && nRow < rows && nCol >= 0 && nCol < cols && board[nRow][nCol])
                {
                    liveNeighbors++;
                }
            }
            return liveNeighbors;
        }

        /// <summary>
        /// Serializes a 2D boolean array board into a JSON string for comparison.
        /// </summary>
        /// <param name="board">The board to serialize.</param>
        /// <returns>A JSON string representation of the board.</returns>
        private string SerializeBoard(bool[][] board)
        {
            // Using System.Text.Json for serialization
            return System.Text.Json.JsonSerializer.Serialize(board);
        }
    }
}
