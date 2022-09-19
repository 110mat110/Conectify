import { Injectable } from '@angular/core';
import { WebsocketAction } from 'src/models/InputBareValue';
import { Sensor } from 'src/models/sensor';
import { Device } from 'src/models/thing';

@Injectable({
  providedIn: 'root'
})
export class OutputCreatorService {

  private deviceId : string ="";
  private sensorId : string="";
  constructor() { }



  createBaseAction(targetId: string, number: number | null, stringValue: string, unit: string) : WebsocketAction{
    let dateTime = new Date()

    return {
      sourceId : this.sensorId,
      type : "Action",
      destinationId : targetId,
      timeCreated : dateTime.getTime(),
      numericValue : number,
      stringValue : stringValue,
      unit : unit,
      name : "Angular APP"
    }
  }

  createDevice(id: string): Device{
    this.deviceId = id;
    return{
      id: this.deviceId,
      macAdress: "xxx",
      iPAdress: "xxx",
      name: "Angular web",
      positionId: null,
      position: null,
    }
  }

  createSensor(id: string): Sensor{
    this.sensorId = id;
    return {
      id: this.sensorId,
      name: "Angular web input",
      metadata: [],
      sourceDeviceId: this.deviceId,
    }
  
  }
}
