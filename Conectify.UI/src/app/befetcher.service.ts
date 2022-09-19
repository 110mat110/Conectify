import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Sensor } from 'src/models/sensor';
import { AdressesService } from './adresses.service';
import { MessagesService } from './messages.service';
import { BaseInputType } from 'src/models/extendedValue';
import { Actuator } from 'src/models/actuator';
import { WebsocketAction } from 'src/models/InputBareValue';
import { OutputCreatorService } from './output-creator.service';
import { Device } from 'src/models/thing';

@Injectable({
  providedIn: 'root'
})
export class BEFetcherService {
  httpOptions = {
    headers: new HttpHeaders({ 'Content-Type': 'application/json' })
  };

  isResigtered: boolean = false;

  constructor(private http: HttpClient, private adresses: AdressesService, private messenger: MessagesService, private ocs: OutputCreatorService) { }

  getActuators(): Observable<Actuator[]>{
     return this.http.get<Actuator[]>(this.adresses.getAllActuatorsDetails());
    }
  getActiveActuatorsIds(): Observable<string[]>{
    return this.http.get<string[]>(this.adresses.getActiveActuatorsIds());
  }

  getSensors(): Observable<Sensor[]>{
   return this.http.get<Sensor[]>(this.adresses.getAllSensorsDetails());
  }
  getActiveSensors(): Observable<string[]>{
     return this.http.get<string[]>(this.adresses.getActiveSensorIds());
    }

    getSensorDetail(id: string): Observable<Sensor>{
      return this.http.get<Sensor>(this.adresses.getSensorDetail(id));
     }   

     getActuatorDetail(id: string): Observable<Actuator>{
      return this.http.get<Actuator>(this.adresses.getActuatorDetail(id));
     }      

  getDevice(id: string): Observable<Device>{
    return this.http.get<Device>(this.adresses.getDeviceDetail(id));
  }

  getSensorValues(id: string) : Observable<BaseInputType[]>{
    //this.messenger.addMessage("Requesting values: "+ this.adresses.getSensorValues(id));
    return this.http.get<BaseInputType[]>(this.adresses.getSensorValues(id));
  }

  getLatestSensorValue(id: string) : Observable<BaseInputType>{
    //this.messenger.addMessage("Requesting latest for sensor: "+ this.adresses.getLatestSensorValue(id));
    return this.http.get<BaseInputType>(this.adresses.getLatestSensorValue(id));
  }

  postSensor(value: Sensor): void{
    this.http.post(this.adresses.postSensor(), value, this.httpOptions).subscribe();
    this.messenger.addMessage("Registered sensor {"+ JSON.stringify(value) +"} to adress: " + this.adresses.getInputBareValue());
  }

  postDevice(value: Device): void{
    this.http.post(this.adresses.postDevice(), value, this.httpOptions).subscribe();
    this.messenger.addMessage("Registered sensor {"+ JSON.stringify(value) +"} to adress: " + this.adresses.getInputBareValue());

  }

  register(id: string) : void{
    this.postDevice(this.ocs.createDevice(id));
    this.postSensor(this.ocs.createSensor(id));
    this.isResigtered = true;
  }
}
