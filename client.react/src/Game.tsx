import { BadRequestMessage, Envelope, MessageType, NewPlayerMessage, TurnMessage, UpdateMessage } from '@lib/TicTacToe/api.models';
import React, { forwardRef, useCallback, useContext, useImperativeHandle, useLayoutEffect, useReducer, useRef } from 'react';
import { useParams } from 'react-router-dom';
import { AppContext } from './App';
import Board from './Board';
import Field from './Field';
import './Game.css';

function copyUrl() {
	navigator.clipboard.writeText(window.location.href);
}

interface GameState {
	playerCount: number;
	myPlayerNumber: number;
	currentPlayer: number;
	board: number[][];
	turnCount: number;
	isComplete: boolean;
	winner?: number;
	winningFields?: [number, number][];
	points: number[];
}

function initState(): GameState {
	return {
		board: [
			[0, 0, 0],
			[0, 0, 0],
			[0, 0, 0],
		],
		currentPlayer: 0,
		isComplete: true,
		myPlayerNumber: 0,
		playerCount: 0,
		points: [0, 0, 0],
		turnCount: 0,
		winner: undefined,
		winningFields: undefined,
	};
}

function reducer(state: GameState, msg: Envelope) {
	console.log('<--', MessageType[msg.type], msg);

	switch (msg.type) {
		case MessageType.BadRequest:
			return onBadRequest(state, msg.data as BadRequestMessage);
		case MessageType.NewPlayer:
			return onNewPlayer(state, msg.data as NewPlayerMessage);
		case MessageType.Update:
			return onGameStateUpdate(state, msg.data as UpdateMessage);
		default:
			console.log(`Message ${MessageType[msg.type]} is unhandled`);
			return state;
	}
}

function onBadRequest(state: GameState, msg: BadRequestMessage): GameState {
	console.log(`<-- BadRequest: ${msg.message}`);
	return state;
}

function onNewPlayer(state: GameState, msg: NewPlayerMessage): GameState {
	console.log(`<-- NewPlayer: ${msg.currentPlayerNumber}`);

	return {
		...state,
		myPlayerNumber: msg.currentPlayerNumber,
		playerCount: msg.playerCount,
	};
}

function onGameStateUpdate(state: GameState, msg: UpdateMessage): GameState {
	console.log(`<-- Update: ${msg.board}`);

	return {
		...state,
		board: msg.board,
		currentPlayer: msg.currentPlayer,
		isComplete: msg.isComplete,
		points: msg.points,
		turnCount: msg.turnCount,
		winner: msg.winner,
		winningFields: msg.winningFields,
	};
}

interface GameConnectionProps {
	gameId: string;
	clientId: string;
	onMessage: (msg: Envelope) => void,
}

interface GameConnectionMethods {
	send: (type: MessageType, data: unknown) => void,
}

let counter = 0;

const GameConnection = forwardRef((props: GameConnectionProps, ref: React.Ref<GameConnectionMethods>) => {

	const state = useRef<GameConnectionProps & { socket?: WebSocket | null }>({
		...props,
	});

	useImperativeHandle(ref, () => {
		return {
			send: function (type: MessageType, data: unknown) {
				const msg: Envelope = { type, data };
				state.current.socket?.send(JSON.stringify(msg));
			}
		};
	}, []);

	useLayoutEffect(() => {
		const { gameId, clientId, onMessage } = state.current;
		const socket = new WebSocket(`wss://${window.location.host}/api/tictactoe/${gameId}?cid=${clientId}`);
		const $id = ++counter;

		console.log(`Socket(${$id}) opening`);

		socket.addEventListener('open', function () {
			console.log(`Socket(${$id}) opened`);
		});

		socket.addEventListener('message', function (msg: MessageEvent<string>) {
			console.log(`Socket(${$id}) inc msg`);
			onMessage(JSON.parse(msg.data));
		});

		socket.addEventListener('close', function (ev: CloseEvent) {
			console.log(`Socket(${$id}) closed: code=${ev.code}, reason=${ev.reason}, wasClean=${ev.wasClean}`)
		});

		socket.addEventListener('error', function (ev: Event) {
			console.error(`Socket(${$id}) error:`, ev);
		});

		state.current.socket = socket;

		return () => {
			console.log(`Clean-up game(${$id}) connection; closing 1`);
			socket.close();

			if (socket.readyState === socket.CONNECTING) {
				socket.addEventListener('open', function () {
					console.log(`Clean-up game(${$id}) connection; closing 2`);
					this.close();
				});
			}
		};
	}, []);

	return (
		<></>
	);
});

const Game: React.FC = () => {
	const { id: gameId } = useParams();
	const appContext = useContext(AppContext);
	const [state, dispatch] = useReducer(reducer, null, initState);
	const connection = useRef<GameConnectionMethods>(null);

	const clientId = appContext.clientId;

	function send(type: MessageType, data: unknown) {
		connection.current!.send(type, data);
	}

	const playTurn = useCallback((x: number, y: number) => {
		const active = state.currentPlayer === state.myPlayerNumber;
		console.log(`Game: Clicked [${x}, ${y}]; active=${active}`);
		if (active)
			send(MessageType.Turn, { x, y } as TurnMessage);
	}, [state.currentPlayer, state.myPlayerNumber]);

	const restartGame = useCallback(() => {
		send(MessageType.Restart, null!);
	}, []);

	function foo(a: Envelope) {
		console.log('foo', a);
		dispatch(a);
	}

	return (
		<>
			{gameId && <GameConnection key={`${gameId},${clientId}`} gameId={gameId} clientId={clientId} onMessage={foo} ref={connection} />}
			<div>
				<p style={{ 'cursor': 'pointer' }} title="Click to copy URL" onClick={copyUrl}>Lobby number: {gameId}</p >
				<div className="game">
					<Board fields={state.board} highlight={state.winningFields} onItemClick={playTurn} active={state.myPlayerNumber === state.currentPlayer} />
					<div className="info">
						<div>
							<div>You are: </div><Field player={state.myPlayerNumber} />
							{!state.isComplete
								? (<><div>Now plays: </div><Field player={state.currentPlayer} /></>)
								: (<><div>Winner: </div><Field player={state.winner!} /></>)
							}
						</div>
					</div>
				</div>
				<div className="status">
					<span>Players</span>
					<span>{state.playerCount}</span>
					<span>Turns</span>
					<span>{state.turnCount}</span>
					<span>Wins</span>
					<span>{state.points[state.myPlayerNumber]}</span>
					<span>Losses</span>
					<span>{state.points[(state.myPlayerNumber % 2) + 1]}</span>
					<span>Draws</span>
					<span>{state.points[0]}</span>
				</div>
				<button onClick={restartGame}>Restart!</button >
			</div>
		</>
	);
};

export default Game;
