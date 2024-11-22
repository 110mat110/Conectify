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
  SetValueRule?: SetValueRule;
  DecisionRule?: DecisionRule;
  AndRuleX?: AndRule;
  readonly dialogRef = inject(MatDialogRef<AutomatizationEditComponent>);

  constructor(public ruleService: RuleProviderService, public be: BefetchAutomatizationService, @Inject(MAT_DIALOG_DATA) public data: {rule: AutomatizationBase}) { };

  ngOnInit(): void {
    if (this.data.rule) {
      this.rule = this.data.rule;
    }

    if(this.rule instanceof ValueInitRule){
      this.ValueInitRule = this.rule;
    }
    if(this.rule instanceof ChangeDestinationRule){
      this.ChangeDestinationRule = this.rule;
    }

    if(this.rule instanceof UserInputRule){
      this.UserInputRule = this.rule;
    }

    if(this.rule instanceof SetTimeRule){
      this.SetTimeRule = this.rule;
    }

    if(this.rule instanceof SetValueRule){
      this.SetValueRule = this.rule;
    }

    if(this.rule instanceof DecisionRule){
      this.DecisionRule = this.rule;
    }

    if(this.rule instanceof AndRule){
      this.AndRuleX = this.rule;
    }
  }

  public AddInput() {
    if (this.rule)
      this.ruleService.CreateInputNode(this.rule.id, 2 /* value */, this.rule?.inputs.length);
  }

  public AddTrigger() {
    if (this.rule)
      this.ruleService.CreateInputNode(this.rule.id, 1 /* trigger */, this.rule?.inputs.length);
  }

  public AddParameter() {
    if (this.rule)
      this.ruleService.CreateInputNode(this.rule.id, 0 /* parameter */, this.rule?.inputs.length);
  }

  public AddOutput(){
    if (this.rule)
      this.ruleService.CreateOutputNode(this.rule.id, this.rule?.outputs.length);
  }

  public Save(){
    if(this.rule){
      let apiModel: EditRule = {id: this.rule.id, x: this.rule.dragPosition.x, y: this.rule.dragPosition.y, behaviourId: this.rule.behaviorId, parameters: this.rule.getParametersJSon() };
      this.be.saveRule(apiModel); 
      this.dialogRef.close();

    }  }

  public Close(){
    this.dialogRef.close();

  }
}
