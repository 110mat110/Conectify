import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Sensor } from 'src/models/sensor';
import { Multiresult, SingleResult } from 'src/models/IncomingModel';
import { AdressesService } from './adresses.service';
import { MessagesService } from './messages.service';
import { BaseInputType } from 'src/models/extendedValue';
import { Actuator } from 'src/models/actuator';
import { InputBareType } from 'src/models/InputBareValue';
import { OutputCreatorService } from './output-creator.service';

@Injectable({
  providedIn: 'root'
})
export class BEFetcherService {
  httpOptions = {
    headers: new HttpHeaders({ 'Content-Type': 'application/json' })
  };

  isResigtered: boolean = false;

  constructor(private http: HttpClient, private adresses: AdressesService, private messenger: MessagesService, private ocs: OutputCreatorService) { }

  getActuators(): Observable<Multiresult<Actuator>>{
     return this.http.get<Multiresult<Actuator>>(this.adresses.getActuatorsDetails());
    }

  getSensors(): Observable<Multiresult<Sensor>>{
  //this.messenger.addMessage("Getting request from: " + this.adresses.getSensorsDetails());
   return this.http.get<Multiresult<Sensor>>(this.adresses.getSensorsDetails());
  }
  getSensorValues(id: string) : Observable<Multiresult<BaseInputType>>{
    //this.messenger.addMessage("Requesting values: "+ this.adresses.getSensorValues(id));
    return this.http.get<Multiresult<BaseInputType>>(this.adresses.getSensorValues(id));
  }

  getLatestSensorValue(id: string) : Observable<SingleResult<BaseInputType>>{
    //this.messenger.addMessage("Requesting latest for sensor: "+ this.adresses.getLatestSensorValue(id));
    return this.http.get<SingleResult<BaseInputType>>(this.adresses.getLatestSensorValue(id));
  }

  postNewValue(value: InputBareType): void{
    if(!this.isResigtered){
      this.register();
    }


    this.http.post(this.adresses.getInputBareValue(), value, this.httpOptions).subscribe();
    this.messenger.addMessage("Send value {"+ JSON.stringify(value) +"} to adress: " + this.adresses.getInputBareValue());

  }

  postSensor(value: Sensor): void{
    this.http.post(this.adresses.getInputSensor(), value, this.httpOptions).subscribe();
    this.messenger.addMessage("Registered sensor {"+ JSON.stringify(value) +"} to adress: " + this.adresses.getInputBareValue());
  }

  private register() : void{
    this.postSensor(this.ocs.createSensor());
    this.isResigtered = true;
  }
}
