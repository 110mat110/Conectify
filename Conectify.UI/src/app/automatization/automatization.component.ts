import { Component, OnInit } from '@angular/core';
import { ValueInitRule } from 'src/models/Automatization/ValueInitRule';
import { MessagesService } from '../messages.service';
import {v4} from 'uuid';
import { MiddleRule } from 'src/models/Automatization/MiddleRule';
import { AutomatizationBase, AutomatizationBaseWithTarget } from 'src/models/automatizationComponent';
import { CdkDragEnd } from '@angular/cdk/drag-drop';
import { ChangeDestinationRule } from 'src/models/Automatization/ChangeDestinationRule';
import { BefetchAutomatizationService } from '../befetch-automatization.service';
import { RuleProviderService } from '../rule-provider.service';

interface Rule {
  value: string;
  viewValue: string;
}

export class Polyline {
  stroke: string;
  x1:number;
  y1:number;
  x2:number;
  y2:number;
  constructor(
    x1:number,
    y1:number,
    x2:number,
    y2:number,
    stroke?: string,
  ) {
    this.x1 = x1;
    this.x2 = x2;
    this.y1 = y1;
    this.y2 = y2;
    this.stroke = stroke || "yellow";
  }
}

@Component({
  selector: 'app-automatization',
  templateUrl: './automatization.component.html',
  styleUrls: ['./automatization.component.css']
})
export class AutomatizationComponent implements OnInit {

  selectedRule: string = "kktina";
  constructor(public messenger: MessagesService, public ruleService: RuleProviderService) { }
  lineToFill: number = 0;
  lineHeight: number = 150;

  source?: AutomatizationBaseWithTarget;
  destination?: AutomatizationBase;

  polylines: Polyline[] = [];

  supportedRules: Rule[] = [
    {value: 'inputValue', viewValue: 'Input from sensor value'},
    {value: 'changeDest', viewValue: 'Change destination actuator'},
  ];

  ngOnInit(): void {
    this.DrawConnections();
  }

 createRule(){
   this.ruleService.createRule(this.selectedRule);
 }

  dragEnd(event: CdkDragEnd, rule: AutomatizationBase) {
    rule.dragPosition = event.dropPoint;
    this.MoveComponent();
  }

  SourceClick(source: AutomatizationBaseWithTarget){
    this.source = source;
  }

  DestinationClick(dest: AutomatizationBase){
    this.destination = dest;
    this.SetConnection();
  }

  SetConnection(){
    this.messenger.addMessage("Set connection");

    if(this.source && this.destination){
      this.source.targets.push(this.destination.id);
      this.DrawConnections();
    }
  }

  MoveComponent(){
    this.DrawConnections();
  }

  DrawConnections(){
    this.polylines = [];
    this.ruleService.Rules.forEach(rule => {
      console.log("Drawing line")
      if(rule instanceof AutomatizationBaseWithTarget && rule.targets){
        rule.targets.forEach(target => {
          var targetRule = this.ruleService.getRuleByID(target);
          if(targetRule)
            this.DrawLine(targetRule.dragPosition, rule.dragPosition);
        })
      }
    })
  }

  DrawLine(source: {x:number, y:number}, dest: {x:number, y:number}){
    var verticalOffset = -220;
    
    this.messenger.addMessage("Drawing line");
    this.polylines.push( new Polyline(source.x,source.y + verticalOffset, dest.x, dest.y + verticalOffset))
  }
}
