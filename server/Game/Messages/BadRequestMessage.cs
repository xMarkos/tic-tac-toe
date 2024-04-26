namespace Markos.TicTacToe.Game.Messages;

/// <summary>
/// Message returned when a client sent an unknown message or a message that is not valid at the time.
/// </summary>
internal class BadRequestMessage
{
    /// <summary>
    /// A message that might help identifying the problem with the rejected request.
    /// </summary>
    public string? Message { get; set; }
}
