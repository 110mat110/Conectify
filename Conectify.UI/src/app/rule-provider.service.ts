import { Injectable } from '@angular/core';
import { ChangeDestinationRule } from 'src/models/Automatization/ChangeDestinationRule';
import { UserInputRule } from 'src/models/Automatization/UserInputRule';
import { ValueInitRule } from 'src/models/Automatization/ValueInitRule';
import { AutomatizationBase } from 'src/models/automatizationComponent';
import { v4 } from 'uuid';
import { AutomatizationComponent } from './automatization/automatization.component';
import { BefetchAutomatizationService } from './befetch-automatization.service';
import { SetTimeRule } from 'src/models/Automatization/SetTimeRule';
import { SetValueRule } from 'src/models/Automatization/SetValueRule';
import { DecisionRule } from 'src/models/Automatization/DecisionRule';
import { AndRule } from 'src/models/Automatization/AndRule';
import { BehaviourMenuItem } from 'src/models/Automatization/BehaviourMenuItem';
import { RuleModel } from 'src/models/Automatization/RuleModel';
import { DelayRule } from 'src/models/Automatization/DelayRule';

@Injectable({
  providedIn: 'root',
})
export class RuleProviderService {
  Rules: AutomatizationBase[] = [];

  constructor(private be: BefetchAutomatizationService) {
    this.LoadAllRules();
  }

  LoadAllRules() {
    this.Rules = [];
    this.be.getAllRules().subscribe(x => {x.forEach((rule) => {
        this.LoadRule(rule);
      })
    });
  }

  private LoadRule(rule: RuleModel) {
    var createdRule = (this.RuleFactoryCreateRuleBasedOnBehaviourId(rule.behaviourId, rule.id, rule.propertyJson, rule.name, rule.description));
    if (createdRule) {
      createdRule.dragPosition = { x: rule.x, y: rule.y };
      createdRule.outputs = [...rule.outputs].sort((a, b) => a.index - b.index);
      createdRule.inputs = [...rule.inputs].sort((a, b) => a.index - b.index);
      this.SaveComponent(createdRule);
    }
  }

  CreateInputNode(ruleId: string, type: number, index: number){
    this.be.addInputNode({ruleId: ruleId, inputType: type, index: index}).subscribe(x => {
      this.Rules.find(x => x.id == ruleId)?.inputs.push({id: x, index: index, type: type});
     });
  }

  CreateOutputNode(ruleId: string, index: number){
    this.be.addOutputNode({ruleId: ruleId, index: index}).subscribe(x => {
      this.Rules.find(x => x.id == ruleId)?.outputs.push({id: x, index: index});
     });
  }

  SaveComponent(rule: AutomatizationBase) {
    if (rule.dragPosition.x == 0 && rule.dragPosition.y == 0) {
      var ypos = 150;
      var xpos = 150;
      rule.dragPosition = { x: xpos, y: ypos };
    }
    rule.drawingPos = {x: rule.dragPosition.x -110, y: rule.dragPosition.y -75};
    this.Rules.push(rule);
  }

  createRule(selectedRuleId: string) {
    this.be.createRule(selectedRuleId).subscribe(rule=> {
      this.LoadRule(rule);
    });
  }

  getRuleByID(id: string): AutomatizationBase | undefined {
    return this.Rules.find((x) => x.id == id);
  }

  RuleFactoryCreateRuleBasedOnBehaviourId(behaviourId: string, id: string, parametersJson: string, name: string, description: string): AutomatizationBase | undefined
  {
    switch(behaviourId) {
    case "24ff4530-887b-48d1-a4fa-38cc83925797": return new ValueInitRule(id, behaviourId,parametersJson, {Name: "Unknown", SourceSensorId: "", Event: "all"}, name, description);
    case "d274c7f0-211e-413a-8689-f2543dbfc818": return new ChangeDestinationRule(id, behaviourId, parametersJson, {DestinationId:"", Name:"Unknown"}, name , description);
    case "24ff4530-887b-48d1-a4fa-38cc83925798": return new UserInputRule(id, behaviourId, parametersJson, {SourceActuatorId: "", Name: "Unknown"}, name, description);
    case "3dff4530-887b-48d1-a4fa-38cc8392469a": return new SetTimeRule(id, behaviourId, parametersJson, {TimeSet: "", Name: "Unknown", Days: "Mo,Tu,We,Th,Fr,Sa,Su"}, name, description);
    case "8c173ffc-7243-4675-9a0d-28c2ce19a18f": return new SetValueRule(id, behaviourId, parametersJson, {NumericValue:-1, StringValue:"", Unit:""}, name, description);
    case "62d50548-fff0-44c4-8bf3-b592042b1c2b": return new DecisionRule(id, behaviourId, parametersJson, {Rule:"="}, name, description);
    case "28ff4530-887b-48d1-a4fa-38dc839257a4": return new AndRule(id, behaviourId, parametersJson, {}, name, description);
    case "768fe726-caff-4120-a7f1-3d4c3c6817ac": return new DelayRule(id, behaviourId, parametersJson, {Delay: "00:00:10"}, name, description);
    default: return;
    }
  }
}
