import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, ObservableInput } from 'rxjs';
import { CreateRule } from 'src/models/Automatization/CreateRule';
import { AdressesService } from './adresses.service';
import { BehaviourMenuItem } from 'src/models/Automatization/BehaviourMenuItem';
import { AutomatizationBase } from 'src/models/automatizationComponent';
import { AddInputApiModel, AddOutputApiModel, RuleModel } from 'src/models/Automatization/RuleModel';
import { EditRule } from 'src/models/Automatization/EditRule';
import { RuleConnection } from 'src/models/Automatization/RuleConnection';
import { CreateActuatorApi } from 'src/models/actuator';
import { AddFunctionPort, CreateFunction, EditFunction, FunctionModel, MoveToFunction } from 'src/models/Automatization/FunctionModel';

@Injectable({
  providedIn: 'root'
})
export class BefetchAutomatizationService {
  httpOptions = {
    headers: new HttpHeaders({ 'Content-Type': 'application/json' })
  };


  constructor(private http: HttpClient, private adresses: AdressesService) { }
  getAllRules(): Observable<RuleModel[]>{
    return this.http.get<RuleModel[]>(this.adresses.allRules());
  }
  GetAllBehaviours(): Observable<BehaviourMenuItem[]> {
    return this.http.get<BehaviourMenuItem[]>(this.adresses.behaviourList());
  }

  GetAllConnections(): Observable<RuleConnection[]>{
    return this.http.get<RuleConnection[]>(this.adresses.allConnections());
  }

  createRule(behaviorId: string, functionId?: string): Observable<RuleModel> {
    return this.http.get<RuleModel>(this.adresses.createRule(behaviorId, functionId));
  }

  getAllFunctions(): Observable<FunctionModel[]> {
    return this.http.get<FunctionModel[]>(this.adresses.allFunctions());
  }

  createFunction(model: CreateFunction): Observable<FunctionModel> {
    return this.http.post<FunctionModel>(this.adresses.function(), model, this.httpOptions);
  }

  saveFunction(functionId: string, model: EditFunction): Observable<any> {
    return this.http.put(this.adresses.functionById(functionId), model, this.httpOptions);
  }

  removeFunction(functionId: string): Observable<any> {
    return this.http.delete(this.adresses.functionById(functionId));
  }

  addFunctionPort(functionId: string, port: AddFunctionPort): Observable<RuleModel> {
    return this.http.post<RuleModel>(this.adresses.functionPort(functionId), port, this.httpOptions);
  }

  moveRuleToFunction(ruleId: string, target: MoveToFunction): Observable<any> {
    return this.http.put(this.adresses.moveRuleToFunction(ruleId), target, this.httpOptions);
  }

  moveFunctionToParent(functionId: string, target: MoveToFunction): Observable<any> {
    return this.http.put(this.adresses.moveFunctionToParent(functionId), target, this.httpOptions);
  }

  setConnection(source: string, destination: string): Observable<any> {
    return this.http.post(this.adresses.connectionChange(source, destination), {});
  }

  addNewParameterConnection(source: string, destination: string) {
    this.http.post(this.adresses.parameterChange(source, destination), {}).subscribe();
  }

  removeParameterConnection(source: string, destination: string) {
    this.http.delete(this.adresses.parameterChange(source, destination)).subscribe();
  }

  saveRule(rule: EditRule) {
    this.http.put(this.adresses.editRule(rule.id), rule).subscribe();
  }

  createActuator(actuator: CreateActuatorApi){
    this.http.post(this.adresses.customInput(), actuator).subscribe();
  }

  addInputNode(input: AddInputApiModel): Observable<string> {
    return this.http.post<string>(this.adresses.addInputNode(), input, this.httpOptions);
  }

  addOutputNode(output: AddOutputApiModel): Observable<string> {
    return this.http.post<string>(this.adresses.addOutputNode(), output, this.httpOptions);
  }
  getBehaviour(behaviourId: string): Observable<BehaviourMenuItem>{
    return this.http.get<BehaviourMenuItem>(this.adresses.behaviour(behaviourId));
  }

  getRule(ruleId: string): Observable<RuleModel>{
    return this.http.get<RuleModel>(this.adresses.rule(ruleId))
  }
}
