import { Component, Input, OnInit } from '@angular/core';
import { DecisionRule } from 'src/models/Automatization/DecisionRule';

@Component({
  selector: 'app-aut-decision',
  templateUrl: './aut-decision.component.html',
  styleUrls: ['./aut-decision.component.css']
})
export class AutDecisionComponent implements OnInit {

  @Input() Rule?: DecisionRule;

  rules: string[] = [">", "<", "=", ">=", "<=", "!="];
  selectedRule: string = "=";
  decisionString: string = "if P1 " + this.selectedRule + " P2 then V -> O"

  constructor() { }

  ngOnInit(): void {
    if(this.Rule?.behaviour?.Rule){
      this.selectedRule = this.Rule.behaviour.Rule
    } else{
      this.selectedRule = "=";
    }
  }

  toggleColor(rule: string) {
    this.selectedRule = rule;
    if(this.Rule){
      this.Rule.behaviour.Rule = this.selectedRule;
    }
  }

  getColor(rule: string): string {
    // Get the current text color for the day
    return this.selectedRule === rule ? 'darkgreen' : 'red'
  }
}
