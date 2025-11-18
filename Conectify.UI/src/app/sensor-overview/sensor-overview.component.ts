import { Component, HostListener, OnInit, ViewChild, ViewContainerRef } from '@angular/core';
import { BEFetcherService } from '../befetcher.service';
import { MessagesService } from '../messages.service';
import { fromEvent, forkJoin } from 'rxjs';
import { debounceTime } from 'rxjs/operators';

@Component({
  selector: 'app-sensor-overview',
  templateUrl: './sensor-overview.component.html',
  styleUrls: ['./sensor-overview.component.css']
})
export class SensorOverviewComponent implements OnInit {
  public sensors: {id: string[], visible: boolean}[] = [];
  columnNum: number = 4;
  tileSize: number = 400;
  @ViewChild('theContainer') theContainer: any;

  constructor(private be: BEFetcherService, private messanger: MessagesService) { 
  }

  ngAfterViewInit(): void {
    this.calculateCols();
  
    fromEvent(window, 'resize')
      .pipe(debounceTime(100))
      .subscribe(() => this.calculateCols());
  }

  ngOnInit(): void {
    // Use forkJoin to batch all API calls and load data in parallel
    forkJoin({
      devices: this.be.getAllDevices(),
      activeSensors: this.be.getActiveSensors()
    }).subscribe({
      next: ({ devices, activeSensors }) => {
        // Process combo devices
        const combos = devices.filter(x => x.metadata?.some(md => md.name === "combo" && md.numericValue === 1));
        
        // Batch all sensor requests for combo devices
        const comboSensorRequests = combos.map(device => 
          this.be.getAllSensorsForDevice(device.id)
        );

        if (comboSensorRequests.length > 0) {
          forkJoin(comboSensorRequests).subscribe(sensorArrays => {
            sensorArrays.forEach(sensors => {
              this.sensors.push({id: sensors.map(s => s.id), visible: true});
            });
            
            // Add active sensors
            activeSensors.forEach(s => this.sensors.push({id: [s], visible: true}));
            
            this.calculateCols();
          });
        } else {
          // No combo devices, just add active sensors
          activeSensors.forEach(s => this.sensors.push({id: [s], visible: true}));
          this.calculateCols();
        }
      },
      error: (err) => {
        this.messanger.addMessage("Error!");
        this.messanger.addMessage(JSON.stringify(err));
      }
    });
  }

  calculateCols(){
    let width = this.theContainer.nativeElement.offsetWidth;
    this.columnNum = Math.trunc(width/this.tileSize);
  }
}