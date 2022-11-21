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

@Component({
  selector: 'app-actuator-cube',
  templateUrl: './actuator-cube.component.html',
  styleUrls: ['./actuator-cube.component.css']
})
export class ActuatorCubeComponent implements OnInit {

  @Input() actuatorId?: {id: string, visible: boolean};
  public actuator?: Actuator;
  public device?: Device;
  public latestVal?: BaseInputType;
  iotype: IOType = IOType.Linear;
  stringvalue: string = "";
  numericvalue: number | undefined = undefined;
  metadatas: Metadata[] = [];
  maxValue: number = 0;
  minValue: number = 0;
  metadataValue: number = 0;

  constructor(public messenger: MessagesService, private websocketService: WebsocketService, private be: BEFetcherService, public overlay: Overlay, public viewContainerRef: ViewContainerRef, private output: OutputCreatorService) { }

  ngOnInit(): void {
    this.refreshActualStatus();
    this.determineType();
    this.websocketService.receivedMessages.subscribe(msg => {
      this.messenger.addMessage("actuator cube has value");
      this.HandleIncomingValue(msg);
    });
  }

  HandleIncomingValue(msg: any) : void{
    var id = msg.sourceId;
    if(id && this.actuator?.sensorId && id == this.actuator?.sensorId){
      this.messenger.addMessage("Got value from ws:");
      this.latestVal = msg;
    }
  }

  refreshActualStatus() {
    if (this.actuatorId) {
      this.be.getActuatorDetail(this.actuatorId.id).subscribe(x => {
        this.actuator = x;
        this.be.getLatestSensorValue(this.actuator.sensorId).subscribe(x => this.latestVal = x);
        this.be.getActuatorMetadatas(this.actuator.id).subscribe(x => {
          this.metadatas = x;
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

  determineType(): void {
    if (this.metadatas) {

    }
  }

  cubeClick() {
    this.refreshActualStatus();
  }

  onButtonClick() {
    this.sendn("", this.maxValue);
  }

  offbuttonClick() {
    this.sendn("", this.minValue);
  }

  triggerButtonClick(){
    this.sendn("", this.metadataValue);
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
}

