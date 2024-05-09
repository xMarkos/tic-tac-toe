using Markos.TicTacToe.Game.Messages;
using Markos.TicTacToe.MVC.Serialization;
using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Markos.TicTacToe.Game;

/// <summary>
/// Represents an instance of Tic-Tac-Toe game.
/// </summary>
internal class TicTacToeGame : IDisposable
{
    private readonly static JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = {
            new TwoDArrayConverterFactory(),
            new TupleConverterFactory(),
        },
    };

    private readonly List<string> _clients = [];
    private ImmutableList<(string ClientId, Func<ReadOnlyMemory<byte>, Task> Handler)> _dispatchers = [];

    private readonly int[,] _board = new int[3, 3];
    private int _turnCount;
    private int _currentPlayer;
    private bool _isComplete;
    private int _winner;
    private (int, int)[]? _winningFields;
    private int _firstPlayer;
    private readonly int[] _points = [0, 0, 0];

    /// <summary>
    /// Gets the count of currently active connections.
    /// </summary>
    public int ConnectedClientCount => _dispatchers.Count;

    /// <summary>
    /// Initializes a new instance of the <see cref="TicTacToeGame"/> class.
    /// </summary>
    public TicTacToeGame()
    {
        ResetGame();
    }

    public void Dispose()
    {
        _dispatchers = [];
        _clients.Clear();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Resets the game state, preparing it for the next round.
    /// </summary>
    public void ResetGame()
    {
        _firstPlayer = _firstPlayer == 1 ? 2 : 1;
        _currentPlayer = _firstPlayer;
        _turnCount = 0;
        _isComplete = false;
        _winner = 0;
        _winningFields = null;

        for (int x = 0, l0 = _board.GetLength(0); x < l0; x++)
        {
            for (int y = 0, l1 = _board.GetLength(1); y < l1; y++)
            {
                _board[x, y] = 0;
            }
        }
    }

    private async Task AnnounceNewPlayer()
    {
        int playerCount = _clients.Count;

        foreach ((string clientId, _) in _dispatchers)
        {
            int player = _clients.IndexOf(clientId) + 1;

            await Send(MessageType.NewPlayer, new NewPlayerMessage
            {
                PlayerCount = playerCount,
                CurrentPlayerNumber = player,
            }, [clientId])
            .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Handles a message sent by client specified by ID.
    /// </summary>
    /// <param name="clientId">The client that sent the message.</param>
    /// <param name="payload">The payload of the message.</param>
    /// <remarks>
    /// The payload must be JSON encoded <see cref="Envelope{T}"/>.
    /// The following messages are supported:
    /// <list type="bullet">
    /// <item><see cref="MessageType.Turn"/></item>
    /// <item><see cref="MessageType.Restart"/></item>
    /// </list>
    /// </remarks>
    public async Task HandleMessage(string clientId, ReadOnlyMemory<byte> payload)
    {
        try
        {
            Envelope<JsonElement> msg =
                JsonSerializer.Deserialize<Envelope<JsonElement>>(payload.Span, _serializerOptions);

            switch (msg.Type)
            {
                case MessageType.Turn:
                    await Turn(JsonSerializer.Deserialize<TurnMessage>(msg.Data, _serializerOptions)!, clientId).ConfigureAwait(false);
                    break;
                case MessageType.Restart:
                    ResetGame();
                    await AnnounceBoardUpdate().ConfigureAwait(false);
                    break;
                default:
                    await SendBadRequest(clientId, $"Command type {(int)msg.Type} is unknown").ConfigureAwait(false);
                    break;
            }
        }
        catch (Exception ex)
        {
            // This prevents receiver loop termination in case of unexpected data
            await SendBadRequest(clientId, "Exception: " + ex.Message).ConfigureAwait(false);
        }
    }

    private Task Turn(TurnMessage message, string clientId)
    {
        int player = _clients.IndexOf(clientId) + 1;

        if (_currentPlayer != player || _isComplete)
            return SendBadRequest(clientId, "Turn not allowed at this moment");

        int l0 = _board.GetLength(0);
        int l1 = _board.GetLength(1);

        int x = message.X;
        int y = message.Y;

        if (x < 0 || x >= l0 || y < 0 || y >= l1)
            return SendBadRequest(clientId, $"Coordinates [{x}, {y}] out of bounds");

        if (_board[x, y] != 0)
            return SendBadRequest(clientId, $"Coordinates [{x}, {y}] already occupied");

        _board[x, y] = player;
        _turnCount++;
        _currentPlayer = player == 1 ? 2 : 1;

        {   // Evaluate winner

            List<(int X, int Y)[]> candidates = [
                [(x, 0), (x, 1), (x, 2)],        // column
                [(0, y), (1, y), (2, y)],        // row
                [(0, 0), (1, 1), (2, 2)],        // diagonal 1
                [(2, 0), (1, 1), (0, 2)],        // diagonal 2
            ];

            foreach (var candidate in candidates)
            {
                if (candidate.All(x => _board[x.X, x.Y] == player))
                {
                    _isComplete = true;
                    _winner = player;
                    _winningFields = candidate;
                    break;
                }
            }

            if (_turnCount == 9)
                _isComplete = true;
        }

        if (_isComplete)
        {
            _currentPlayer = 0;
            _points[_winner]++;
        }

        return AnnounceBoardUpdate();
    }

    private Task SendBadRequest(string clientId, string? message = null)
    {
        BadRequestMessage? msg = message != null ? new BadRequestMessage { Message = message } : null;
        return Send(MessageType.BadRequest, msg, [clientId]);
    }

    private Task AnnounceBoardUpdate(IEnumerable<string>? clients = null)
    {
        return Send(MessageType.Update, new UpdateMessage()
        {
            Board = _board,
            IsComplete = _isComplete,
            CurrentPlayer = _currentPlayer,
            TurnCount = _turnCount,
            Winner = _isComplete ? _winner : null,
            WinningFields = _winningFields,
            Points = _points,
        }, clients ?? _clients);
    }

    /// <summary>
    /// Registers a dispatcher that delivers messages to a client.
    /// </summary>
    /// <param name="clientId">The client identifier associated with this dispatcher.</param>
    /// <param name="handler">The handler function processing the message.</param>
    public async Task RegisterDispatcher(string clientId, Func<ReadOnlyMemory<byte>, Task> handler)
    {
        int player = _clients.IndexOf(clientId);
        if (player == -1)
        {
            lock (_clients)
            {
                player = _clients.IndexOf(clientId);
                if (player == -1)
                {
                    _clients.Add(clientId);
                    player = _clients.Count;
                }
            }
        }

        _dispatchers = _dispatchers.Add((clientId, handler));

        await AnnounceNewPlayer().ConfigureAwait(false);
        await AnnounceBoardUpdate([clientId]).ConfigureAwait(false);
    }

    /// <summary>
    /// Unregisters a dispatcher previously registered by <see cref="RegisterDispatcher(string, Func{ReadOnlyMemory{byte}, Task})"/>.
    /// </summary>
    public void UnregisterDispatcher(string clientId, Func<ReadOnlyMemory<byte>, Task> handler)
        => _dispatchers = _dispatchers.Remove((clientId, handler));

    private Task Send<T>(MessageType type, T? msg, IEnumerable<string> destinations)
        => Send(Serialize(type, msg), destinations);

    private async Task Send(ReadOnlyMemory<byte> payload, IEnumerable<string> destinations)
    {
        foreach (string clientId in destinations)
        {
            foreach ((_, Func<ReadOnlyMemory<byte>, Task> handler) in _dispatchers.FindAll(x => x.ClientId == clientId))
            {
                try
                {
                    await handler(payload).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Could not send message to client {0}\n{1}", clientId, ex);
                }
            }
        }
    }

    private static byte[] Serialize<T>(MessageType type, T? msg)
    {
        Envelope<T> envelope = new(type, msg);

        using MemoryStream ms = new();
        JsonSerializer.Serialize(ms, envelope, _serializerOptions);

        return ms.ToArray();
    }
}
