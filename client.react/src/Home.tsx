import React, { useCallback, useRef } from 'react';
import { useNavigate } from 'react-router-dom';

interface CreateGameResult {
	gameId: string;
}

export default function Home() {
	const refInput = useRef<HTMLInputElement>(null);
	const navigate = useNavigate();

	const joinLobby = useCallback((id: string) => {
		navigate(`/lobby/${id}`)
	}, [navigate]);

	const joinLobbyClicked = useCallback(() => {
		joinLobby(refInput.current!.value);
	}, [joinLobby, refInput]);

	const keyHandler = useCallback((ev: React.KeyboardEvent<HTMLInputElement>) => {
		if (ev.key === 'Enter')
			joinLobbyClicked();
	}, [joinLobbyClicked]);

	const createLobby = useCallback(async () => {
		const response = await fetch('/api/tictactoe', {
			method: 'POST',
		});

		if (response.ok) {
			const result: CreateGameResult = await response.json();
			joinLobby(result.gameId);
		}
	}, [joinLobby]);

	return (
		<>
			<button onClick={createLobby}>Create lobby</button >
			<label>
				<span> or join existing </span>
				<input ref={refInput} onKeyUp={keyHandler} />
			</label>
			<button onClick={joinLobbyClicked}>Join</button >
		</>
	);
}
