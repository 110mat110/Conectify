import { Component, OnInit } from '@angular/core';
import { UntypedFormControl } from '@angular/forms';
import { MatSelectionListChange } from '@angular/material/list';
import { delay } from 'rxjs/operators';
import { ApiMetadata, Metadata } from 'src/models/metadata';
import { DeviceSelector } from 'src/models/thing';
import { BEFetcherService } from '../befetcher.service';

@Component({
  selector: 'app-metadata',
  templateUrl: './metadata.component.html',
  styleUrls: ['./metadata.component.css']
})
export class MetadataComponent implements OnInit {

  formControlObj: UntypedFormControl = new UntypedFormControl();
  supportedDevices: DeviceSelector[] = [];
  selectedDeviceId?: string;
  selectedMetadata?: Metadata;
  metadatas: Metadata[] = [];
  avaliableMetadata: ApiMetadata[] = [];
  exclusive: boolean = false;
  constructor(private beFetcher: BEFetcherService) { }

  ngOnInit(): void {


      this.beFetcher.getAllActuators().subscribe(x => x.forEach(device => {
        this.supportedDevices.push({ name: "Act: " + device.name, id: device.id, type: 1 });
      }));
      this.beFetcher.getAllSensors().subscribe(x => x.forEach(device => {
        this.supportedDevices.push({ name: "Sensor: " + device.name, id: device.id, type: 2 });
      }));
          this.beFetcher.getAllDevices().subscribe(devices => devices.forEach( device =>{
              this.supportedDevices.push({ name: "Device: " + device.name, id: device.id, type: 3 });

    }   ));
    this.reloadMetadata();
  }

  reloadMetadata() {
    this.beFetcher.getAllMetadata().subscribe(x => {
      this.avaliableMetadata = x;
      this.formControlObj = new UntypedFormControl(this.avaliableMetadata);
      this.formControlObj?.setValue(this.avaliableMetadata[0].id);
    });
  }

  public selChange(e: MatSelectionListChange) {
    let selection = e.options[0].value;
    this.selectedMetadata = this.metadatas.find(x => x.metadataId = selection);
    this.formControlObj?.setValue(this.selectedMetadata?.metadataId);
  }

  selectDevice() {
    if (this.selectedDeviceId) {
      let selectedDevice = this.supportedDevices.find(x => x.id == this.selectedDeviceId);
      if (selectedDevice) {
        if (selectedDevice.type === 1) {
          this.beFetcher.getActuatorMetadatas(selectedDevice.id).subscribe(x => this.metadatas = x);
        }
        if (selectedDevice.type === 2) {
          this.beFetcher.getSensorMetadatas(selectedDevice.id).subscribe(x => this.metadatas = x);
        }
        if (selectedDevice.type === 3) {
          this.beFetcher.getDeviceMetadata(selectedDevice.id).subscribe(x => this.metadatas = x);
        }
      }
    }
  }

  compareCategoryObjects(object1: any, object2: any) {
    return object1 && object2 && object1 === object2;
  }

  updateMetadata(numericValue: string, stringValue: string, minVal: string, maxVal: string) {
    let metadataId = this.formControlObj?.value;
    console.log(metadataId);
    let selectedMetadata = this.avaliableMetadata.find(x => x.id == metadataId);
    let device = this.supportedDevices.find(x => x.id == this.selectedDeviceId);

    if (selectedMetadata && device) {
      if (device.type == 1) {
        this.beFetcher.postActuatorMetadata({ name: selectedMetadata.name, metadataId: selectedMetadata.id, numericValue: Number(numericValue), stringValue: stringValue, minVal: Number(minVal), maxVal: Number(maxVal), unit: "", deviceId: device.id, typeValue: 0 })
      }
      if (device.type == 2) {
        this.beFetcher.postSensorMetadata({ name: selectedMetadata.name, metadataId: selectedMetadata.id, numericValue: Number(numericValue), stringValue: stringValue, minVal: Number(minVal), maxVal: Number(maxVal), unit: "", deviceId: device.id, typeValue: 0 })
      }
      if (device.type == 3) {
        this.beFetcher.postDeviceMetadata({ name: selectedMetadata.name, metadataId: selectedMetadata.id, numericValue: Number(numericValue), stringValue: stringValue, minVal: Number(minVal), maxVal: Number(maxVal), unit: "", deviceId: device.id, typeValue: 0 })
      }
    }
    setTimeout(() => { this.selectDevice() }, 1000)
    this.selectDevice();
  }

  createMetadata(name: string) {
    this.beFetcher.postMetadata({ id: "00000000-0000-0000-0000-000000000000", name: name, exclusive: this.exclusive });
    setTimeout(() => { this.reloadMetadata() }, 1000)
  }
}
