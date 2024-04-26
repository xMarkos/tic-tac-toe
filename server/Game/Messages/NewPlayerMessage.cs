namespace Markos.TicTacToe.Game.Messages;

/// <summary>
/// Represents a message that announces a new player in the lobby.
/// </summary>
internal class NewPlayerMessage
{
    /// <summary>
    /// The count of players currently connected to the game.
    /// </summary>
    public int PlayerCount { get; set; }

    /// <summary>
    /// The player number associated with the receiver of this message.
    /// </summary>
    public int CurrentPlayerNumber { get; set; }
}
