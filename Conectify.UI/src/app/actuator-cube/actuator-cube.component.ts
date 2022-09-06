import { Overlay } from '@angular/cdk/overlay';
import { Component, Input, OnInit, Output, ViewContainerRef, EventEmitter } from '@angular/core';
import { Actuator } from 'src/models/actuator';
import { BaseInputType } from 'src/models/extendedValue';
import { IOType } from 'src/models/IOType';
import { Metadata } from 'src/models/metadata';
import { BEFetcherService } from '../befetcher.service';
import { MessagesService } from '../messages.service';
import { OutputCreatorService } from '../output-creator.service';

@Component({
  selector: 'app-actuator-cube',
  templateUrl: './actuator-cube.component.html',
  styleUrls: ['./actuator-cube.component.css']
})
export class ActuatorCubeComponent implements OnInit {

  @Input() actuator?: Actuator;
  public latestVal?: BaseInputType;
  iotype: IOType = IOType.Linear;
  stringvalue: string = "";
  numericvalue: number | undefined = undefined;
  metadatas: Metadata[] = [];
  maxValue: number = 0;
  minValue: number = 0;

  constructor(public messenger: MessagesService, private be: BEFetcherService,  public overlay: Overlay, public viewContainerRef: ViewContainerRef, private output: OutputCreatorService) { }

  ngOnInit(): void {
    this.refreshActualStatus();
    this.determineType();
  }

  refreshActualStatus(){
    if(this.actuator){
      this.be.getLatestSensorValue(this.actuator.sensorId).subscribe(x => this.latestVal = x.entity);
      this.metadatas = this.actuator.metadata;
      if(this.actuator.sourceThing)
        this.metadatas.concat(this.actuator.sourceThing.metadata);
      if(this.latestVal)
        this.stringvalue = this.latestVal?.stringValue;
        this.numericvalue = this.latestVal?.numericValue;
    }
  }

  determineType(): void {
    if(this.actuator){
      this.messenger.addMessage("Determining type");
      var typeMetadata = this.actuator.metadata.find(x => x.name === "IOType");
      if(typeMetadata){
        this.iotype = IOType[typeMetadata.stringValue as keyof typeof IOType];
        this.minValue = typeMetadata.minVal;
        this.maxValue = typeMetadata.maxVal;
      }
    }
  }

  cubeClick(){
    this.refreshActualStatus();
  }

  onbuttonClick(){
    this.sendn("", this.maxValue);
  }

  offbuttonClick(){
    this.sendn("", this.minValue);
  }

  sendn(stringValue: string, numericValue: number): void{
    if(this.actuator){
      var output = this.output.createBaseAction(this.actuator?.id, numericValue, stringValue, "");
      this.messenger.addMessage("Wanna send " + JSON.stringify(output));
      this.be.postNewValue(output);
      }
  }

  send(stringvalue: string, numericValue: string): void{
    var number = Number(numericValue);
    this.sendn(stringvalue, number);
  }
}

