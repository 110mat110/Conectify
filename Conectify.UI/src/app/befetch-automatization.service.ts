import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CreateRule } from 'src/models/Automatization/CreateRule';
import { AdressesService } from './adresses.service';
import { BehaviourMenuItem } from 'src/models/Automatization/BehaviourMenuItem';
import { AutomatizationBase } from 'src/models/automatizationComponent';
import { RuleModel } from 'src/models/Automatization/RuleModel';
import { EditRule } from 'src/models/Automatization/EditRule';
import { RuleConnection } from 'src/models/Automatization/RuleConnection';
import { CreateActuatorApi } from 'src/models/actuator';

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

  createRule(value: CreateRule): Observable<string> {
    return this.http.post<string>(this.adresses.createRule(), value, this.httpOptions);
  }

  addNewConnection(source: string, destination: string) {
    this.http.post(this.adresses.connectionChange(source, destination), {}).subscribe();
  }

  removeConnection(source: string, destination: string) {
    this.http.delete(this.adresses.connectionChange(source, destination)).subscribe();
  }

  saveRule(rule: EditRule) {
    this.http.put(this.adresses.editRule(rule.id), rule).subscribe();
  }

  createActuator(actuator: CreateActuatorApi){
    this.http.post(this.adresses.customInput(), actuator).subscribe();
  }
}
