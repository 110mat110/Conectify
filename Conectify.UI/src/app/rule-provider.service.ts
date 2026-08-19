import { Injectable } from '@angular/core';
import { forkJoin } from 'rxjs';
import { ChangeDestinationRule } from 'src/models/Automatization/ChangeDestinationRule';
import { UserInputRule } from 'src/models/Automatization/UserInputRule';
import { ValueInitRule } from 'src/models/Automatization/ValueInitRule';
import { AutomatizationBase } from 'src/models/automatizationComponent';
import { BefetchAutomatizationService } from './befetch-automatization.service';
import { SetTimeRule } from 'src/models/Automatization/SetTimeRule';
import { SetValueRule } from 'src/models/Automatization/SetValueRule';
import { DecisionRule } from 'src/models/Automatization/DecisionRule';
import { AndRule } from 'src/models/Automatization/AndRule';
import { RuleModel } from 'src/models/Automatization/RuleModel';
import { DelayRule } from 'src/models/Automatization/DelayRule';
import { HttpCallRule } from 'src/models/Automatization/HttpCallRule';
import { FunctionModel } from 'src/models/Automatization/FunctionModel';
import { PortRule } from 'src/models/Automatization/PortRule';

@Injectable({
  providedIn: 'root',
})
export class RuleProviderService {
  Rules: AutomatizationBase[] = [];
  Functions: FunctionModel[] = [];

  /** Function the board currently shows, empty means the top level. */
  currentFunctionId?: string;

  /** Rules and functions that belong to the level the board shows. */
  visibleRules: AutomatizationBase[] = [];
  visibleFunctions: FunctionModel[] = [];

  /** Path from the top level down to the function that is open. */
  breadcrumb: FunctionModel[] = [];

  /** Called after the board content changed, so that the page can redraw its lines. */
  levelChanged?: () => void;

  constructor(private be: BefetchAutomatizationService) {
    this.LoadAllRules();
  }

  LoadAllRules() {
    forkJoin({
      rules: this.be.getAllRules(),
      functions: this.be.getAllFunctions(),
    }).subscribe(result => {
      this.Rules = [];
      result.rules.forEach(rule => this.LoadRule(rule));
      this.Functions = result.functions;

      // A function that was open and is gone now would leave the board empty with no way back.
      if (this.currentFunctionId && !this.Functions.some(x => x.id == this.currentFunctionId)) {
        this.currentFunctionId = undefined;
      }

      this.RefreshLevel();
    });
  }

  /** Opens a function, or the top level board when nothing is given. */
  OpenFunction(functionId?: string) {
    this.currentFunctionId = functionId;
    this.RefreshLevel();
  }

  get currentFunction(): FunctionModel | undefined {
    return this.Functions.find(x => x.id == this.currentFunctionId);
  }

  /** Function holding the one that is open, used by "move one level up". */
  get parentFunctionId(): string | undefined {
    return this.currentFunction?.parentFunctionId;
  }

  RefreshLevel() {
    this.visibleRules = this.Rules.filter(rule => (rule.functionId ?? undefined) == this.currentFunctionId);
    this.visibleFunctions = this.Functions.filter(fn => (fn.parentFunctionId ?? undefined) == this.currentFunctionId);
    this.breadcrumb = this.BuildBreadcrumb();

    if (this.levelChanged) {
      this.levelChanged();
    }
  }

  /** Points drawn on the current level. A line can only be drawn between two of them. */
  visiblePointIds(): Set<string> {
    const ids = new Set<string>();

    this.visibleRules.forEach(rule => {
      rule.inputs.forEach(input => ids.add(input.id));
      rule.outputs.forEach(output => ids.add(output.id));
    });

    this.visibleFunctions.forEach(fn => {
      fn.inputPorts.forEach(port => ids.add(port.pointId));
      fn.outputPorts.forEach(port => ids.add(port.pointId));
    });

    return ids;
  }

  private BuildBreadcrumb(): FunctionModel[] {
    const path: FunctionModel[] = [];
    let current = this.currentFunction;

    // A function cannot hold itself, the counter is only a guard against broken data.
    while (current && path.length < 50) {
      path.unshift(current);
      current = this.Functions.find(x => x.id == current!.parentFunctionId);
    }

    return path;
  }

  private LoadRule(rule: RuleModel) {
    var createdRule = (this.RuleFactoryCreateRuleBasedOnBehaviourId(rule.behaviourId, rule.id, rule.propertyJson, rule.name, rule.description));
    if (createdRule) {
      createdRule.dragPosition = { x: rule.x, y: rule.y };
      createdRule.functionId = rule.functionId ?? undefined;
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

  /** Creates a rule on the level the board currently shows. */
  createRule(selectedRuleId: string) {
    this.be.createRule(selectedRuleId, this.currentFunctionId).subscribe(rule => {
      this.LoadRule(rule);
      this.RefreshLevel();
    });
  }

  createFunction(name: string) {
    this.be.createFunction({ name: name, parentFunctionId: this.currentFunctionId }).subscribe(() => this.LoadAllRules());
  }

  removeFunction(functionId: string) {
    this.be.removeFunction(functionId).subscribe(() => this.LoadAllRules());
  }

  addFunctionPort(functionId: string, direction: number, type: number, name: string) {
    this.be.addFunctionPort(functionId, { direction: direction, type: type, name: name }).subscribe(() => this.LoadAllRules());
  }

  moveRuleToFunction(ruleId: string, functionId?: string) {
    this.be.moveRuleToFunction(ruleId, { functionId: functionId }).subscribe(() => this.LoadAllRules());
  }

  moveFunctionToParent(functionId: string, parentFunctionId?: string) {
    this.be.moveFunctionToParent(functionId, { functionId: parentFunctionId }).subscribe(() => this.LoadAllRules());
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
    case "dbb05c98-9112-460f-be89-7fe399cb5a58": return new HttpCallRule(id, behaviourId, parametersJson, {Http: ""}, name, description);
    // Ports carry a signal across the border of a function, their only option is the pin label.
    case "b7e6f0c2-4d1a-4a3e-9f2b-6d5c4b3a2101":
    case "b7e6f0c2-4d1a-4a3e-9f2b-6d5c4b3a2102": return new PortRule(id, behaviourId, parametersJson, {Name: name}, name, description);
    default: return;
    }
  }
}
