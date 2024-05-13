namespace Markos.TicTacToe.Game;

public interface IGameManager<T>
{
    Task<(string, T)> CreateGame();
    void DeleteGame(string id);
    Task<T?> GetGame(string id);
}
