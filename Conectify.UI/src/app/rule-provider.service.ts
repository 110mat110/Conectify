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
        var createdRule = (this.RuleFactoryCreateRuleBasedOnBehaviourId(rule.behaviourId, rule.id, rule.propertyJson));
        if(createdRule){
          createdRule.dragPosition = {x: rule.x, y: rule.y};
          createdRule.targets = rule.targets;
          this.SaveComponent(createdRule);
        }
      })
    });
  }

  SaveComponent(component: AutomatizationBase) {
    if (component.dragPosition.x == 0 && component.dragPosition.y == 0) {
      var ypos = 150;
      var xpos = 150;
      component.dragPosition = { x: xpos, y: ypos };
    }
    component.drawingPos = {x: component.dragPosition.x -110, y: component.dragPosition.y -75};
    this.Rules.push(component);
  }

  createRule(selectedRuleId: string) {
    this.be.createRule({x: 100, y:100, behaviourId: selectedRuleId, propertyJson:"{}"}).subscribe(x => {
        let component = this.RuleFactoryCreateRuleBasedOnBehaviourId(selectedRuleId, x, "{}");
        if(component){
          this.SaveComponent(component);
        }
    });
  }

  getRuleByID(id: string): AutomatizationBase | undefined {
    return this.Rules.find((x) => x.id == id);
  }

  RuleFactoryCreateRuleBasedOnBehaviourId(behaviourId: string, id: string, parametersJson: string): AutomatizationBase | undefined
  {
    switch(behaviourId) {
    case "24ff4530-887b-48d1-a4fa-38cc83925797": return new ValueInitRule(id, behaviourId,parametersJson, {Name: "Unknown", SourceSensorId: ""});
    case "d274c7f0-211e-413a-8689-f2543dbfc818": return new ChangeDestinationRule(id, behaviourId, parametersJson, {DestinationId:"", Name:"Unknown"} );
    case "24ff4530-887b-48d1-a4fa-38cc83925798": return new UserInputRule(id, behaviourId, parametersJson, {SourceActuatorId: "", Name: "Unknown"});
    case "3dff4530-887b-48d1-a4fa-38cc8392469a": return new SetTimeRule(id, behaviourId, parametersJson, {TimeSet: "", Name: "Unknown", Days: "Mo,Tu,We,Th,Fr,Sa,Su"});
    case "8c173ffc-7243-4675-9a0d-28c2ce19a18f": return new SetValueRule(id, behaviourId, parametersJson, {NumericValue:-1, StringValue:"", Unit:""});
    default: return;
    }
  }
}
