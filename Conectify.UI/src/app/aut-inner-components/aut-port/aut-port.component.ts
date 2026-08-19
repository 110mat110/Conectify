import { Component, Input, OnInit } from '@angular/core';
import { PortRule } from 'src/models/Automatization/PortRule';

/** Options of a port of a function - the label its pin carries on the parent board. */
@Component({
  selector: 'app-aut-port',
  templateUrl: './aut-port.component.html',
  styleUrls: ['./aut-port.component.css']
})
export class AutPortComponent implements OnInit {
  @Input() Rule?: PortRule;
  portName: string = "";

  ngOnInit(): void {
    if (this.Rule?.behaviour) {
      this.portName = this.Rule.behaviour.Name;
    }
  }

  onChange(): void {
    if (this.Rule?.behaviour) {
      this.Rule.behaviour.Name = this.portName;
    }
  }
}
