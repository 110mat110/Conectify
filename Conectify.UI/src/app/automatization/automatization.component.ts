import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { ValueInitRule } from 'src/models/Automatization/ValueInitRule';
import { MessagesService } from '../messages.service';
import { AutomatizationBase } from 'src/models/automatizationComponent';
import { CdkDragEnd } from '@angular/cdk/drag-drop';
import { ChangeDestinationRule } from 'src/models/Automatization/ChangeDestinationRule';
import { BefetchAutomatizationService } from '../befetch-automatization.service';
import { RuleProviderService } from '../rule-provider.service';
import { BehaviourMenuItem } from 'src/models/Automatization/BehaviourMenuItem';
import { EditRule } from 'src/models/Automatization/EditRule';

interface Rule {
  value: string;
  viewValue: string;
}

export class Polyline {
  stroke: string;
  x1: number;
  y1: number;
  x2: number;
  y2: number;
  constructor(
    x1: number,
    y1: number,
    x2: number,
    y2: number,
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

  selectedRuleId: string = "";
  constructor(public messenger: MessagesService, public ruleService: RuleProviderService, public be: BefetchAutomatizationService) { }
  lineToFill: number = 0;
  lineHeight: number = 150;

  source?: AutomatizationBase;
  destination?: AutomatizationBase;
  parameter?: AutomatizationBase;

  polylines: Polyline[] = [];
  @ViewChild('rules') element?: ElementRef;

  supportedRules: BehaviourMenuItem[] = [];

  ngOnInit(): void {
    this.be.GetAllBehaviours().subscribe(x => {
      console.log("Loaded rules");
      this.supportedRules = x;
      this.DrawConnections();
    }, (err) => {
      console.error(JSON.stringify(err));
    }
    );
    this.DrawConnections();
  }

  createRule() {
    if (this.selectedRuleId != "") {
      this.ruleService.createRule(this.selectedRuleId);
    }
  }

  dragEnd(event: CdkDragEnd, rule: AutomatizationBase) {
    const { x, y } = event.source.element.nativeElement.getBoundingClientRect();
    rule.dragPosition = { x: x + event.source.element.nativeElement.offsetWidth / 2 + window.scrollX, y: y + event.source.element.nativeElement.offsetHeight / 2 + window.scrollY };
    this.MoveComponent();
    let apiModel: EditRule = { id: rule.id, x: rule.dragPosition.x, y: rule.dragPosition.y, behaviourId: rule.behaviorId, parameters: rule.getParametersJSon() };
    this.be.saveRule(apiModel);
  }

  SourceClick(source: AutomatizationBase) {
    this.source = source;
  }

  DestinationClick(dest: AutomatizationBase) {
    this.destination = dest;
    this.SetConnection();
  }

  ParameterClick(param: AutomatizationBase){
    this.parameter = param;
    this.SetParameter();
  }

  SetConnection() {
    this.messenger.addMessage("Set connection");

    if (this.source && this.destination) {
      const index = this.source.targets.indexOf(this.destination.id);
      if(index !== -1){
        this.source.targets.splice(index,1);
        this.be.removeConnection(this.source.id, this.destination.id);
      } else{
        this.source.targets.push(this.destination.id);

        this.be.addNewConnection(this.source.id, this.destination.id);
      }
      this.DrawConnections();
    }
  }

  SetParameter(){
    if (this.source && this.parameter) {

    const index = this.parameter.parameters.indexOf(this.source.id);
    if(index !== -1){
      this.parameter.parameters.splice(index,1);
      this.be.removeParameterConnection(this.source.id, this.parameter.id);
    } else{
      this.parameter.parameters.push(this.source.id);

      this.be.addNewParameterConnection(this.source.id, this.parameter.id);
    }
    this.DrawConnections();
  }
}

  MoveComponent() {
    this.DrawConnections();
  }

  DrawConnections() {
    this.polylines = [];
    this.ruleService.Rules.forEach(rule => {
      console.log("Drawing line")
      if (rule.targets) {
        rule.targets.forEach(target => {
          var targetRule = this.ruleService.getRuleByID(target);
          if (targetRule)
            this.DrawLine(targetRule.dragPosition, rule.dragPosition);
        })
      }
      if(rule.parameters){
        rule.parameters.forEach(param => {
          var paramRule = this.ruleService.getRuleByID(param);
          if (paramRule)
            this.DrawParamLine(rule.dragPosition, paramRule.dragPosition);
        })
      }
    })
  }

  DrawLine(destination: { x: number, y: number }, source: { x: number, y: number }) {
    const cubeWidth = 170;
    const { x, y } = this.element?.nativeElement.getBoundingClientRect();
    let verticalOffset = - y - window.scrollY;
    let horizontalOffset = -x - window.scrollX;
    this.polylines.push(new Polyline(destination.x - (cubeWidth/2) + horizontalOffset -30, destination.y + verticalOffset, source.x + (cubeWidth/2) +horizontalOffset, source.y + verticalOffset))
  }

  DrawParamLine(destination: { x: number, y: number }, source: { x: number, y: number }) {
    const cubeHeight = 170;
    const cubeWidth = 170;
    const { x, y } = this.element?.nativeElement.getBoundingClientRect();
    let verticalOffset = - y - window.scrollY;
    let horizontalOffset = -x - window.scrollX;
    this.polylines.push(new Polyline(destination.x + horizontalOffset, destination.y - (cubeHeight/2) + verticalOffset, source.x + (cubeWidth/2) +horizontalOffset, source.y + verticalOffset, "blue"))
  }

  AddCustomInput(inputName: string) {
    this.be.createActuator({ actuatorName: inputName });

    this.ruleService.LoadAllRules();
  }
}
