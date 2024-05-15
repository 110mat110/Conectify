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
import { ApiMetadata, ApiMetadataConnector, Metadata } from 'src/models/metadata';
import { AddDashboardApi, EditDashboardApi } from 'src/models/Dashboard/addDashboardApi';
import { DashboardApi } from 'src/models/Dashboard/DashboardApi';
import { AddDashboardDeviceApi } from 'src/models/Dashboard/DashboardDevice';
import { EditDashboardDeviceApi } from 'src/models/Dashboard/EditDashboardDeviceApi';

@Injectable({
  providedIn: 'root'
})
export class BEFetcherService {
  httpOptions = {
    headers: new HttpHeaders({ 'Content-Type': 'application/json' })
  };

  isResigtered: boolean = false;

  constructor(private http: HttpClient, private adresses: AdressesService, private messenger: MessagesService, private ocs: OutputCreatorService) { }

  getAllActuators(): Observable<Actuator[]> {
    return this.http.get<Actuator[]>(this.adresses.getAllActuatorsDetails());
  }
  getAllActuatorsForDevice(deviceId: string): Observable<Actuator[]> {
    return this.http.get<Actuator[]>(this.adresses.getAllActuatorsForDevice(deviceId));
  }

  getActiveActuatorsIds(): Observable<string[]> {
    return this.http.get<string[]>(this.adresses.getActiveActuatorsIds());
  }
  getActuatorMetadatas(id: string): Observable<Metadata[]> {
    return this.http.get<Metadata[]>(this.adresses.getActuatorMetdatas(id));
  }
  postActuatorMetadata(metadata: ApiMetadataConnector): void {
    this.http.post(this.adresses.postActuatorMetadata(), metadata, this.httpOptions).subscribe();
    this.messenger.addMessage("Sending metadata {" + JSON.stringify(metadata) + "} to adress: " + this.adresses.postActuatorMetadata());
  }

  postSensorMetadata(metadata: ApiMetadataConnector): void {
    this.http.post(this.adresses.postSensorMetadata(), metadata, this.httpOptions).subscribe();
    this.messenger.addMessage("Sending metadata {" + JSON.stringify(metadata) + "} to adress: " + this.adresses.postSensorMetadata());
  }
  getSensorMetadatas(id: string): Observable<Metadata[]> {
    return this.http.get<Metadata[]>(this.adresses.getSensorMetdatas(id));
  }
  getAllSensors(): Observable<Sensor[]> {
    return this.http.get<Sensor[]>(this.adresses.getAllSensorsDetails());
  }

  getAllSensorsForDevice(deviceId: string): Observable<Sensor[]> {
    return this.http.get<Sensor[]>(this.adresses.getAllSensorsForDevice(deviceId));
  }

  getActiveSensors(): Observable<string[]> {
    return this.http.get<string[]>(this.adresses.getActiveSensorIds());
  }

  getSensorDetail(id: string): Observable<Sensor> {
    return this.http.get<Sensor>(this.adresses.getSensorDetail(id));
  }

  getActuatorDetail(id: string): Observable<Actuator> {
    return this.http.get<Actuator>(this.adresses.getActuatorDetail(id));
  }

  getDevice(id: string): Observable<Device> {
    return this.http.get<Device>(this.adresses.getDeviceDetail(id));
  }

  getSensorValues(id: string): Observable<BaseInputType[]> {
    //this.messenger.addMessage("Requesting values: "+ this.adresses.getSensorValues(id));
    return this.http.get<BaseInputType[]>(this.adresses.getSensorValues(id));
  }

  getLatestSensorValue(id: string): Observable<BaseInputType> {
    //this.messenger.addMessage("Requesting latest for sensor: "+ this.adresses.getLatestSensorValue(id));
    return this.http.get<BaseInputType>(this.adresses.getLatestSensorValue(id));
  }

  postSensor(value: Sensor): void {
    this.http.post(this.adresses.postSensor(), value, this.httpOptions).subscribe();
    this.messenger.addMessage("Registered sensor {" + JSON.stringify(value) + "} to adress: " + this.adresses.postSensor());
  }

  postDevice(value: Device): Observable<Object> {
    this.messenger.addMessage("Registered device {" + JSON.stringify(value) + "} to adress: " + this.adresses.postDevice());
    return this.http.post(this.adresses.postDevice(), value, this.httpOptions)
  }

  getAllMetadata(): Observable<ApiMetadata[]> {
    return this.http.get<ApiMetadata[]>(this.adresses.getAllMetadata());
  }

  getDeviceMetadata(deviceId: string): Observable<Metadata[]> {
    return this.http.get<Metadata[]>(this.adresses.getDeviceMetdatas(deviceId));
  }

  postMetadata(metadata: ApiMetadata) {
    this.http.post(this.adresses.postMetadata(), metadata, this.httpOptions).subscribe();
    this.messenger.addMessage("Creating new metadata {" + JSON.stringify(metadata) + "} to adress: " + this.adresses.postMetadata());
  }

  getUserId(userMail: string): Observable<string> {
    return this.http.get<string>(this.adresses.getUserId(userMail));
  }

  getDasboards(userId: string) : Observable<DashboardApi[]> {
    return this.http.get<DashboardApi[]>(this.adresses.getDashboards(userId));
  }

  getDashboard(dashboardId: string) : Observable<DashboardApi>{
    return this.http.get<DashboardApi>(this.adresses.getDashboard(dashboardId));
  }

  addDashboard(dashboard: AddDashboardApi) : Observable<DashboardApi>{
    return this.http.post<DashboardApi>(this.adresses.addDashboard(),dashboard, this.httpOptions);
  } 

  editDashboard(dashboardId: string, dashboard: EditDashboardApi)
  {
    this.http.put(this.adresses.getDashboard(dashboardId), dashboard, this.httpOptions).subscribe();
  }

  addDashboardDevice(dashboardId: string, device: AddDashboardDeviceApi) : Observable<string>{
    return this.http.post<string>(this.adresses.addDashboardDevice(dashboardId),device, this.httpOptions);
  }

  editDasboardDevice(dashboardId: string, device: EditDashboardDeviceApi){
    return this.http.put(this.adresses.addDashboardDevice(dashboardId),device, this.httpOptions).subscribe();
  }

  removeDashboardDevice(dashboardId: string, deviceid: string){
    return this.http.delete(this.adresses.removeDashboardDevice(dashboardId, deviceid)).subscribe();
  }

  deleteMetadata(id: string){
    return this.http.delete(this.adresses.deleteMetadata(id)).subscribe();
  }

  register(id: string, name: string): void {
    this.postDevice(this.ocs.createDevice(id, name)).subscribe(x => {
      console.error("I am inside post device sub!");
      this.http.get<any>(this.adresses.subscribeToAll(id)).subscribe();
      this.postSensor(this.ocs.createSensor(id));
      this.getAllMetadata().subscribe(x => {
        let metadataId = x.find(f => f.name == "Visible")?.id;
        if (metadataId) {
          this.postSensorMetadata(this.ocs.createMetadataToHideThis(metadataId));
        }
      })
      this.messenger.addMessage("Registered everything");
      this.isResigtered = true;
    });
  }
}
