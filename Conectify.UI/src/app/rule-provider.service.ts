import { Injectable } from '@angular/core';
import { ChangeDestinationRule } from 'src/models/Automatization/ChangeDestinationRule';
import { UserInputRule } from 'src/models/Automatization/UserInputRule';
import { ValueInitRule } from 'src/models/Automatization/ValueInitRule';
import { AutomatizationBase, AutomatizationBaseWithTarget } from 'src/models/automatizationComponent';
import { v4 } from 'uuid';
import { AutomatizationComponent } from './automatization/automatization.component';
import { BefetchAutomatizationService } from './befetch-automatization.service';

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
          if(createdRule instanceof AutomatizationBaseWithTarget){
            createdRule.targets = rule.targets;
          }
          this.SaveComponent(createdRule);
        }
      })
    });
    /*
    this.be.getAllInputRules().subscribe((x) =>
      x.forEach((y) => {
        var rule = new ValueInitRule(y.id);
        rule.sourceId = y.sourceId;
        rule.targets = y.targets;
        console.warn(y.targets);
        console.warn(rule.targets);
        rule.dragPosition = y.dragPosition;
        this.SaveComponent(rule);
      })
    );

    this.be.getAllChangeDestRules().subscribe((x) =>
      x.forEach((y) => {
        var rule = new ChangeDestinationRule(y.id);
        console.warn(rule.id)
        rule.destinationId = y.destinationId;
        rule.targets = y.targets;
        rule.dragPosition = y.dragPosition;
        this.SaveComponent(rule);
      })
    );*/
  }

  SaveComponent(component: AutomatizationBase) {
    if (component.dragPosition.x == 0 && component.dragPosition.y == 0) {
      var ypos = 100;
      var xpos = 100;
      component.dragPosition = { x: xpos, y: ypos };
    }

    component.initialPos = component.dragPosition;
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
    case "24ff4530-887b-48d1-a4fa-38cc83925797":{
      return new ValueInitRule(id, behaviourId,parametersJson);
    }
    case "d274c7f0-211e-413a-8689-f2543dbfc818": return new ChangeDestinationRule(id, behaviourId, parametersJson );
    case "24ff4530-887b-48d1-a4fa-38cc83925798": return new UserInputRule(id, behaviourId, parametersJson);
    default: return;
    }
  }
}
