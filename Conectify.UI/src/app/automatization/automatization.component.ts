import { AfterViewInit, Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { MessagesService } from '../messages.service';
import { AutomatizationBase } from 'src/models/automatizationComponent';
import { CdkDragEnd } from '@angular/cdk/drag-drop';
import { BefetchAutomatizationService } from '../befetch-automatization.service';
import { RuleProviderService } from '../rule-provider.service';
import { BehaviourMenuItem } from 'src/models/Automatization/BehaviourMenuItem';
import { EditRule } from 'src/models/Automatization/EditRule';
import { RuleConnection } from 'src/models/Automatization/RuleConnection';
import { MatButton } from '@angular/material/button';
import LeaderLine from 'leader-line-new';



interface Rule {
  value: string;
  viewValue: string;
}

export class ButtonReferences { elementRef: MatButton; id: string; constructor(elementRef: MatButton, id: string) { this.elementRef = elementRef, this.id = id } };

@Component({
  selector: 'app-automatization',
  templateUrl: './automatization.component.html',
  styleUrls: ['./automatization.component.css']
})

export class AutomatizationComponent implements OnInit, OnDestroy, AfterViewInit {

  selectedRuleId: string = "";
  constructor(public messenger: MessagesService, public ruleService: RuleProviderService, public be: BefetchAutomatizationService) { }
  ngAfterViewInit(): void {
    setTimeout(() => {
      this.ReDrawConnections();
    }, 500);
    }
  lineToFill: number = 0;
  lineHeight: number = 150;
  connections: RuleConnection[] = [];
  source?: string;
  destination?: string;
  references: ButtonReferences[] = [];
  supportedRules: BehaviourMenuItem[] = [];
  lines: LeaderLine[] = [];
  @ViewChild('rules') rulesCanvas?: ElementRef<SVGElement>;

  ngOnInit(): void {
    this.be.GetAllBehaviours().subscribe(x => {
      this.supportedRules = x;
    }, (err) => {
      console.error(JSON.stringify(err));
    }
    );

    this.be.GetAllConnections().subscribe(x => {
      this.connections = x;
      this.DrawConnections();
    })

    window.addEventListener('resize', () => {
      this.ReDrawConnections();
    });
  }

  createRule() {
    if (this.selectedRuleId != "") {
      this.ruleService.createRule(this.selectedRuleId);
    }
  }

  handleButtonGenerated(event: { elementRef: MatButton; id: string }): void {
    console.log('Generated Button ID:', event.id);
    this.references.push(new ButtonReferences(event.elementRef, event.id));
  }

  dragEnd(event: CdkDragEnd, rule: AutomatizationBase) {
    const { x, y } = event.source.element.nativeElement.getBoundingClientRect();
    rule.dragPosition = { x: x + event.source.element.nativeElement.offsetWidth / 2 + window.scrollX, y: y + event.source.element.nativeElement.offsetHeight / 2 + window.scrollY };
    this.MoveComponent();
    let apiModel: EditRule = { id: rule.id, x: rule.dragPosition.x, y: rule.dragPosition.y, behaviourId: rule.behaviorId, parameters: rule.getParametersJSon() };
    this.be.saveRule(apiModel);

    setTimeout(() => {
      this.MoveComponent();
    }, 50);
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
    this.ReDrawConnections();
  }

  ReDrawConnections(){
    this.lines.forEach(line => {
      line.position();
    });
  }

  DrawConnections() {
    // First check if we have all references needed
    const allReferencesReady = this.connections.every(conn => 
      this.references.some(ref => ref.id === conn.sourceId) && 
      this.references.some(ref => ref.id === conn.destinationId)
    );
    
    if (!allReferencesReady) {
      console.warn('Not all references are ready. Delaying drawing.');
      setTimeout(() => this.DrawConnections(), 200);
      return;
    }
    
    this.lines.forEach(line => line.remove());
    this.lines = [];
    this.connections.forEach(connecion => {
      console.log("Drawing line")

      let sourceElement = this.references.find(x => x.id == connecion.sourceId);
      let destinationElement = this.references.find(x => x.id == connecion.destinationId);

      if (sourceElement?.elementRef._elementRef.nativeElement && destinationElement?.elementRef._elementRef.nativeElement) {
        this.AddLine(sourceElement?.elementRef._elementRef.nativeElement, destinationElement?.elementRef._elementRef.nativeElement);
      }
    });
    this.ReDrawConnections();
  }

  AddLine(source: HTMLElement, destination: HTMLElement) {
    var line = new LeaderLine(
      source,
      destination,
      {
        color: '#cccccc',
        size: 3,
        startSocket: 'right',
        endSocket: 'left',
        path: 'fluid', // Options: 'straight', 'arc', 'fluid', 'magnet', 'grid'
        startPlug: 'behind',
        endPlug: 'arrow3',
        startPlugSize: 1,
        endPlugSize: 1.5,
        startPlugColor: '#d1b906',
        endPlugColor: '#bf5102',
        gradient: true,
        dash: {animation: true} // Animated dashed line
      }
    );
    this.lines.push(line);
  };

  AddCustomInput(inputName: string) {
    this.be.createActuator({ actuatorName: inputName });

    this.ruleService.LoadAllRules();
  }

  ngOnDestroy(): void {
    this.lines.forEach(line => line.remove());
    window.removeEventListener('resize', () => {
      this.ReDrawConnections();
    });
  }
}