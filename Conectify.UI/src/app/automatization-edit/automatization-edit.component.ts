import { Component, inject, Inject, Input } from '@angular/core';
import { RuleProviderService } from '../rule-provider.service';
import { AutomatizationBase } from 'src/models/automatizationComponent';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { SetTimeRule } from 'src/models/Automatization/SetTimeRule';
import { SetValueRule } from 'src/models/Automatization/SetValueRule';
import { DecisionRule } from 'src/models/Automatization/DecisionRule';
import { ChangeDestinationRule } from 'src/models/Automatization/ChangeDestinationRule';
import { UserInputRule } from 'src/models/Automatization/UserInputRule';
import { ValueInitRule } from 'src/models/Automatization/ValueInitRule';
import { AndRule } from 'src/models/Automatization/AndRule';
import { EditRule } from 'src/models/Automatization/EditRule';
import { BefetchAutomatizationService } from '../befetch-automatization.service';
import { BehaviourMenuItem, MinMaxDef } from 'src/models/Automatization/BehaviourMenuItem';
import { DelayRule } from 'src/models/Automatization/DelayRule';


@Component({
  selector: 'app-automatization-edit',
  templateUrl: './automatization-edit.component.html',
  styleUrl: './automatization-edit.component.css'
})
export class AutomatizationEditComponent {
  @Input() rule?: AutomatizationBase;
  ValueInitRule?: ValueInitRule;
  ChangeDestinationRule?: ChangeDestinationRule;
  UserInputRule?: UserInputRule;
  SetTimeRule?: SetTimeRule;
  DelayRule?: DelayRule;
  SetValueRule?: SetValueRule;
  DecisionRule?: DecisionRule;
  AndRuleX?: AndRule;
  Behaviour?: BehaviourMenuItem;
  readonly dialogRef = inject(MatDialogRef<AutomatizationEditComponent>);

  CanAddOutput: boolean = false;
  CanAddValue: boolean = false;
  CanAddParameter: boolean = false;
  CanAddTrigger: boolean = false;

  constructor(public ruleService: RuleProviderService, public be: BefetchAutomatizationService, @Inject(MAT_DIALOG_DATA) public data: { rule: AutomatizationBase }) { };

  ngOnInit(): void {
    if (this.data.rule) {
      this.rule = this.data.rule;
    }

    if (this.rule instanceof ValueInitRule) {
      this.ValueInitRule = this.rule;
    }
    if (this.rule instanceof ChangeDestinationRule) {
      this.ChangeDestinationRule = this.rule;
    }

    if (this.rule instanceof UserInputRule) {
      this.UserInputRule = this.rule;
    }

    if (this.rule instanceof SetTimeRule) {
      this.SetTimeRule = this.rule;
    }

    if (this.rule instanceof DelayRule) {
      this.DelayRule = this.rule;
    }

    if (this.rule instanceof SetValueRule) {
      this.SetValueRule = this.rule;
    }

    if (this.rule instanceof DecisionRule) {
      this.DecisionRule = this.rule;
    }

    if (this.rule instanceof AndRule) {
      this.AndRuleX = this.rule;
    }

    if (this.data.rule.behaviorId) {
      this.be.getBehaviour(this.data.rule.behaviorId).subscribe(x => {
        this.Behaviour = x;
        this.RecalculateCanAdd();
      }, (err) => {
        console.error(JSON.stringify(err));
      });
    }
  }

  public AddInput() {
    if (this.rule)
      this.ruleService.CreateInputNode(this.rule.id, 2 /* value */, this.rule?.inputs.length);
    this.RecalculateCanAdd();
  }

  public AddTrigger() {
    if (this.rule)
      this.ruleService.CreateInputNode(this.rule.id, 1 /* trigger */, this.rule?.inputs.length);
    this.RecalculateCanAdd();
  }

  public AddParameter() {
    if (this.rule)
      this.ruleService.CreateInputNode(this.rule.id, 0 /* parameter */, this.rule?.inputs.length);
    this.RecalculateCanAdd();
  }

  public AddOutput() {
    if (this.rule)
      this.ruleService.CreateOutputNode(this.rule.id, this.rule?.outputs.length);
      this.RecalculateCanAdd();
  }

  public Save() {
    if (this.rule) {
      let apiModel: EditRule = { id: this.rule.id, x: this.rule.dragPosition.x, y: this.rule.dragPosition.y, behaviourId: this.rule.behaviorId, parameters: this.rule.getParametersJSon() };
      this.be.saveRule(apiModel);
      this.dialogRef.close();

    }
  }

  public Close() {
    this.dialogRef.close();

  }

  private RecalculateCanAdd(){
    if(!this.rule?.outputs || !this.rule?.inputs || !this.Behaviour?.outputs || !this.Behaviour?.inputs){
      this.CanAddOutput = false;
      this.CanAddParameter = false;
      this.CanAddTrigger = false;
      this.CanAddValue = false;
      return;
    }

    this.CanAddOutput = this.Behaviour?.outputs.max > this.rule.outputs.length;
    this.CanAddParameter = this.getMaxForInput(this.Behaviour,0) > this.rule.inputs.filter(input => input.type === 0).length;
    this.CanAddTrigger = this.getMaxForInput(this.Behaviour,1) > this.rule.inputs.filter(input => input.type === 1).length;
    this.CanAddValue = this.getMaxForInput(this.Behaviour,2) > this.rule.inputs.filter(input => input.type === 2).length;

  }

  getMaxForInput(item: BehaviourMenuItem, inputNumber: number) : number
  {
    const input = item.inputs.find( (input) => {;  
      return input.item1 === inputNumber});
    return input?.item2?.max ?? -1; // Return max if found, otherwise -1
  };

  getIsArray(i: [number, MinMaxDef]) : boolean{
    var res = Array.isArray(i);

    console.log(res);

    return res;
  }
}
