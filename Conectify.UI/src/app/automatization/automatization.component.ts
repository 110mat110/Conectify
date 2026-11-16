import { AfterViewInit, Component, ElementRef, HostBinding, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { MessagesService } from '../messages.service';
import { AutomatizationBase } from 'src/models/automatizationComponent';
import { CdkDragEnd, CdkDragMove } from '@angular/cdk/drag-drop';
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

  /** Stored position of a box is its middle, drawn position its top left corner. */
  private static readonly boxCentre = { x: 110, y: 75 };
  private static readonly boxSize = { width: 200, height: 150 };
  /** Free room kept right of and below the furthest box, so there is always somewhere to drag to. */
  private static readonly canvasMargin = 400;
  /** How close to the edge of the window a dragged box starts scrolling the page. */
  private static readonly scrollEdge = 60;
  /** Side padding of the page, keep in sync with :host in the css. */
  private static readonly pagePadding = 16;

  selectedRuleId: string = "";
  newFunctionName: string = "";
  newInputName: string = "";
  newPortName: string = "";
  newPortType: number = 2;
  @ViewChild('canvas') canvas?: ElementRef<HTMLElement>;
  /** How far the box being dragged has reached, the canvas grows to it while dragging. */
  private dragReach = { x: 0, y: 0 };
  private lastPointer?: { x: number, y: number };
  private autoScrollSpeed = { x: 0, y: 0 };
  private autoScrollFrame?: number;
  private panFrom?: { x: number, y: number };
  private readonly onResize = () => this.ReDrawConnections();
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

    window.addEventListener('resize', this.onResize);
  }

  createRule() {
    if (this.selectedRuleId != "") {
      this.ruleService.createRule(this.selectedRuleId);
    }
  }

  createFunction() {
    const name = this.newFunctionName.trim();
    if (name.length > 0) {
      this.ruleService.createFunction(name);
      this.newFunctionName = "";
    }
  }

  addPort(direction: FunctionPortDirection) {
    const functionId = this.ruleService.currentFunctionId;
    const name = this.newPortName.trim();

    if (functionId && name.length > 0) {
      this.ruleService.addFunctionPort(functionId, direction, this.newPortType, name);
      this.newPortName = "";
    }
  }

  get portDirections() {
    return FunctionPortDirection;
  }

  OpenFunction(functionId?: string) {
    this.CancelConnection();
    this.ruleService.OpenFunction(functionId);
  }

  /** The canvas has no fixed size - it is as big as it needs to be to hold every box, plus room to drag further. */
  get canvasWidth(): number {
    return Math.max(this.furthestBox(pos => pos.x) + AutomatizationComponent.boxSize.width + AutomatizationComponent.canvasMargin, this.dragReach.x);
  }

  /** The page is as wide as the canvas, so the header can stay in view when it is scrolled sideways. */
  @HostBinding('style.width.px')
  get pageWidth(): number {
    return this.canvasWidth + AutomatizationComponent.pagePadding * 2;
  }

  get canvasHeight(): number {
    return Math.max(700, this.furthestBox(pos => pos.y) + AutomatizationComponent.boxSize.height + AutomatizationComponent.canvasMargin, this.dragReach.y);
  }

  private furthestBox(coordinate: (pos: { x: number, y: number }) => number): number {
    const boxes = [
      ...this.ruleService.visibleRules.map(rule => rule.drawingPos ?? { x: 0, y: 0 }),
      ...this.ruleService.visibleFunctions.map(fn => this.functionDrawingPos(fn)),
    ];

    return boxes.reduce((max, pos) => Math.max(max, coordinate(pos)), 0);
  }

  /** Functions are placed the same way as rules, the stored position is the middle of the box. */
  functionDrawingPos(fn: FunctionModel): { x: number, y: number } {
    const x = fn.x == 0 && fn.y == 0 ? 400 : fn.x;
    const y = fn.x == 0 && fn.y == 0 ? 150 : fn.y;

    return { x: x - AutomatizationComponent.boxCentre.x, y: y - AutomatizationComponent.boxCentre.y };
  }

  /**
   * Where the dragged box was dropped, as its top left corner within the canvas. Measured against the canvas, not the
   * page, so the box stays exactly where it was dropped after a reload whatever is above the canvas.
   */
  private dropPosition(event: CdkDragEnd): { x: number, y: number } {
    const box = event.source.element.nativeElement.getBoundingClientRect();
    const canvas = this.canvas!.nativeElement;
    const canvasRect = canvas.getBoundingClientRect();

    // Nothing can be placed above or left of the canvas, everything else is free.
    return {
      x: Math.max(0, Math.round(box.left - canvasRect.left - canvas.clientLeft)),
      y: Math.max(0, Math.round(box.top - canvasRect.top - canvas.clientTop)),
    };
  }

  onDragMoved(event: CdkDragMove) {
    this.lastPointer = event.pointerPosition;
    this.growCanvasToPointer();

    const edge = AutomatizationComponent.scrollEdge;
    const speed = (position: number, size: number) =>
      position < edge ? -Math.ceil((edge - position) / 3) :
      position > size - edge ? Math.ceil((position - (size - edge)) / 3) : 0;
    this.autoScrollSpeed = { x: speed(event.pointerPosition.x, window.innerWidth), y: speed(event.pointerPosition.y, window.innerHeight) };

    if ((this.autoScrollSpeed.x || this.autoScrollSpeed.y) && this.autoScrollFrame === undefined) {
      this.autoScrollFrame = requestAnimationFrame(() => this.autoScrollStep());
    }

    this.ReDrawConnections();
  }

  private growCanvasToPointer() {
    if (!this.lastPointer || !this.canvas) return;

    const canvasRect = this.canvas.nativeElement.getBoundingClientRect();
    this.dragReach = {
      x: Math.max(this.dragReach.x, this.lastPointer.x - canvasRect.left + AutomatizationComponent.canvasMargin),
      y: Math.max(this.dragReach.y, this.lastPointer.y - canvasRect.top + AutomatizationComponent.canvasMargin),
    };
  }

  /** Keeps scrolling while a dragged box is held at the edge of the window, even when the mouse does not move. */
  private autoScrollStep() {
    this.autoScrollFrame = undefined;
    if (!this.autoScrollSpeed.x && !this.autoScrollSpeed.y) return;

    window.scrollBy(this.autoScrollSpeed.x, this.autoScrollSpeed.y);
    this.growCanvasToPointer();
    this.ReDrawConnections();
    this.autoScrollFrame = requestAnimationFrame(() => this.autoScrollStep());
  }

  private stopDragging() {
    this.autoScrollSpeed = { x: 0, y: 0 };
    if (this.autoScrollFrame !== undefined) {
      cancelAnimationFrame(this.autoScrollFrame);
      this.autoScrollFrame = undefined;
    }
    this.lastPointer = undefined;
    this.dragReach = { x: 0, y: 0 };
  }

  /** Dragging the empty background with the mouse moves the view. Touch scrolls natively. */
  onCanvasPointerDown(event: PointerEvent) {
    if (event.pointerType !== 'mouse' || event.button !== 0 || event.target !== event.currentTarget) return;

    this.panFrom = { x: event.clientX, y: event.clientY };
    (event.currentTarget as HTMLElement).setPointerCapture(event.pointerId);
    event.preventDefault();
  }

  onCanvasPointerMove(event: PointerEvent) {
    if (!this.panFrom) return;

    window.scrollBy(this.panFrom.x - event.clientX, this.panFrom.y - event.clientY);
    this.panFrom = { x: event.clientX, y: event.clientY };
  }

  onCanvasPointerUp() {
    this.panFrom = undefined;
  }

  get panning(): boolean {
    return this.panFrom !== undefined;
  }

  handleButtonGenerated(event: { elementRef: MatButton; id: string }): void {
    this.references = this.references.filter(reference => reference.id != event.id);
    this.references.push(new ButtonReferences(event.elementRef, event.id));
  }

  dragEnd(event: CdkDragEnd, rule: AutomatizationBase) {
    const position = this.dropPosition(event);
    this.stopDragging();
    // The box is moved to the drop position itself instead of keeping the drag offset on top of the old one.
    rule.drawingPos = position;
    rule.dragPosition = { x: position.x + AutomatizationComponent.boxCentre.x, y: position.y + AutomatizationComponent.boxCentre.y };
    event.source.reset();
    this.MoveComponent();
    let apiModel: EditRule = { id: rule.id, x: rule.dragPosition.x, y: rule.dragPosition.y, behaviourId: rule.behaviorId, parameters: rule.getParametersJSon() };
    this.be.saveRule(apiModel);

    setTimeout(() => {
      this.MoveComponent();
    }, 50);
  }

  functionDragEnd(event: CdkDragEnd, fn: FunctionModel) {
    const position = this.dropPosition(event);
    this.stopDragging();
    fn.x = position.x + AutomatizationComponent.boxCentre.x;
    fn.y = position.y + AutomatizationComponent.boxCentre.y;
    event.source.reset();
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

  AddCustomInput() {
    const name = this.newInputName.trim();
    if (name.length == 0) return;

    // Only once it exists on the server is there something to load.
    this.be.createActuator({ actuatorName: name, functionId: this.ruleService.currentFunctionId })
      .subscribe(() => this.ruleService.LoadAllRules());
    this.newInputName = "";
  }

  ngOnDestroy(): void {
    this.stopDragging();
    this.lines.forEach(line => line.remove());
    this.ruleService.levelChanged = undefined;
    window.removeEventListener('resize', this.onResize);
  }
}
