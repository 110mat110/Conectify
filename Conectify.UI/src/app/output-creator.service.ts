import { Injectable } from '@angular/core';
import { WebsocketAction } from 'src/models/InputBareValue';
import { ApiMetadata, ApiMetadataConnector } from 'src/models/metadata';
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

  createDevice(id: string, usernName: string): Device{
    this.deviceId = id;
    return{
      id: id,
      macAdress: "",
      ipAdress: "",
      name: "Angular@" + usernName,
      state: 0,
      metadata: []
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

  createMetadataToHideThis(metadataId: string) : ApiMetadataConnector{
    return{
      name: "Visible",
      deviceId: this.sensorId,
      numericValue: 0,
      stringValue: "automatic",
      unit: "",
      minVal: 0,
      maxVal: 1,
      metadataId: metadataId,
      typeValue: 0
    }
  }
}
