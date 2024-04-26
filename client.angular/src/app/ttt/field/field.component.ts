import { Component, EventEmitter, Input, Output, ViewEncapsulation } from '@angular/core';

@Component({
  selector: 'app-field',
  templateUrl: './field.component.html',
  styleUrl: './field.component.css',
  encapsulation: ViewEncapsulation.ShadowDom,
})
export class FieldComponent {

  @Input()
  player: number = 0;

  @Input()
  highlight: boolean = false;

  @Output()
  click = new EventEmitter<never>();

  clicked(ev: Event) {
    ev.stopPropagation();
    this.click.emit();
  }
}
