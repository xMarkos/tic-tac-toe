namespace Markos.TicTacToe.Game.Messages;

internal struct Envelope<T>(MessageType type, T? data)
{
    public MessageType Type { get; set; } = type;
    public T? Data { get; set; } = data;
}
