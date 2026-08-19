import { AfterViewInit, Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { MessagesService } from '../messages.service';
import { AutomatizationBase } from 'src/models/automatizationComponent';
import { CdkDragEnd } from '@angular/cdk/drag-drop';
import { BefetchAutomatizationService } from '../befetch-automatization.service';
import { RuleProviderService } from '../rule-provider.service';
import { BehaviourMenuItem } from 'src/models/Automatization/BehaviourMenuItem';
import { EditRule } from 'src/models/Automatization/EditRule';
import { RuleConnection } from 'src/models/Automatization/RuleConnection';
import { FunctionModel, FunctionPortDirection } from 'src/models/Automatization/FunctionModel';
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
  newPortType: number = 2;
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
  /** Everything the service knows, used to label a block with the name of its behaviour. */
  allBehaviours: BehaviourMenuItem[] = [];
  /** What the "new rule" menu offers. */
  supportedRules: BehaviourMenuItem[] = [];
  lines: LeaderLine[] = [];
  /** Points drawn on the level that is open, everything else cannot be reached by a line. */
  visiblePoints: Set<string> = new Set<string>();
  @ViewChild('rules') rulesCanvas?: ElementRef<SVGElement>;

  ngOnInit(): void {
    this.be.GetAllBehaviours().subscribe(x => {
      this.allBehaviours = x;
      // Ports of a function are added from the function itself, not from the plain rule menu.
      this.supportedRules = x.filter(behaviour => !behaviour.isInternal);
    }, (err) => {
      console.error(JSON.stringify(err));
    }
    );

    this.be.GetAllConnections().subscribe(x => {
      this.connections = x;
      this.DrawConnections();
    })

    // Opening or leaving a function replaces every box on the board, so the old anchors go with them.
    this.ruleService.levelChanged = () => this.OnLevelChanged();
    this.OnLevelChanged();

    window.addEventListener('resize', () => {
      this.ReDrawConnections();
    });
  }

  createRule() {
    if (this.selectedRuleId != "") {
      this.ruleService.createRule(this.selectedRuleId);
    }
  }

  createFunction(name: string) {
    if (name.trim().length > 0) {
      this.ruleService.createFunction(name.trim());
    }
  }

  addPort(direction: FunctionPortDirection, name: string) {
    const functionId = this.ruleService.currentFunctionId;

    if (functionId) {
      this.ruleService.addFunctionPort(functionId, direction, this.newPortType, name.trim());
    }
  }

  get portDirections() {
    return FunctionPortDirection;
  }

  OpenFunction(functionId?: string) {
    this.CancelConnection();
    this.ruleService.OpenFunction(functionId);
  }

  /** Height that fits every box plus room to drag further down. */
  get canvasHeight(): number {
    const boxes = [
      ...this.ruleService.visibleRules.map(rule => rule.drawingPos?.y ?? 0),
      ...this.ruleService.visibleFunctions.map(fn => this.functionDrawingPos(fn).y),
    ];
    const lowestBox = boxes.reduce((max, y) => Math.max(max, y), 0);

    return Math.max(700, lowestBox + this.lineHeight + 300);
  }

  /** Functions are placed the same way as rules, the stored position is the middle of the box. */
  functionDrawingPos(fn: FunctionModel): { x: number, y: number } {
    const x = fn.x == 0 && fn.y == 0 ? 400 : fn.x;
    const y = fn.x == 0 && fn.y == 0 ? 150 : fn.y;

    return { x: x - 110, y: y - 75 };
  }

  handleButtonGenerated(event: { elementRef: MatButton; id: string }): void {
    this.references = this.references.filter(reference => reference.id != event.id);
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

  functionDragEnd(event: CdkDragEnd, fn: FunctionModel) {
    const { x, y } = event.source.element.nativeElement.getBoundingClientRect();
    fn.x = x + event.source.element.nativeElement.offsetWidth / 2 + window.scrollX;
    fn.y = y + event.source.element.nativeElement.offsetHeight / 2 + window.scrollY;
    this.MoveComponent();

    this.be.saveFunction(fn.id, { name: fn.name, description: fn.description, x: fn.x, y: fn.y }).subscribe();

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

  CancelConnection() {
    this.source = undefined;
    this.destination = undefined;
  }

  SetConnection() {
    this.messenger.addMessage("Set connection");

    if (this.source && this.destination) {
      this.be.setConnection(this.source, this.destination).subscribe(x => {
        this.CancelConnection();
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

  /** Connections of the rule that lead to a box on another level and cannot be drawn here. */
  ExternalConnectionCount(rule: AutomatizationBase): number {
    const own = new Set<string>([...rule.inputs.map(x => x.id), ...rule.outputs.map(x => x.id)]);

    return this.connections.filter(connection =>
      (own.has(connection.sourceId) && !this.visiblePoints.has(connection.destinationId)) ||
      (own.has(connection.destinationId) && !this.visiblePoints.has(connection.sourceId))).length;
  }

  private OnLevelChanged(): void {
    this.references = [];
    this.lines.forEach(line => line.remove());
    this.lines = [];
    this.visiblePoints = this.ruleService.visiblePointIds();

    // The boxes of the new level are rendered in the next tick, only then can lines be attached.
    setTimeout(() => this.DrawConnections(), 100);
  }

  DrawConnections(attempt: number = 0) {
    // Only connections between two boxes of the level that is open can be drawn. The rest belongs to
    // another level and is reported on the box itself instead.
    const drawable = this.connections.filter(connection =>
      this.visiblePoints.has(connection.sourceId) && this.visiblePoints.has(connection.destinationId));

    const missingReferences = drawable.some(connection =>
      !this.references.some(ref => ref.id === connection.sourceId) ||
      !this.references.some(ref => ref.id === connection.destinationId));

    if (missingReferences && attempt < 10) {
      setTimeout(() => this.DrawConnections(attempt + 1), 200);
      return;
    }

    this.lines.forEach(line => line.remove());
    this.lines = [];
    drawable.forEach(connecion => {
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
        color: '#3a86ff',
        size: 2.5,
        startSocket: 'right',
        endSocket: 'left',
        path: 'fluid', // Options: 'straight', 'arc', 'fluid', 'magnet', 'grid'
        startPlug: 'behind',
        endPlug: 'arrow3',
        startPlugSize: 1,
        endPlugSize: 1.5,
        // Runs from the colour of an output pin to the colour of an input pin.
        startPlugColor: '#4fc3f7',
        endPlugColor: '#3a86ff',
        gradient: true,
        outlineColor: '#0a0a0a',
        outline: true,
        outlineSize: 0.15,
        dropShadow: { dx: 0, dy: 1, blur: 3, color: '#000000', opacity: 0.5 },
        dash: {animation: true} // Animated dashed line
      }
    );
    this.lines.push(line);
  };

  AddCustomInput(inputName: string) {
    this.be.createActuator({ actuatorName: inputName, functionId: this.ruleService.currentFunctionId });

    this.ruleService.LoadAllRules();
  }

  ngOnDestroy(): void {
    this.lines.forEach(line => line.remove());
    this.ruleService.levelChanged = undefined;
    window.removeEventListener('resize', () => {
      this.ReDrawConnections();
    });
  }
}
