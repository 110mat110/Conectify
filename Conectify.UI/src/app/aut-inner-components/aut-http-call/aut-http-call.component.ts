import { Component, Input } from '@angular/core';
import { HttpCallRule } from 'src/models/Automatization/HttpCallRule';

@Component({
  selector: 'app-aut-http-call',
  templateUrl: './aut-http-call.component.html',
  styleUrl: './aut-http-call.component.css'
})
export class AutHttpCallComponent {
  @Input() Rule?: HttpCallRule;
  httpAdress: string = "";

  constructor() { }

  ngOnInit(): void {
    if(this.Rule?.behaviour){
    this.httpAdress = this.Rule.behaviour.Http;
    }
  }

  onChange(): void{
    if(this.Rule?.behaviour){
      this.Rule.behaviour.Http = this.httpAdress;
    }
  }
}
