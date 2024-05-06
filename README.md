# Tic Tac Toe

Yes, this is yet another tic tac toe. There is probably thousands of tic tac toes already, so why does this project exist? There is a couple of reasons.

## The reasons

- I am searching for a job and I needed a simple sample of presentable code.
- I wanted to see if I can make it fully functional in just 1 weekend.
- I wanted to refresh my knowledge.
- I wanted to see if there are major changes in recent versions of frameworks/libraries.

## Constraints

Originally, I have challenged myself to code tic tac toe in react in 1 hour. I succeeded, the game was fully functional and I finished it in 59 minutes. But it was very basic and frankly quite ugly (what can one do in 60 minutes, eh?).

Next, I wanted to step up the challenge and turn it into a network game, where you could play with your friends on the web. And the requirements were:

- Backend is written in .NET.
- Real time communication is done using WebSockets.
- Frontend is a single page app written either in React or Angular.
- Host it somewhere publicly accessible.
- And the most important of all: Do it all in just 1 weekend.

## The architecture

### Backend

The backend is written in .NET 8. It is a simple ASP.NET Core application.

The core of the app is just 1 REST endpoint for creating a game lobby, and 1 WebSocket endpoint which represents a connection to a lobby.

The backend maintains the state of a game, evaluates turns, announces winners, and keeps scores of the players. After each change of the state, all clients receive the updated states - this allows the clients to be quite stateless.

Each lobby is kept alive as long as there are connected clients. Once all clients fail to reconnect within 1 minute timeframe, the lobby is removed and clients can no longer connect.

And finally, it is hosted on Azure [here](https://markos-tictactoe.azurewebsites.net/).

### Frontend

Frontend is intended to be minimalistic as all game logic is evaluated on the server. The client only needs to:
1. Create a lobby.
2. Connect to a WebSocket.
3. Render updates sent by the server.

There are 2 frontend implementations, both offering identical user interface.

#### Angular

Angular implementation of the frontend uses angular 17 and can be found [here](https://markos-tictactoe.azurewebsites.net/a).

#### React

React implementation of the frontend uses react 18 and can be found [here](https://markos-tictactoe.azurewebsites.net/r).
