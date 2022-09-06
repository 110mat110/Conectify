import { Injectable } from '@angular/core';
import { ChangeDestinationRule } from 'src/models/Automatization/ChangeDestinationRule';
import { ValueInitRule } from 'src/models/Automatization/ValueInitRule';
import { AutomatizationBase } from 'src/models/automatizationComponent';
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
    );
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

  createRule(selectedRule: string) {
    if (selectedRule === 'inputValue') {
      this.SaveComponent(new ValueInitRule(v4()));
    }

    if (selectedRule === 'changeDest') {
      this.SaveComponent(new ChangeDestinationRule(v4()));
    }
  }

  getRuleByID(id: string): AutomatizationBase | undefined {
    return this.Rules.find((x) => x.id == id);
  }
}
