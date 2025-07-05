using ConwayGame.Models;
using ConwayGame.Repositories;
using ConwayGame.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ConwayGame.Controllers
{
    [ApiController]
    [Route("api/boards")]
    public class GameOfLifeController : ControllerBase
    {
        private readonly IBoardRepository _boardRepository;
        private readonly IGameOfLifeService _gameOfLifeService;
        private readonly ILogger<GameOfLifeController> _logger;

        public GameOfLifeController(IBoardRepository boardRepository, IGameOfLifeService gameOfLifeService, ILogger<GameOfLifeController> logger)
        {
            _boardRepository = boardRepository;
            _gameOfLifeService = gameOfLifeService;
            _logger = logger;
        }

        /// <summary>
        /// Uploads a new Game of Life board state and stores it.
        /// </summary>
        /// <param name="boardStateDto">The 2D boolean grid representing the initial board state.</param>
        /// <returns>A unique identifier for the stored board.</returns>
        /// <response code="201">Returns the unique ID of the newly created board.</response>
        /// <response code="400">If the board state is invalid or malformed.</response>
        [HttpPost]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadBoardState([FromBody] BoardStateDto boardStateDto)
        {
            if (boardStateDto == null || !_gameOfLifeService.IsValidBoard(boardStateDto.Grid))
            {
                _logger.LogWarning("Invalid board state received for upload.");
                return BadRequest("Invalid board state. Ensure it's a non-empty, rectangular 2D boolean array.");
            }

            try
            {
                var board = new Board
                {
                    StateJson = JsonSerializer.Serialize(boardStateDto.Grid)
                };

                var createdBoard = await _boardRepository.AddBoardAsync(board);
                _logger.LogInformation("Board uploaded successfully with ID: {BoardId}", createdBoard.Id);
                return CreatedAtAction(nameof(GetBoard), new { boardId = createdBoard.Id }, createdBoard.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading board state.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while uploading the board state.");
            }
        }

        /// <summary>
        /// Retrieves a specific board state by its ID. (Helper endpoint for testing/debugging)
        /// </summary>
        /// <param name="boardId">The unique identifier of the board.</param>
        /// <returns>The current state of the board.</returns>
        /// <response code="200">Returns the board state.</response>
        /// <response code="404">If the board with the given ID is not found.</response>
        [HttpGet("{boardId}")]
        [ProducesResponseType(typeof(BoardStateDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBoard(Guid boardId)
        {
            var board = await _boardRepository.GetBoardAsync(boardId);
            if (board == null)
            {
                _logger.LogWarning("Board with ID {BoardId} not found.", boardId);
                return NotFound($"Board with ID '{boardId}' not found.");
            }

            var boardState = JsonSerializer.Deserialize<bool[][]>(board.StateJson);
            if (boardState == null)
            {
                _logger.LogError("Failed to deserialize board state for ID: {BoardId}", boardId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to retrieve board state due to deserialization error.");
            }

            return Ok(new BoardStateDto { Grid = boardState });
        }


        /// <summary>
        /// Gets the next generation state of a board given its ID.
        /// </summary>
        /// <param name="boardId">The unique identifier of the board.</param>
        /// <returns>The next generation state of the board.</returns>
        /// <response code="200">Returns the next generation board state.</response>
        /// <response code="404">If the board with the given ID is not found.</response>
        /// <response code="500">If an internal server error occurs during calculation.</response>
        [HttpGet("{boardId}/next")]
        [ProducesResponseType(typeof(BoardStateDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetNextState(Guid boardId)
        {
            var board = await _boardRepository.GetBoardAsync(boardId);
            if (board == null)
            {
                _logger.LogWarning("Board with ID {BoardId} not found for next state calculation.", boardId);
                return NotFound($"Board with ID '{boardId}' not found.");
            }

            try
            {
                var currentGrid = JsonSerializer.Deserialize<bool[][]>(board.StateJson);
                if (currentGrid == null)
                {
                    _logger.LogError("Failed to deserialize current board state for ID: {BoardId}", boardId);
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to process board state due to deserialization error.");
                }

                var nextGrid = _gameOfLifeService.GetNextGeneration(currentGrid);

                // Optionally, update the stored board with the next state if desired,
                // but the requirement is just to return the next state.
                // board.StateJson = JsonSerializer.Serialize(nextGrid);
                // await _boardRepository.UpdateBoardAsync(board);

                return Ok(new BoardStateDto { Grid = nextGrid });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Validation error when calculating next state for board {BoardId}.", boardId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating next state for board {BoardId}.", boardId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while calculating the next board state.");
            }
        }

        /// <summary>
        /// Gets the board state after N generations for a given board ID.
        /// </summary>
        /// <param name="boardId">The unique identifier of the board.</param>
        /// <param name="n">The number of generations to advance.</param>
        /// <returns>The board state after N generations.</returns>
        /// <response code="200">Returns the board state after N generations.</response>
        /// <response code="400">If N is negative or board state is invalid.</response>
        /// <response code="404">If the board with the given ID is not found.</response>
        /// <response code="500">If an internal server error occurs during calculation.</response>
        [HttpGet("{boardId}/generations/{n}")]
        [ProducesResponseType(typeof(BoardStateDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetNStatesAhead(Guid boardId, int n)
        {
            if (n < 0)
            {
                return BadRequest("Number of generations (N) cannot be negative.");
            }

            var board = await _boardRepository.GetBoardAsync(boardId);
            if (board == null)
            {
                _logger.LogWarning("Board with ID {BoardId} not found for N generations ahead calculation.", boardId);
                return NotFound($"Board with ID '{boardId}' not found.");
            }

            try
            {
                var currentGrid = JsonSerializer.Deserialize<bool[][]>(board.StateJson);
                if (currentGrid == null)
                {
                    _logger.LogError("Failed to deserialize current board state for ID: {BoardId}", boardId);
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to process board state due to deserialization error.");
                }

                var finalGrid = _gameOfLifeService.GetNGenerationsAhead(currentGrid, n);
                return Ok(new BoardStateDto { Grid = finalGrid });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Validation error when calculating N generations ahead for board {BoardId}.", boardId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating N generations ahead for board {BoardId}.", boardId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while calculating the board state after N generations.");
            }
        }

        /// <summary>
        /// Gets the final stable state of a board.
        /// </summary>
        /// <param name="boardId">The unique identifier of the board.</param>
        /// <param name="maxIterations">Optional: Maximum iterations to attempt before considering it unstable. Defaults to 1000.</param>
        /// <returns>The final stable state of the board.</returns>
        /// <response code="200">Returns the final stable board state.</response>
        /// <response code="400">If maxIterations is invalid or board state is invalid.</response>
        /// <response code="404">If the board with the given ID is not found.</response>
        /// <response code="409">If the board does not reach a stable conclusion within the specified iterations.</response>
        /// <response code="500">If an internal server error occurs during calculation.</response>
        [HttpGet("{boardId}/final")]
        [ProducesResponseType(typeof(BoardStateDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFinalState(Guid boardId, [FromQuery] int maxIterations = 1000)
        {
            if (maxIterations <= 0)
            {
                return BadRequest("Max iterations must be a positive number.");
            }

            var board = await _boardRepository.GetBoardAsync(boardId);
            if (board == null)
            {
                _logger.LogWarning("Board with ID {BoardId} not found for final state calculation.", boardId);
                return NotFound($"Board with ID '{boardId}' not found.");
            }

            try
            {
                var currentGrid = JsonSerializer.Deserialize<bool[][]>(board.StateJson);
                if (currentGrid == null)
                {
                    _logger.LogError("Failed to deserialize current board state for ID: {BoardId}", boardId);
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to process board state due to deserialization error.");
                }

                var finalGrid = _gameOfLifeService.GetFinalStableState(currentGrid, maxIterations);
                return Ok(new BoardStateDto { Grid = finalGrid });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogInformation(ex, "Board {BoardId} did not reach a stable conclusion within {MaxIterations} iterations.", boardId, maxIterations);
                return Conflict(ex.Message); // 409 Conflict for non-stable board
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Validation error when calculating final state for board {BoardId}.", boardId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating final state for board {BoardId}.", boardId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while calculating the final board state.");
            }
        }
    }
}
