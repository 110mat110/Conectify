import { Overlay } from '@angular/cdk/overlay';
import { Component, Input, OnInit, Output, ViewContainerRef, EventEmitter } from '@angular/core';
import { Actuator } from 'src/models/actuator';
import { BaseInputType } from 'src/models/extendedValue';
import { IOType } from 'src/models/IOType';
import { Metadata } from 'src/models/metadata';
import { Device } from 'src/models/thing';
import { BEFetcherService } from '../befetcher.service';
import { MessagesService } from '../messages.service';
import { OutputCreatorService } from '../output-creator.service';
import { WebsocketService } from '../websocket.service';
import { DashboardParams } from 'src/models/Dashboard/DashboardParams';
import { Router } from '@angular/router';


@Component({
  selector: 'app-actuator-cube',
  templateUrl: './actuator-cube.component.html',
  styleUrls: ['./actuator-cube.component.css']
})
export class ActuatorCubeComponent implements OnInit {

  @Input() actuatorId?: {id: string, visible: boolean};
  @Input() params?: DashboardParams;
  public actuator?: Actuator;
  public device?: Device;
  public latestVal?: BaseInputType;
  public showName?: string = undefined;
  iotype: IOType = IOType.Undefined;
  stringvalue: string = "";
  numericvalue: number | undefined = undefined;
  metadatas: Metadata[] = [];
  maxValue: number = 0;
  minValue: number = 0;
  metadataValue: number = 0;

  constructor(public messenger: MessagesService, private websocketService: WebsocketService, private be: BEFetcherService, public overlay: Overlay, public viewContainerRef: ViewContainerRef, private output: OutputCreatorService, private router: Router  ){}

  ngOnInit(): void {
    this.refreshActualStatus();
  }

  HandleIncomingValue(msg: any) : void{
    var id = msg.sourceId;
    if(id && this.actuator?.sensorId && id == this.actuator?.sensorId){
      this.messenger.addMessage("Got value from ws:");
      this.setLatestVal(msg, "websocket");
    }
  }

  setLatestVal(val: BaseInputType, soruce: string){
    this.latestVal = val;
    console.warn(val.numericValue + " from " + soruce);
  }

  refreshActualStatus() {
    if (this.actuatorId) {
      this.be.getActuatorDetail(this.actuatorId.id).subscribe(x => {
        this.actuator = x;
        this.websocketService.receivedMessages.subscribe(msg => {
          this.HandleIncomingValue(msg);
        });
        if(!this.latestVal) this.be.getLatestSensorValue(this.actuator.sensorId).subscribe(x => this.setLatestVal(x, "latestSensorValue"));
        this.be.getActuatorMetadatas(this.actuator.id).subscribe(x => {
          this.metadatas = x;
          this.processMetadata();
        });
        if (this.actuator.sourceDeviceId) {
          this.be.getDevice(this.actuator.sourceDeviceId).subscribe(x => this.device = x);
        }
      });
      if (this.latestVal)
        this.stringvalue = this.latestVal?.stringValue;
      this.numericvalue = this.latestVal?.numericValue;
    }
  }

  cubeClick() {
    this.refreshActualStatus();
  }

  onButtonClick() {
    this.sendn(this.latestVal?.stringValue ?? this.stringvalue, this.maxValue);
  }

  offbuttonClick() {
    this.sendn(this.latestVal?.stringValue ?? this.stringvalue, this.minValue);
  }

  onSliderChange(value: string) {
    this.numericvalue = Number(value);
    this.sendn(this.stringvalue, Number(value));
  }

  onCCT(value: string){
    this.sendn(value, this.latestVal?.numericValue?? 0);
  }

  triggerButtonClick(){
    this.sendn("", this.metadataValue);
  }

  onColorChange(stringvalue: string){
    this.stringvalue = stringvalue;
    this.sendn(this.stringvalue, this.maxValue);
  }

  sendn(stringValue: string, numericValue: number): void {
    if (this.actuator) {
      let action = this.output.createBaseAction(this.actuator?.id, numericValue, stringValue, "");
      this.websocketService.SendMessage(action);
    }
  }

  send(stringvalue: string, numericValue: string): void {
    var number = Number(numericValue);
    this.sendn(stringvalue, number);
  }

  SourceClick(){
    this.router.navigate(['/device/'+ this.actuator?.sourceDeviceId])
  }

  processMetadata() {
    var typeMetadata = this.metadatas.find(x => x.name === "IOType");
    if (typeMetadata) {
      this.iotype = IOType[typeMetadata.stringValue as keyof typeof IOType];
      this.minValue = typeMetadata.minVal;
      this.maxValue = typeMetadata.maxVal;
      this.metadataValue = typeMetadata.numericValue;
    }
    var visibilityMetadata = this.metadatas.find(x => x.name === "Visible");
    if(visibilityMetadata && this.actuatorId){
      this.actuatorId.visible = visibilityMetadata.numericValue > 0;
    }
    var typeMetadata = this.metadatas.find(x => x.name === "IOType");
    if (typeMetadata) {
      this.iotype = IOType[typeMetadata.stringValue as keyof typeof IOType];
      this.minValue = typeMetadata.minVal;
      this.maxValue = typeMetadata.maxVal;
      this.metadataValue = typeMetadata.numericValue;
    }

    var nameMetadata = this.metadatas.find(x => x.name === "Name");
    if (nameMetadata) {
      this.showName = nameMetadata.stringValue;
    }
  }
}

