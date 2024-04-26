namespace Markos.TicTacToe.Game.Messages;

/// <summary>
/// Represents a message that carries the most up-to-date state of the game.
/// </summary>
internal class UpdateMessage
{
    /// <summary>
    /// The state of the game board.
    /// </summary>
    public int[,]? Board { get; set; }

    /// <summary>
    /// The count of turns played.
    /// </summary>
    public int TurnCount { get; set; }

    /// <summary>
    /// The player number of the client that should play next.
    /// </summary>
    public int CurrentPlayer { get; set; }

    /// <summary>
    /// Indicates that the game is finished.
    /// </summary>
    /// <remarks>
    /// Fields <see cref="Winner"/> and <see cref="WinningFields"/> will also be set. One of the clients must send <see cref="MessageType.Restart"/> message.
    /// </remarks>
    public bool IsComplete { get; set; }

    /// <summary>
    /// The winner of the current game round.
    /// </summary>
    public int? Winner { get; set; }

    /// <summary>
    /// The coordinates on the board that caused this game to end.
    /// </summary>
    public (int, int)[]? WinningFields { get; set; }

    /// <summary>
    /// A fixed length array containing draws, player 1, and player 2 points respectively.
    /// </summary>
    public int[]? Points { get; set; }
}
