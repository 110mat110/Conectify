import {
  Component,
  HostListener,
  Input,
  OnInit,
  ViewChild,
} from '@angular/core';
import { Sensor } from 'src/models/sensor';
import { BEFetcherService } from '../befetcher.service';
import { MessagesService } from '../messages.service';
import { AutValueInputComponent } from '../aut-inner-components/aut-value-input/aut-value-input.component';
import { forkJoin } from 'rxjs';
import { Metadata } from 'src/models/metadata';
import { Device } from 'src/models/thing';

interface SensorItem {
  sensor: Sensor;
  device: Device | null;
  displayName: string;
  color: string;
  selected: boolean;
}

@Component({
  selector: 'app-select-input-sensor-overlay',
  templateUrl: './select-input-sensor-overlay.component.html',
  styleUrls: ['./select-input-sensor-overlay.component.css'],
})
export class SelectInputSensorOverlayComponent implements OnInit {
  @Input() creatorComponent?: AutValueInputComponent;
  @Input() multiselect: boolean = true;
  
  selectedSensors: Sensor[] = [];
  sensors: SensorItem[] = [];
  columnNum: number = 4;
  tileSize: number = 150;
  
  @ViewChild('theContainer') theContainer: any;

  constructor(
    private be: BEFetcherService,
    private messanger: MessagesService
  ) {}

  select(sensorItem: SensorItem): void {
    if (this.multiselect) {
      // Toggle selection for multiselect mode
      sensorItem.selected = !sensorItem.selected;
      sensorItem.color = sensorItem.selected ? 'green' : '#6666';
      
      // Update selected sensors array
      if (sensorItem.selected) {
        this.selectedSensors.push(sensorItem.sensor);
      } else {
        const index = this.selectedSensors.findIndex(s => s === sensorItem.sensor);
        if (index > -1) {
          this.selectedSensors.splice(index, 1);
        }
      }
    } else {
      // Single select mode - clear all and select one
      this.sensors.forEach(s => {
        s.selected = false;
        s.color = '#6666';
      });
      
      sensorItem.selected = true;
      sensorItem.color = 'green';
      this.selectedSensors = [sensorItem.sensor];
    }
  }

  clearSelection(): void {
    this.sensors.forEach(s => {
      s.selected = false;
      s.color = '#6666';
    });
    this.selectedSensors = [];
  }

  getSelectedSensors(): Sensor[] {
    return [...this.selectedSensors];
  }

  private getMetadataName(metadata: Metadata[] | undefined): string | null {
    if(!metadata) return null;
    const nameMetadata = metadata.find(m => m.name.toLowerCase() === 'name');
    return nameMetadata?.stringValue || null;
  }

  private getDisplayName(sensor: Sensor, device: Device | null): string {
    const sensorName = this.getMetadataName(sensor.metadata) || sensor.name;
    const deviceName = device 
      ? (this.getMetadataName(device.metadata) || device.name)
      : 'Unknown Device';
    
    return `${deviceName}@${sensorName}`;
  }

  ngOnInit(): void {
    forkJoin({
      sensors: this.be.getAllSensors(),
      devices: this.be.getAllDevices()
    }).subscribe(
      ({ sensors, devices }) => {
        // Create a map of devices by ID for quick lookup
        const deviceMap = new Map<string, Device>();
        devices.forEach(device => deviceMap.set(device.id, device));

        // Process sensors and pair with devices
        this.sensors = sensors.map(sensor => {
          const device = deviceMap.get(sensor.sourceDeviceId) || null;
          const displayName = this.getDisplayName(sensor, device);

          return {
            sensor,
            device,
            displayName,
            color: '#6666',
            selected: false
          };
        }).sort((a, b) => a.displayName.localeCompare(b.displayName));


        this.setColNum();
      },
      (err) => {
        this.messanger.addMessage('Error!');
        this.messanger.addMessage(JSON.stringify(err));
      }
    );
  }

  setColNum(): void {
    if (this.theContainer?.nativeElement) {
      const width = this.theContainer.nativeElement.offsetWidth;
      this.columnNum = Math.trunc(width / this.tileSize);
    }
  }

  @HostListener('window:resize', ['$event'])
  onResize(event: any): void {
    this.setColNum();
  }
}