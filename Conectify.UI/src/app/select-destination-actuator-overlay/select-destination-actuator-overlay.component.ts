import { HostListener, Input, ViewChild } from '@angular/core';
import { Component, OnInit } from '@angular/core';
import { Actuator } from 'src/models/actuator';
import { AutChangeDestinationComponent } from '../aut-change-destination/aut-change-destination.component';
import { BEFetcherService } from '../befetcher.service';
import { MessagesService } from '../messages.service';
import { forkJoin } from 'rxjs';
import { Metadata } from 'src/models/metadata';
import { Device } from 'src/models/thing';

interface ActuatorItem {
  actuator: Actuator;
  device: Device | null;
  displayName: string;
  color: string;
  selected: boolean;
}

@Component({
  selector: 'app-select-destination-actuator-overlay',
  templateUrl: './select-destination-actuator-overlay.component.html',
  styleUrls: ['./select-destination-actuator-overlay.component.css']
})
export class SelectDestinationActuatorOverlayComponent implements OnInit {
  @Input() multiselect: boolean = false;
  
  actuators: ActuatorItem[] = [];
  selectedActuators: Actuator[] = [];
  columnNum: number = 4;
  tileSize: number = 150;
  @ViewChild('theContainer') theContainer: any;

  constructor(
    private be: BEFetcherService,
    private messanger: MessagesService,
  ) {}

  select(actuatorItem: ActuatorItem): void {
    if (this.multiselect) {
      // Toggle selection for multiselect mode
      actuatorItem.selected = !actuatorItem.selected;
      actuatorItem.color = actuatorItem.selected ? 'green' : '#6666';
      
      // Update selected actuators array
      if (actuatorItem.selected) {
        this.selectedActuators.push(actuatorItem.actuator);
      } else {
        const index = this.selectedActuators.findIndex(a => a === actuatorItem.actuator);
        if (index > -1) {
          this.selectedActuators.splice(index, 1);
        }
      }
    } else {
      // Single select mode - clear all and select one
      this.actuators.forEach(a => {
        a.selected = false;
        a.color = '#6666';
      });
      
      actuatorItem.selected = true;
      actuatorItem.color = 'green';
      this.selectedActuators = [actuatorItem.actuator];
    }
  }

  clearSelection(): void {
    this.actuators.forEach(a => {
      a.selected = false;
      a.color = '#6666';
    });
    this.selectedActuators = [];
  }

  getSelectedActuators(): Actuator[] {
    return [...this.selectedActuators];
  }

  private getMetadataName(metadata: Metadata[] | undefined): string | null {
    if(!metadata) return null;
    const nameMetadata = metadata.find(m => m.name.toLowerCase() === 'name');
    return nameMetadata?.stringValue || null;
  }

  private getDisplayName(actuator: Actuator, device: Device | null): string {
    const actuatorName = this.getMetadataName(actuator.metadata) || actuator.name;
    const deviceName = device 
      ? (this.getMetadataName(device.metadata) || device.name)
      : 'Unknown Device';
    
    return `${deviceName}@${actuatorName}`;
  }

  ngOnInit(): void {
    forkJoin({
      actuators: this.be.getAllActuators(),
      devices: this.be.getAllDevices()
    }).subscribe(
      ({ actuators, devices }) => {
        // Create a map of devices by ID for quick lookup
        const deviceMap = new Map<string, Device>();
        devices.forEach(device => deviceMap.set(device.id, device));

        // Process actuators and pair with devices
        this.actuators = actuators.map(actuator => {
          const device = deviceMap.get(actuator.sourceDeviceId) || null;
          const displayName = this.getDisplayName(actuator, device);

          return {
            actuator,
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