import { stringify } from '@angular/compiler/src/util';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class AdressesService {

  private baseURL: string = "http://localhost:5000/api/"
  private automatizationURL: string = "http://localhost:5010/api/"

  constructor() { }

  getActuatorsDetails(): string{
    return this.baseURL+ "actuators/all"
  }

  getSensorsDetails(): string{
    return this.baseURL + "sensors/all"
  }

  getSensorValues(id: string): string{
    return this.baseURL + "output/sensor/" + id;
  }

  getLatestSensorValue(id: string): string{
    return this.baseURL + "output/sensor/latest/" + id;
  }

  getInputBareValue(){
    return this.baseURL + "input/bareValue";
  }

  getInputSensor(){
    return this.baseURL + "input/sensor";
  }

  InputRule() : string{
    return this.automatizationURL + "input/inputValueRule"
  }

  saveChangeDestRule() : string{
    return this.automatizationURL + "input/changeDestRule"
  }
}
