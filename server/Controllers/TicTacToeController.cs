using Markos.TicTacToe.Game;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;

namespace Markos.TicTacToe.Controllers;

[Route("api/[controller]")]
[ApiController]
internal class TicTacToeController(TicTacToeGameManager manager) : ControllerBase
{
    private readonly TicTacToeGameManager _manager = manager;

    /// <summary>
    /// Creates a new lobby of Tic Tac Toe.
    /// </summary>
    [HttpPost]
    public async Task<CreateGameResult> CreateGame()
    {
        (string id, _) = await _manager.CreateGame().ConfigureAwait(false);

        return new()
        {
            GameId = id,
        };
    }

    /// <summary>
    /// Joins the game lobby specified by id.
    /// </summary>
    /// <param name="id">The identifier of the lobby to join.</param>
    /// <param name="clientId">The identifier of the joining client (only serves as a non-unique weak identity).</param>
    /// <remarks>
    /// This is a WebSocket API. The messages sent must adhere to the protocol of <see cref="TicTacToeGame"/>.
    /// </remarks>
    [Route("{id}")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task JoinGame(string id, [FromQuery(Name = "cid"), Required] string clientId, CancellationToken cancellationToken)
    {
        Task Bail(IActionResult result) => result.ExecuteResultAsync(ControllerContext);

        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            await Bail(BadRequest()).ConfigureAwait(false);
            return;
        }

        if (await _manager.GetGame(id).ConfigureAwait(false) is not TicTacToeGame game)
        {
            await Bail(NotFound()).ConfigureAwait(false);
            return;
        }

        using WebSocket socket = await HttpContext.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);

        async Task MessageDispatcher(ReadOnlyMemory<byte> payload)
        {
            try
            {
                await socket.SendAsync(payload, WebSocketMessageType.Text, true, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // This can happen in a race condition shortly after client closes the WebSocket and before this dispatcher is unregistered.
                // The dispatcher will eventually get unregistered and the problem should not happen again.
            }
        }

        Task MessageHandler(ReadOnlyMemory<byte> payload)
            => game.HandleMessage(clientId, payload);

        try
        {
            await game.RegisterDispatcher(clientId, MessageDispatcher).ConfigureAwait(false);

            await WebSocketReceiverLoop(MessageHandler, socket, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            game.UnregisterDispatcher(clientId, MessageDispatcher);
        }
    }

    /// <summary>
    /// Deletes a game lobby specified by ID.
    /// </summary>
    /// <param name="id">The identifier of the lobby to delete.</param>
    /// <remarks>
    /// Lobbies without clients are cleaned-up automatically after 120 seconds.
    /// </remarks>
    [HttpDelete]
    public ActionResult DeleteGame(string id)
    {
        _manager.DeleteGame(id);
        return Ok();
    }

    private static async Task WebSocketReceiverLoop(Func<ReadOnlyMemory<byte>, Task> handler, WebSocket socket, CancellationToken cancellationToken)
    {
        ArraySegment<byte> buffer = new byte[256];

        async Task<ArraySegment<byte>?> GetNextMessage()
        {
            bool discard = false;
            int count = 0;

            while (!cancellationToken.IsCancellationRequested)
            {
                WebSocketReceiveResult msg;
                try
                {
                    msg = await socket.ReceiveAsync(discard ? buffer : buffer.Slice(count), cancellationToken).ConfigureAwait(false);
                    if (msg.MessageType == WebSocketMessageType.Close)
                        break;
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                count += msg.Count;

                if (msg.EndOfMessage)
                {
                    if (discard)
                    {
                        discard = false;
                        count = 0;
                    }
                    else
                    {
                        return buffer.Slice(0, count);
                    }
                }
                else if (count == buffer.Count)
                {
                    // Buffer is full -> read the message to the end and discard it
                    discard = true;
                }
            }

            return null;
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            ArraySegment<byte>? msg = await GetNextMessage().ConfigureAwait(false);
            if (msg == null)
                break;

            await handler(msg.Value).ConfigureAwait(false);
        }

        try
        {
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None).ConfigureAwait(false);
        }
        catch (WebSocketException)
        {
        }
    }

    public class CreateGameResult
    {
        /// <summary>
        /// Gets the identifier of the created game lobby.
        /// </summary>
        [NotNull]
        public string? GameId { get; set; }
    }
}
