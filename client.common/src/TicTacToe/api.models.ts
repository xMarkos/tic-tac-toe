export enum MessageType {
	Turn,
	Update,
	BadRequest,
	Restart,
	NewPlayer,
}

export interface BadRequestMessage {
	message: string,
}

export interface NewPlayerMessage {
	playerCount: number;
	currentPlayerNumber: number;
}

export interface UpdateMessage {
	board: number[][];
	turnCount: number;
	currentPlayer: number;
	isComplete: boolean;
	winner?: number;
	winningFields?: [number, number][],
	points: [number, number, number];
}

export interface TurnMessage {
	x: number,
	y: number;
}

export interface Envelope {
	type: MessageType;
	data: unknown;
}
