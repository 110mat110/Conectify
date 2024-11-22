import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { MessagesService } from '../messages.service';
import { AutomatizationBase } from 'src/models/automatizationComponent';
import { CdkDragEnd } from '@angular/cdk/drag-drop';
import { BefetchAutomatizationService } from '../befetch-automatization.service';
import { RuleProviderService } from '../rule-provider.service';
import { BehaviourMenuItem } from 'src/models/Automatization/BehaviourMenuItem';
import { EditRule } from 'src/models/Automatization/EditRule';
import { RuleConnection } from 'src/models/Automatization/RuleConnection';
import { MatButton } from '@angular/material/button';



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

export class ButtonReferences { elementRef: MatButton; id: string; constructor(elementRef: MatButton, id: string) { this.elementRef = elementRef, this.id = id } };

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
  connections: RuleConnection[] = [];
  polylines: Polyline[] = [];
  source?: string;
  destination?: string;
  references: ButtonReferences[] = [];
  supportedRules: BehaviourMenuItem[] = [];
  @ViewChild('rules') rulesCanvas?: ElementRef<SVGElement>;

  inputMapping: { [key: number]: string } = {
    0: 'P',
    1: 'T',
    2: 'V'
  };

  ngOnInit(): void {
    this.be.GetAllBehaviours().subscribe(x => {
      this.supportedRules = x;
      this.DrawConnections();
    }, (err) => {
      console.error(JSON.stringify(err));
    }
    );

    this.be.GetAllConnections().subscribe(x => {
      this.connections = x;
      this.DrawConnections();
    })


  }

  createRule() {
    if (this.selectedRuleId != "") {
      this.ruleService.createRule(this.selectedRuleId);
    }
  }

  handleButtonGenerated(event: { elementRef: MatButton; id: string }): void {
    console.log('Generated Button ID:', event.id);
    this.references.push(new ButtonReferences(event.elementRef, event.id));

    this.DrawConnections();
  }

  dragEnd(event: CdkDragEnd, rule: AutomatizationBase) {
    const { x, y } = event.source.element.nativeElement.getBoundingClientRect();
    rule.dragPosition = { x: x + event.source.element.nativeElement.offsetWidth / 2 + window.scrollX, y: y + event.source.element.nativeElement.offsetHeight / 2 + window.scrollY };
    this.MoveComponent();
    let apiModel: EditRule = { id: rule.id, x: rule.dragPosition.x, y: rule.dragPosition.y, behaviourId: rule.behaviorId, parameters: rule.getParametersJSon() };
    this.be.saveRule(apiModel);
  }

  SourceClick(source: string) {
    this.source = source;
  }

  DestinationClick(dest: string) {
    this.destination = dest;
    this.SetConnection();
  }

  SetConnection() {
    this.messenger.addMessage("Set connection");

    if (this.source && this.destination) {
      this.be.setConnection(this.source, this.destination).subscribe(x => {
        this.be.GetAllConnections().subscribe(x => {
          this.connections = x;
          this.DrawConnections();
        })
      });
    }
  }

  MoveComponent() {
    this.DrawConnections();
  }

  DrawConnections() {
    this.polylines = [];
    this.connections.forEach(connecion => {
      console.log("Drawing line")

      let sourceElement = this.references.find(x => x.id == connecion.sourceId);
      let destinationElement = this.references.find(x => x.id == connecion.destinationId);

      if (sourceElement?.elementRef._elementRef.nativeElement && destinationElement?.elementRef._elementRef.nativeElement) {
        this.DrawLine(sourceElement?.elementRef._elementRef.nativeElement, destinationElement?.elementRef._elementRef.nativeElement);
      }


    })
  }

  DrawLine(destination: HTMLElement, source: HTMLElement) {
    const { x: dx, y: dy, height: dh, width: dw } = destination.getBoundingClientRect();
    const { x: sx, y: sy, height: sh, width: sw } = source.getBoundingClientRect();

    let verticalOffset = this.rulesCanvas?.nativeElement.getBoundingClientRect().top ?? 0;
    let horizontalOffset = this.rulesCanvas?.nativeElement.getBoundingClientRect().left ?? 0;
    this.polylines.push(new Polyline(dx + (dw / 2) - horizontalOffset, dy + (dh / 2) - verticalOffset, sx + (sw / 2) - horizontalOffset, sy + (sh / 2) - verticalOffset))
  }

  AddCustomInput(inputName: string) {
    this.be.createActuator({ actuatorName: inputName });

    this.ruleService.LoadAllRules();
  }
}