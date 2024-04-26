import { Component, EventEmitter, HostBinding, Input, Output, ViewEncapsulation } from '@angular/core';

@Component({
  selector: 'app-board',
  templateUrl: './board.component.html',
  styleUrl: './board.component.css',
  encapsulation: ViewEncapsulation.ShadowDom,
})
export class BoardComponent {

  @HostBinding('hidden')
  get isHidden() { return !this.fields; }

  @Input({ required: true })
  fields?: number[][];

  @Input()
  highlight?: [number, number][];

  @Output()
  itemClicked = new EventEmitter<{ x: number, y: number }>;

  onItemClicked(x: number, y: number) {
    this.itemClicked.emit({ x: x, y: y });
  }

  shouldHighlight(x: number, y: number) {
    return this.highlight != null && this.highlight.some(([x0, y0]) => x0 === x && y0 === y);
  }
}
