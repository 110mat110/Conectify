import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ChangeDestinationRule } from 'src/models/Automatization/ChangeDestinationRule';
import { ValueInitRule } from 'src/models/Automatization/ValueInitRule';
import { AdressesService } from './adresses.service';
import { MessagesService } from './messages.service';
import { OutputCreatorService } from './output-creator.service';

@Injectable({
  providedIn: 'root'
})
export class BefetchAutomatizationService {
  httpOptions = {
    headers: new HttpHeaders({ 'Content-Type': 'application/json' })
  };


  constructor(private http: HttpClient, private adresses: AdressesService, private messenger: MessagesService, private ocs: OutputCreatorService) { }

  /*
  getActuators(): Observable<Multiresult<Actuator>>{
    return this.http.post(this.adresses.saveInputRule());
   }
   */
   saveInputRule(value: ValueInitRule){
    this.http.post(this.adresses.InputRule(), value, this.httpOptions).subscribe();
   }

   getAllInputRules(): Observable<ValueInitRule[]>{
     return this.http.get<ValueInitRule[]>(this.adresses.InputRule());
   }

   getAllChangeDestRules(): Observable<ChangeDestinationRule[]>{
    return this.http.get<ChangeDestinationRule[]>(this.adresses.saveChangeDestRule());
  }

   saveChangeDestRule(value: ChangeDestinationRule){
    this.http.post(this.adresses.saveChangeDestRule(), value, this.httpOptions).subscribe();
   }
}
