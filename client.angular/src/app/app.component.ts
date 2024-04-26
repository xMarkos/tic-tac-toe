import { Component } from '@angular/core';
import { SessionService } from './session.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {

  clientId: string;

  constructor(session: SessionService) {
    this.clientId = session.clientId;
  }
}
