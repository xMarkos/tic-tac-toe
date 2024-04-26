import { HttpClient } from '@angular/common/http';
import { Component, ViewEncapsulation } from '@angular/core';
import { Router } from '@angular/router';

interface CreateGameResult {
  gameId: string;
}

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['/src/styles.css', './home.component.css'],
  encapsulation: ViewEncapsulation.ShadowDom,
})
export class HomeComponent {

  constructor(private http: HttpClient, private router: Router) { }

  createLobby() {
    this.http.post<CreateGameResult>('/api/tictactoe', null).subscribe(data => {
      this.joinLobby(data.gameId);
    });
  }

  joinLobby(id: string) {
    this.router.navigate(['lobby', id]);
  }
}
