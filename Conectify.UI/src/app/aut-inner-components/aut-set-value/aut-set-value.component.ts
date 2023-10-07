import { Component, Input, OnInit } from '@angular/core';
import { SetValueRule } from 'src/models/Automatization/SetValueRule';

@Component({
  selector: 'app-aut-set-value',
  templateUrl: './aut-set-value.component.html',
  styleUrls: ['./aut-set-value.component.css']
})
export class AutSetValueComponent implements OnInit {
  @Input() Rule?: SetValueRule;
  stringvalue: string = "";
  numericvalue: number = 0;

  constructor() { }

  ngOnInit(): void {
    if(this.Rule?.behaviour){
    this.stringvalue = this.Rule.behaviour.StringValue;
    this.numericvalue = this.Rule.behaviour.NumericValue;
    }
  }

  onChange(): void{
    if(this.Rule?.behaviour){
      this.Rule.behaviour.NumericValue = this.numericvalue;
      this.Rule.behaviour.StringValue = this.stringvalue;
    }
  }

}
