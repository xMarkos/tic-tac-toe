import { Component, Input, OnInit, ViewEncapsulation } from '@angular/core';
import { WebSocketSubject, webSocket } from 'rxjs/webSocket';
import { MessageType, BadRequestMessage, NewPlayerMessage, TurnMessage, UpdateMessage, Envelope } from '/TicTacToe/api.models';
import { SessionService } from '/app/session.service';

@Component({
  selector: 'app-game',
  templateUrl: './game.component.html',
  styleUrls: ['/src/styles.css', './game.component.css'],
  encapsulation: ViewEncapsulation.ShadowDom,
})
export class GameComponent implements OnInit {

  @Input({ alias: 'id', required: true })
  gameId!: string;

  private socket!: WebSocketSubject<Envelope>;

  playerCount: number = 0;
  myPlayerNumber: number = 0;
  currentPlayer: number = 0;
  board!: number[][];
  turnCount: number = 0;
  isComplete?: boolean;
  winner?: number;
  winningFields?: [number, number][];
  points: number[] = [0, 0, 0];

  constructor(private session: SessionService) { }

  ngOnInit(): void {
    this.board = [
      [0, 0, 0],
      [0, 0, 0],
      [0, 0, 0]
    ];

    this.socket =
      webSocket({
        url: `wss://${window.location.host}/api/tictactoe/${this.gameId}?cid=${this.session.clientId}`,
      });

    this.socket.subscribe({
      next: this.onMessage.bind(this),
      error: err => console.log('socket error', err),
      complete: () => console.log('socket closed'),
    });
  }

  private async onMessage(msg: Envelope) {
    console.log('<--', MessageType[msg.type], msg);

    switch (msg.type) {
      case MessageType.BadRequest:
        this.onBadRequest(<BadRequestMessage>msg.data);
        break;
      case MessageType.NewPlayer:
        this.onNewPlayer(<NewPlayerMessage>msg.data);
        break;
      case MessageType.Update:
        this.onUpdate(<UpdateMessage>msg.data);
        break;
      default:
        console.log(`Message ${MessageType[msg.type]} is unhandled`);
    }
  }

  private onBadRequest(msg: BadRequestMessage) {
    console.log(`<-- BadRequest: ${msg.message}`);
  }

  private onNewPlayer(msg: NewPlayerMessage) {
    console.log(`<-- NewPlayer: ${msg.currentPlayerNumber}`);
    this.myPlayerNumber = msg.currentPlayerNumber;
    this.playerCount = msg.playerCount;
  }

  private onUpdate(msg: UpdateMessage) {
    console.log(`<-- Update: ${msg.board}`);
    this.board = msg.board;
    this.currentPlayer = msg.currentPlayer;
    this.isComplete = msg.isComplete;
    this.turnCount = msg.turnCount;
    this.winner = msg.winner;
    this.winningFields = msg.winningFields;
    this.points = msg.points;
  }

  itemClicked(x: number, y: number) {
    let active = this.currentPlayer === this.myPlayerNumber;
    console.log(`Game: Clicked [${x}, ${y}]; active=${active}`);
    if (active)
      this.send(MessageType.Turn, <TurnMessage>{ x, y });
  }

  restartGame() {
    this.send(MessageType.Restart, null!);
  }

  private send(type: MessageType, data: any) {
    this.socket.next(<Envelope>{ type, data });
  }

  copyUrl() {
    navigator.clipboard.writeText(window.location.href);
  }
}
