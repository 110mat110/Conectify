import { Injectable } from '@angular/core';
import { InputBareType } from 'src/models/InputBareValue';
import { Sensor } from 'src/models/sensor';

@Injectable({
  providedIn: 'root'
})
export class OutputCreatorService {

  //sourceId: string = "3dc216b6-ac02-4bfe-abb7-e37c6df90af0";
  private thingId : string ="ea3ebbbd-87da-427a-b67a-1e7ee06577c4";
  private sensorId : string="c4680416-0267-4ae4-b78a-2bad5212a9e9";
  constructor() { }



  createBaseAction(targetId: string, number: number | null, stringValue: string, unit: string) : InputBareType{
    let unixTimeUtc = Math.floor(Date.now() / 1000);

    return {
      sourceId : this.sensorId,
      type : "Action",
      destinationId : targetId,
      timeCreated : unixTimeUtc,
      numericValue : number,
      stringValue : stringValue,
      unit : unit,
      name : "Angular APP"
    }
  }

  createSensor(): Sensor{
    return {
      id: this.sensorId,
      sensorName: "Angular web input",
      metadata: [],
      sourceThingId:"",
      sourceThing: {
        id:this.thingId,
        iPAdress: "192.168.2.128",
        metadata:[],
        positionId:"",
        thingName:"Angular web app",
        macAdress:"xx",
        position:{
          description:"Web in cloud",
          lat:0,
          long:0,
        }
      }
      
    }
  
  }
}
