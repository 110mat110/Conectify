import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AdressesService {

  private serverURL: string = environment.serverUrl + "/api/"
  private automatizationURL: string = environment.automatizationURL + "/api/"
  private historyURL: string = environment.historyUrl + "/api/"

  constructor() { }

  getActiveSensorIds(): string {
    return this.historyURL + "device/sensors"
  }

  getSensorDetail(id: string): string {
    return this.serverURL + "sensors/" + id;
  }

  postSensor():string{
    return this.serverURL + "sensors";
  }

  postSensorMetadata():string{
    return this.serverURL + "sensors/metadata";
  }

  postActuatorMetadata():string{
    return this.serverURL + "actuators/metadata";
  }

  getDeviceDetail(id: string): string {
    return this.serverURL + "device/" + id;
  }

  postDevice():string{
    return this.serverURL + "device";
  }

  getActiveActuatorsIds(): string {
    return this.historyURL + "device/actuators"
  }

  getAllActuatorsDetails(): string{
    return this.serverURL+ "actuators/all";
  }

  getActuatorDetail(id: string): string{
    return this.serverURL+ "actuators/" + id;
  }

  getActuatorMetdatas(id: string):string{
    return this.serverURL + "actuators/" + id + "/metadata";
  }

  getSensorMetdatas(id: string):string{
    return this.serverURL + "sensors/" + id + "/metadata";
  }

  getAllSensorsDetails(): string{
    return this.serverURL + "sensors/all"
  }

  getSensorValues(id: string): string{
    return this.historyURL + "data/" + id + "/values";
  }

  getLatestSensorValue(id: string): string{
    return this.historyURL + "data/" + id + "/latest";
  }

  getAllMetadata(){
    return this.serverURL + "metadata/all"
  }

  subscribeToAll(id:string){
    return this.serverURL + "subscribe/"+id+"/all"
  }

  postMetadata(){
    return this.serverURL + "metadata"
  }

  InputRule() : string{
    return this.automatizationURL + "input/inputValueRule"
  }

  saveChangeDestRule() : string{
    return this.automatizationURL + "input/changeDestRule"
  }


}
