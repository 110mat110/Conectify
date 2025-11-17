import { Component, ElementRef, OnInit, ViewChild, AfterViewInit, OnChanges, SimpleChanges, Input } from '@angular/core';
import { ActivatedRoute, RedirectCommand, Router } from '@angular/router';
import { BEFetcherService } from '../befetcher.service';
import { Device } from 'src/models/thing';
import { Actuator } from 'src/models/actuator';
import { Sensor } from 'src/models/sensor';
import { ApiMetadata, Metadata } from 'src/models/metadata';
import { MatListOption, MatSelectionListChange } from '@angular/material/list';
import { FormControl } from '@angular/forms';
import { fromEvent } from 'rxjs';
import { debounceTime } from 'rxjs/operators';

@Component({
  selector: 'app-device',
  templateUrl: './device.component.html',
  styleUrls: ['./device.component.css']
})
export class DeviceComponent implements OnInit {

  public id: string | null = "";
  public device: Device | undefined = undefined;
  public devices: Device[] = [];
  public actuators: Actuator[] = [];
  public actuatorCubes: { id: string, visible: boolean }[] = [];
  public actuatorWithMetadata: { actuator: Actuator, metadatas: Metadata[] }[] = [];
  public sensorWithMetadata: { sensor: Sensor, metadatas: Metadata[] }[] = []
  public sensorCubes: { id: string[], visible: boolean }[] = [];
  public sensors: Sensor[] = [];
  public metadatas: Metadata[] = [];
  public deviceSrc: string | undefined;
  selectedMetadata?: Metadata;
  avaliableMetadata: ApiMetadata[] = [];
  selectedSensors: string[] = [];
  columnNum: number = 4;
  tileSize: number = 400;
  @ViewChild('theContainer') theContainer: any;
  @ViewChild('sensorMetadatas') sensorMetadatas: any;
  @ViewChild('actuatorMetadatas') actuatorMetadatas: any;
  @ViewChild('deviceWebsite', { static: true }) deviceWebsite: any;


  formControlObj: FormControl = new FormControl();


  constructor(private route: ActivatedRoute, private be: BEFetcherService, private router: Router) { }

  ngOnInit(): void {
    this.be.getAllDevices().subscribe(
      devices => {
        this.devices = devices.sort(x => x.state);


    this.id = this.route.snapshot.paramMap.get('id');
    if (this.id == null || this.id == "" || this.id == "-1") {
      this.selectDevice(devices[0])
      return;
    }

    this.be.getDevice(this.id).subscribe(
      device => {
        this.device = device;
        this.deviceSrc = "http://" + device.ipAdress + "/";
        this.deviceWebsite.nativeElement.src = this.deviceSrc;
      }
    )

    this.be.getAllActuatorsForDevice(this.id).subscribe(
      actuators => {
        this.actuators = actuators;
        actuators.forEach(element => {
          this.actuatorCubes.push({ id: element.id, visible: true })
          this.be.getActuatorMetadatas(element.id).subscribe(
            metadatas => {
              this.actuatorWithMetadata.push({ actuator: element, metadatas: metadatas })
            }
          )
        });
      }
    )

    this.be.getAllSensorsForDevice(this.id).subscribe(
      sensors => {
        this.sensors = sensors;
        sensors.forEach(element => {
          this.sensorCubes.push({ id: [element.id], visible: true })
          this.be.getSensorMetadatas(element.id).subscribe(
            metadatas => {
              this.sensorWithMetadata.push({ sensor: element, metadatas: metadatas })
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

    this.calculateCols();
  
})};

ngAfterViewInit(): void {
  this.calculateCols();

  fromEvent(window, 'resize')
    .pipe(debounceTime(100))
    .subscribe(() => this.calculateCols());
}

getStatusColor(state: number): string {
  switch (state) {
    case 0: return 'red';
    case 1: return 'yellow';
    case 2: return 'green';
    default: return 'white';
  }
}

calculateCols(): void {
  const containerWidth = this.theContainer.nativeElement.clientWidth;
  this.columnNum = Math.floor(containerWidth / 400);
}

  ngOnChanges(changes: SimpleChanges): void {
  }
  updateMetadata(numericValue: string, stringValue: string, minVal: string, maxVal: string) {
    this.selectedSensors.forEach(sensorId => {
      let metadataId = this.formControlObj?.value;
      console.log(metadataId);
      let selectedMetadata = this.avaliableMetadata.find(x => x.id == metadataId);
      if(selectedMetadata)
        this.be.postSensorMetadata({name: selectedMetadata.name, metadataId: selectedMetadata.id, numericValue: Number(numericValue), stringValue: stringValue, minVal: Number(minVal), maxVal: Number(maxVal), unit: "", deviceId: sensorId, typeValue: 0})
    });
  }

  listSelectionChange(event: MatSelectionListChange) {
    // Get the selected options
    const selectedOptions = event.source.selectedOptions.selected;
    
    // Extract the sensor IDs from the selected options
    const selectedSensorIds = selectedOptions.map(option => {
      // Get the sensor data from the option
      if (option.value) {
        return option.value.sensor.id;
      } else {
        const index = option._elementRef?.nativeElement?.getAttribute('data-index');
        if (index !== null && index !== undefined) {
          return this.sensorWithMetadata[parseInt(index)].sensor.id;
        }
        return null; // Or handle this case appropriately
      }
    }).filter(id => id !== null); // Remove any null values

    this.selectedSensors = selectedSensorIds;
  }

  deleteMetadata(id: string) {
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

  selectDevice(device: Device): void {
    this.router.navigateByUrl('/', { skipLocationChange: true }).then(() => {
      this.router.navigate(['/device/'+device.id])});
  }
}
