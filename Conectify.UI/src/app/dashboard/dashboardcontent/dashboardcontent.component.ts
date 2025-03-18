import { Component, Input, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { BEFetcherService } from 'src/app/befetcher.service';
import { DashboardApi } from 'src/models/Dashboard/DashboardApi';
import { DasboardAddDeviceDialogComponent } from '../dasboard-add-device-dialog/dasboard-add-device-dialog.component';
import { Sensor } from 'src/models/sensor';
import { CdkDragEnd } from '@angular/cdk/drag-drop';
import { DashboardParams } from 'src/models/Dashboard/DashboardParams';
import { EditDashboardDeviceApi } from 'src/models/Dashboard/EditDashboardDeviceApi';
import { DashboardDeviceApi } from 'src/models/Dashboard/DashboardDevice';
import { Actuator } from 'src/models/actuator';

@Component({
  selector: 'app-dashboardcontent',
  templateUrl: './dashboardcontent.component.html',
  styleUrls: ['./dashboardcontent.component.css'],
})
export class DashboardcontentComponent implements OnInit {
  @Input() dashboard: DashboardApi = null!;
  sensors: {
    apiSensor: DashboardDeviceApi;
    cube: { id: string; visible: boolean; position: { x: number; y: number } };
  }[] = [];
  actuators: {
    apiActuator: DashboardDeviceApi;
    cube: { id: string; visible: boolean; position: { x: number; y: number } };
  }[] = [];
  params: DashboardParams = { editable: false };
  sensorCursor: string = 'pointer';
  constructor(private befetcher: BEFetcherService, public dialog: MatDialog) {}

  ngOnInit(): void {
    if (this.dashboard.dashboardDevices) {
      this.dashboard.dashboardDevices.forEach((item) => {
        if (item.sourceType === 'sensor') {
          this.sensors.push({
            apiSensor: item,
            cube: {
              id: item.deviceId,
              visible: true,
              position: { x: item.posX, y: item.posY },
            },
          });
        } else if (item.sourceType === 'actuator') {
          this.actuators.push({
            apiActuator: item,
            cube: {
              id: item.deviceId,
              visible: true,
              position: { x: item.posX, y: item.posY },
            },
          });
        }
      });
    }
  }

  remove(id: string) {
    this.befetcher.removeDashboardDevice(this.dashboard.id, id);

    let index = this.sensors.findIndex(d => d.apiSensor.id === id); //find index in your array
        if (index > -1) {
          this.sensors.splice(index, 1);//remove element from array
          return;
        }

    index = this.actuators.findIndex(d => d.apiActuator.id === id); //find index in your array
    if (index > -1) {
      this.actuators.splice(index, 1);//remove element from array
      return;
    }
}

  addDevice() {
    const dialogRef = this.dialog.open(DasboardAddDeviceDialogComponent, {
      width: '60%',
      height: '30%',
      data: { dashboard: this.dashboard, params: this.params },
    });

    dialogRef.afterClosed().subscribe((result) => {
      var actuator = result.actuator as Actuator;
      if (actuator) {
        this.befetcher
        .addDashboardDevice(this.dashboard.id, {
          sourceType: 'actuator',
          deviceId: actuator.id,
          posX: 100,
          posY: 100,
        })
        .subscribe((id) => {
          var device = {
            id: id,
            sourceType: 'actuator',
            deviceId: actuator.id,
            posX: 100,
            posY: 100,
          };
          this.actuators.push({
            apiActuator: device,
            cube: {
              id: actuator.id,
              visible: true,
              position: { x: device.posX, y: device.posY },
            },
          });
        });
      }

      var sensor = result.sensor as Sensor;
      if (sensor) {
        this.befetcher
          .addDashboardDevice(this.dashboard.id, {
            sourceType: 'sensor',
            deviceId: sensor.id,
            posX: 100,
            posY: 100,
          })
          .subscribe((id) => {
            var device = {
              id: id,
              sourceType: 'sensor',
              deviceId: sensor.id,
              posX: 100,
              posY: 100,
            };
            this.sensors.push({
              apiSensor: device,
              cube: {
                id: sensor.id,
                visible: true,
                position: { x: device.posX, y: device.posY },
              },
            });
          });
      }
      this.setCursor();
    });

    dialogRef.backdropClick().subscribe((result) => this.setCursor());

    this.setCursor();
  }

  setCursor() {
    this.sensorCursor = this.params.editable ? 'move' : 'pointer';
  }

  dragEnd(event: CdkDragEnd, rule: DashboardDeviceApi) {
    // Get the transform values directly from the MatrixTransform
    const transform = event.source.element.nativeElement.style.transform;
    const regex = /translate3d\((-?\d+)px, (-?\d+)px, 0px\)/;
    const matches = transform.match(regex);
    
    if (matches) {
      const deltaX = parseInt(matches[1], 10);
      const deltaY = parseInt(matches[2], 10);
      
      // Add these deltas to the original position
      const newX = rule.posX + deltaX;
      const newY = rule.posY + deltaY;
      
      // Update the model
      let apiModel: EditDashboardDeviceApi = { id: rule.id, posX: newX, posY: newY };
      rule.posX = newX;
      rule.posY = newY;
      
      // Reset the drag element's position to avoid double-counting
      event.source.reset();
      
      this.befetcher.editDasboardDevice(this.dashboard.id, apiModel);
    }
  }
}
