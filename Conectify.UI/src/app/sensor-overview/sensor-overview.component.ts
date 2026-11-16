import { Component, OnInit } from '@angular/core';
import { BEFetcherService } from '../befetcher.service';
import { MessagesService } from '../messages.service';
import { forkJoin } from 'rxjs';
import { UiSensor } from 'src/models/UiSensor';

@Component({
  selector: 'app-sensor-overview',
  templateUrl: './sensor-overview.component.html',
  styleUrls: ['./sensor-overview.component.css']
})
export class SensorOverviewComponent implements OnInit {
  public sensors: {id: string[], visible: boolean, preloaded?: UiSensor[]}[] = [];

  constructor(private be: BEFetcherService, private messanger: MessagesService) {
  }

  ngOnInit(): void {
    forkJoin({
      uiSensors: this.be.getUiSensors(),
      devices: this.be.getAllDevices()
    }).subscribe({
      next: ({ uiSensors, devices }) => {
        const sensorMap = new Map<string, UiSensor>(uiSensors.map(s => [s.id, s]));

        const combos = devices.filter(x => x.metadata?.some(md => md.name === "combo" && md.numericValue === 1));

        if (combos.length > 0) {
          const comboSensorRequests = combos.map(device =>
            this.be.getAllSensorsForDevice(device.id)
          );

          forkJoin(comboSensorRequests).subscribe(sensorArrays => {
            const comboSensorIds = new Set<string>();
            sensorArrays.forEach(sensorList => {
              const ids = sensorList.map(s => s.id);
              const preloaded = ids.map(id => sensorMap.get(id)).filter(s => s !== undefined) as UiSensor[];
              this.sensors.push({ id: ids, visible: true, preloaded });
              ids.forEach(id => comboSensorIds.add(id));
            });

            uiSensors
              .filter(s => !comboSensorIds.has(s.id))
              .forEach(s => this.sensors.push({ id: [s.id], visible: true, preloaded: [s] }));
          });
        } else {
          uiSensors.forEach(s => this.sensors.push({ id: [s.id], visible: true, preloaded: [s] }));
        }
      },
      error: (err) => {
        this.messanger.addMessage("Error!");
        this.messanger.addMessage(JSON.stringify(err));
      }
    });
  }
}
