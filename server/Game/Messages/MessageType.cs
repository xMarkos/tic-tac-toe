namespace Markos.TicTacToe.Game.Messages;

/// <summary>
/// Represents a type of message.
/// </summary>
internal enum MessageType : byte
{
    /// <summary>
    /// Client requested to play their turn.
    /// </summary>
    Turn,

    /// <summary>
    /// The server updates clients with the current state of the game.
    /// </summary>
    Update,

    /// <summary>
    /// The server rejected the most recent request.
    /// </summary>
    BadRequest,

    /// <summary>
    /// Client requested to restart the game.
    /// </summary>
    Restart,

    /// <summary>
    /// The server informs the clients that a new client has joined.
    /// </summary>
    NewPlayer,
}
