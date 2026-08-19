import { AfterViewInit, Component, EventEmitter, Input, Output, QueryList, ViewChildren } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { FunctionModel } from 'src/models/Automatization/FunctionModel';
import { AutomatizationComponent } from '../automatization/automatization.component';
import { AutomatizationFunctionEditComponent } from '../automatization-function-edit/automatization-function-edit.component';
import { RuleProviderService } from '../rule-provider.service';

/**
 * A function drawn on the board of its parent. The pins are the ports of the function, so a line
 * from a rule to a function is an ordinary connection to the rule that sits behind the port.
 */
@Component({
  selector: 'app-automatization-function',
  templateUrl: './automatization-function.component.html',
  styleUrls: ['./automatization-function.component.css']
})
export class AutomatizationFunctionComponent implements AfterViewInit {

  @Input() Function?: FunctionModel;
  @Input() Cage?: AutomatizationComponent;
  @ViewChildren('inputPortRef') inputPortRefs!: QueryList<MatButton>;
  @ViewChildren('outputPortRef') outputPortRefs!: QueryList<MatButton>;

  @Output() buttonGenerated = new EventEmitter<{ elementRef: MatButton; id: string }>();

  typeNames: { [key: number]: string } = {
    0: 'Parameter',
    1: 'Trigger',
    2: 'Value'
  };

  constructor(public ruleService: RuleProviderService, private dialog: MatDialog) { }

  ngAfterViewInit(): void {
    this.EmitPorts(this.inputPortRefs, () => this.Function?.inputPorts ?? []);
    this.EmitPorts(this.outputPortRefs, () => this.Function?.outputPorts ?? []);
  }

  private EmitPorts(refs: QueryList<MatButton>, ports: () => { pointId: string }[]): void {
    const emit = (buttons: QueryList<MatButton>) => buttons.forEach((button, index) => {
      const id = ports()[index]?.pointId;
      if (id) {
        this.buttonGenerated.emit({ elementRef: button, id });
      }
    });

    refs.changes.subscribe(emit);
    emit(refs);
  }

  TypeName(type: number): string {
    return this.typeNames[type] ?? 'Input';
  }

  IsArmed(pointId: string): boolean {
    return this.Cage?.source == pointId;
  }

  get hasArmedOutput(): boolean {
    return this.Function?.outputPorts.some(port => port.pointId == this.Cage?.source) ?? false;
  }

  SourceClick(pointId: string) {
    this.Cage?.SourceClick(pointId);
  }

  DestinationClick(pointId: string) {
    this.Cage?.DestinationClick(pointId);
  }

  Open() {
    if (this.Function) {
      this.ruleService.OpenFunction(this.Function.id);
    }
  }

  Edit() {
    if (!this.Function) {
      return;
    }

    const dialog = this.dialog.open(AutomatizationFunctionEditComponent, {
      width: '400px',
      data: { function: this.Function }
    });

    dialog.afterClosed().subscribe(() => this.ruleService.LoadAllRules());
  }

  Remove() {
    if (!this.Function) {
      return;
    }

    const inside = this.ruleService.Rules.filter(rule => rule.functionId == this.Function!.id).length;

    if (confirm(`Delete function "${this.Function.name}" with everything inside it (${inside} rule(s) and all nested functions)?`)) {
      this.ruleService.removeFunction(this.Function.id);
    }
  }
}
