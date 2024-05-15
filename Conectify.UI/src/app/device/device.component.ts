import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { BEFetcherService } from '../befetcher.service';
import { Device } from 'src/models/thing';
import { Actuator } from 'src/models/actuator';
import { Sensor } from 'src/models/sensor';
import { ApiMetadata, Metadata } from 'src/models/metadata';
import { MatListOption, MatSelectionListChange } from '@angular/material/list';
import { FormControl } from '@angular/forms';

@Component({
  selector: 'app-device',
  templateUrl: './device.component.html',
  styleUrls: ['./device.component.css']
})
export class DeviceComponent implements OnInit {

  public id: string | null = "";
  public device: Device | undefined = undefined;
  public actuators: Actuator[] = [];
  public actuatorCubes: {id: string, visible: boolean}[] = [];
  public actuatorWithMetadata: {actuator: Actuator, metadatas: Metadata[]}[] = [];
  public sensorWithMetadata: {sensor: Sensor, metadatas: Metadata[]}[] = []
  public sensorCubes: {id: string, visible: boolean}[] = [];
  public sensors: Sensor[] = [];
  public metadatas: Metadata[] = [];
  selectedMetadata?: Metadata;
  avaliableMetadata: ApiMetadata[] = [];
  columnNum: number = 4;
  tileSize: number = 400;
  @ViewChild('theContainer') theContainer: any;
  @ViewChild('sensorMetadatas') sensorMetadatas: any;
  @ViewChild('actuatorMetadatas') actuatorMetadatas: any;


  formControlObj: FormControl = new FormControl();


  constructor(private route: ActivatedRoute, private be: BEFetcherService) { }

  ngOnInit(): void {
    this.id = this.route.snapshot.paramMap.get('id');
    if(this.id == null){
      return;
    }
    this.be.getDevice(this.id).subscribe(
      device => this.device = device
    )

    this.be.getAllActuatorsForDevice(this.id).subscribe(
      actuators => {
        this.actuators = actuators;
        actuators.forEach(element => {
          this.actuatorCubes.push({id: element.id, visible: true})
          this.be.getActuatorMetadatas(element.id).subscribe(
            metadatas =>{
               this.actuatorWithMetadata.push({actuator:element, metadatas: metadatas})
            }
          )
        });
      }
    )

    this.be.getAllSensorsForDevice(this.id).subscribe(
      sensors => {this.sensors = sensors;
        sensors.forEach(element => {
          this.sensorCubes.push({id: element.id, visible: true})
          this.be.getSensorMetadatas(element.id).subscribe(
            metadatas =>{
               this.sensorWithMetadata.push({sensor:element, metadatas: metadatas})
            }
          )
          
        });
      }
    )

    this.be.getDeviceMetadata(this.id).subscribe(
      metadatas => this.metadatas = metadatas
    )

    this.be.getAllMetadata().subscribe(x => {
      this.avaliableMetadata = x;
      this.formControlObj = new FormControl(this.avaliableMetadata);
      this.formControlObj?.setValue(this.avaliableMetadata[0].id);
    });

    let width = this.theContainer.nativeElement.offsetWidth;
    this.columnNum = Math.trunc(width/this.tileSize);
  }
  updateMetadata(numericValue: string, stringValue: string, minVal: string, maxVal: string){
    this.actuatorMetadatas.selectedOptions.selected.forEach((element: {actuator: {id: string} }) => {
    console.warn(element.actuator.id);
   }); 
   this.sensorMetadatas.selectedOptions.selected.map((o: MatListOption) => o.value).forEach((element: {sensor: {id: string} }) => {
    console.warn(element.sensor.id);
   }); 
  }

  listSelectionChange(e: MatSelectionListChange) {
    console.log(e.source.selectedOptions.selected.map((o: MatListOption) => o.value));
  }

  deleteMetadata(id: string){
    this.be.deleteMetadata(id);
  }

  public selChange(e: MatSelectionListChange) {
    let selection = e.options[0].value;
    this.selectedMetadata = this.metadatas.find(x => x.metadataId = selection);
    this.formControlObj?.setValue(this.selectedMetadata?.metadataId);
}
compareCategoryObjects(object1: any, object2: any) {
  return object1 && object2 && object1 === object2;
}
}
