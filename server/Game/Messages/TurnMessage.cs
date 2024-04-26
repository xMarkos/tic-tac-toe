namespace Markos.TicTacToe.Game.Messages;

/// <summary>
/// Represents a message of a client playing their turn.
/// </summary>
internal class TurnMessage
{
    /// <summary>
    /// The X coordinate played.
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// The Y coordinate played.
    /// </summary>
    public int Y { get; set; }
}
