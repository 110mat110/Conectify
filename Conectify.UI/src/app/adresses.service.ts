import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AdressesService {

  private serverURL: string = environment.serverUrl + "/api/"
  private automatizationURL: string = environment.automatizationURL + "/api/"
  private historyURL: string = environment.historyUrl + "/api/"
  private dashboardURL: string = environment.dashboardUrl + "/api/"

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

  getAllDevices(): string {
    return this.serverURL + "device/all";
  }

  postDevice():string{
    return this.serverURL + "device";
  }

  getActiveActuatorsIds(): string {
    return this.historyURL + "device/actuators"
  }

  getAllActuatorsForDevice(id: string) : string{
    return this.serverURL + "actuators/by-device/" + id;
  }

  getAllSensorsForDevice(id: string) : string{
    return this.serverURL + "sensors/by-device/" + id;
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

  getDeviceMetdatas(id: string):string{
    return this.serverURL + "device/" + id + "/metadata";
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

  createRule(behaviourId: string) : string{
    return this.automatizationURL + "rule/create/" + behaviourId
  }
  
  allRules() : string{
    return this.automatizationURL + "rule/all"
  }

  editRule(id: string){
    return this.automatizationURL + "rule/" + id;
  }

  behaviourList() : string{
    return this.automatizationURL + "behaviour/all"
  }

  connectionChange(source: string, destination: string){
    return this.automatizationURL + "rule/connection/"+source+"/"+destination;
  }

  parameterChange(source: string, destination: string){
    return this.automatizationURL + "rule/parameter/"+source+"/"+destination;
  }
  
  allConnections(){
    return this.automatizationURL + "rule/connections"
  }

  customInput(){
    return this.automatizationURL + "rule/input"
  }

  getUserId(username: string){
    return this.serverURL + "user/"+username;
  }
  getDashboards(userId: string){
    return this.dashboardURL + "dashboard/all/" + userId;
  }

  getDashboard(dashboardId: string){
    return this.dashboardURL + "dashboard/" + dashboardId;
  }

  addDashboard(){
    return this.dashboardURL + "dashboard";
  }

  addDashboardDevice(idDashboard: string){
    return this.dashboardURL + "dashboard/"+ idDashboard + "/device";
  }

  removeDashboardDevice(dashboardId: string, deviceId: string){
    return this.dashboardURL + "dashboard/"+ dashboardId + "/device/" + deviceId;
  }

  deleteMetadata(id: string){
    return this.serverURL + "metadata/" + id;
  }

  addInputNode() {
    return this.automatizationURL + "rule/addinputnode";
  }

  addOutputNode() {
    return this.automatizationURL + "rule/addoutputnode";
  }
}
