
namespace Markos.TicTacToe.Game;

/// <summary>
/// Manages instances of <see cref="TicTacToeGame"/>, cleaning-up stale objects.
/// </summary>
internal sealed class TicTacToeGameManager : IDisposable
{
    private readonly CancellationTokenSource _cts;

    private int counter = 0;
    private readonly Dictionary<string, TicTacToeGame> _games = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="TicTacToeGameManager"/> class.
    /// </summary>
    public TicTacToeGameManager()
    {
        _cts = new();
        Task.Factory.StartNew(CleanUpLoop, TaskCreationOptions.LongRunning);
    }

    public void Dispose()
    {
        _cts.Cancel();

        foreach ((_, TicTacToeGame game) in _games)
        {
            game.Dispose();
        }

        _games.Clear();

        _cts.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task CleanUpLoop()
    {
        HashSet<string> candidates = [];

        while (!_cts.IsCancellationRequested)
        {
            HashSet<string> current = new(_games.Where(x => x.Value.ConnectedClientCount == 0).Select(x => x.Key));

            candidates.IntersectWith(current);
            foreach (string id in candidates)
            {
                current.Remove(id);

                if (_games.Remove(id, out TicTacToeGame? game))
                    game.Dispose();
            }

            candidates = current;

            try
            {
                await Task.Delay(60000, _cts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Creates a new instance of <see cref="TicTacToeGame"/>.
    /// </summary>
    public Task<(string, TicTacToeGame)> CreateGame()
    {
        TicTacToeGame result = new();

        string id = $"{++counter}";
        _games.Add(id, result);

        return Task.FromResult((id, result));
    }

    /// <summary>
    /// Gets game by id, or null if it does not exist.
    /// </summary>
    public Task<TicTacToeGame?> GetGame(string id)
    {
        _games.TryGetValue(id, out TicTacToeGame? result);
        return Task.FromResult(result);
    }

    /// <summary>
    /// Deletes a game specified by ID.
    /// </summary>
    public void DeleteGame(string id)
    {
        if (_games.Remove(id, out TicTacToeGame? game))
            game.Dispose();
    }
}
