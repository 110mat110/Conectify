import { Component, Input, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { BEFetcherService } from 'src/app/befetcher.service';
import { DashboardApi } from 'src/models/Dashboard/DashboardApi';
import { DasboardAddDeviceDialogComponent } from '../dasboard-add-device-dialog/dasboard-add-device-dialog.component';
import { Sensor } from 'src/models/sensor';
import { CdkDragDrop, CdkDragEnd, moveItemInArray } from '@angular/cdk/drag-drop';
import { DashboardParams } from 'src/models/Dashboard/DashboardParams';
import { EditDashboardDeviceApi } from 'src/models/Dashboard/EditDashboardDeviceApi';
import { DashboardDeviceApi } from 'src/models/Dashboard/DashboardDevice';
import { Actuator } from 'src/models/actuator';
import { UiSensor } from 'src/models/UiSensor';
import { UiActuator } from 'src/models/UiActuator';
import { forkJoin } from 'rxjs';

interface GridItem {
  dashboardDevice: DashboardDeviceApi;
  sourceType: 'sensor' | 'actuator';
  sensorCube: { id: string[]; visible: boolean; position: { x: number; y: number }; preloaded?: UiSensor[] } | null;
  actuatorCube: { id: string; visible: boolean; position: { x: number; y: number }; preloaded?: UiActuator } | null;
}

@Component({
  selector: 'app-dashboardcontent',
  templateUrl: './dashboardcontent.component.html',
  styleUrls: ['./dashboardcontent.component.css'],
})
export class DashboardcontentComponent implements OnInit {
  @Input() dashboard: DashboardApi = null!;
  sensors: {
    apiSensor: DashboardDeviceApi;
    cube: { id: string[]; visible: boolean; position: { x: number; y: number }; preloaded?: UiSensor[] };
  }[] = [];
  actuators: {
    apiActuator: DashboardDeviceApi;
    cube: { id: string; visible: boolean; position: { x: number; y: number }; preloaded?: UiActuator };
  }[] = [];
  gridItems: GridItem[] = [];
  params: DashboardParams = { editable: false };
  sensorCursor: string = 'pointer';

  constructor(private befetcher: BEFetcherService, public dialog: MatDialog) { }

  get isGrid(): boolean {
    return this.dashboard?.type === 1;
  }

  ngOnInit(): void {
    // Pull the fully-prepared sensor/actuator data (metadata + latest + 24h values) in a single
    // call, then hand each cube its data so it never has to fetch anything itself.
    forkJoin({
      uiSensors: this.befetcher.getUiSensors(),
      uiActuators: this.befetcher.getUiActuators(),
    }).subscribe({
      next: ({ uiSensors, uiActuators }) => this.buildDashboard(uiSensors, uiActuators),
      error: () => this.buildDashboard([], []),
    });
  }

  private buildDashboard(uiSensors: UiSensor[], uiActuators: UiActuator[]): void {
    const sensorMap = new Map<string, UiSensor>(uiSensors.map(s => [s.id, s]));
    const actuatorMap = new Map<string, UiActuator>(uiActuators.map(a => [a.id, a]));

    if (this.dashboard.dashboardDevices) {
      this.dashboard.dashboardDevices.forEach((item) => {
        if (item.sourceType === 'sensor') {
          const preloaded = sensorMap.get(item.deviceId);
          this.sensors.push({
            apiSensor: item,
            cube: {
              id: [item.deviceId],
              visible: true,
              position: { x: item.posX, y: item.posY },
              preloaded: preloaded ? [preloaded] : undefined,
            },
          });
        } else if (item.sourceType === 'actuator') {
          this.actuators.push({
            apiActuator: item,
            cube: {
              id: item.deviceId,
              visible: true,
              position: { x: item.posX, y: item.posY },
              preloaded: actuatorMap.get(item.deviceId),
            },
          });
        }
      });
    }
    this.rebuildGridItems();
  }

  private rebuildGridItems(): void {
    const items: GridItem[] = [
      ...this.sensors.map(s => ({
        dashboardDevice: s.apiSensor,
        sourceType: 'sensor' as const,
        sensorCube: s.cube,
        actuatorCube: null,
      })),
      ...this.actuators.map(a => ({
        dashboardDevice: a.apiActuator,
        sourceType: 'actuator' as const,
        sensorCube: null,
        actuatorCube: a.cube,
      })),
    ];
    this.gridItems = items.sort((a, b) => a.dashboardDevice.posX - b.dashboardDevice.posX);
  }

  remove(id: string) {
    this.befetcher.removeDashboardDevice(this.dashboard.id, id);

    let index = this.sensors.findIndex(d => d.apiSensor.id === id);
    if (index > -1) {
      this.sensors.splice(index, 1);
      this.rebuildGridItems();
      return;
    }

    index = this.actuators.findIndex(d => d.apiActuator.id === id);
    if (index > -1) {
      this.actuators.splice(index, 1);
      this.rebuildGridItems();
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
      const nextOrder = this.gridItems.length;

      var actuators = result.actuator as Actuator[];
      if (actuators) {
        actuators.forEach((actuator, i) => {
          const posX = this.isGrid ? nextOrder + i : 100;
          this.befetcher
            .addDashboardDevice(this.dashboard.id, {
              sourceType: 'actuator',
              deviceId: actuator.id,
              posX,
              posY: 100,
            })
            .subscribe((id) => {
              var device = {
                id: id,
                sourceType: 'actuator',
                deviceId: actuator.id,
                posX,
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
              this.rebuildGridItems();
            });
        });
      }
      var sensors = result.sensors as Sensor[];
      if (sensors) {
        sensors.forEach((sensor, i) => {
          const posX = this.isGrid ? nextOrder + (actuators?.length ?? 0) + i : 100;
          this.befetcher
            .addDashboardDevice(this.dashboard.id, {
              sourceType: 'sensor',
              deviceId: sensor.id,
              posX,
              posY: 100,
            })
            .subscribe((id) => {
              var device = {
                id: id,
                sourceType: 'sensor',
                deviceId: sensor.id,
                posX,
                posY: 100,
              };
              this.sensors.push({
                apiSensor: device,
                cube: {
                  id: [sensor.id],
                  visible: true,
                  position: { x: device.posX, y: device.posY },
                },
              });
              this.rebuildGridItems();
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
    const transform = event.source.element.nativeElement.style.transform;
    const regex = /translate3d\((-?\d+)px, (-?\d+)px, 0px\)/;
    const matches = transform.match(regex);

    if (matches) {
      const deltaX = parseInt(matches[1], 10);
      const deltaY = parseInt(matches[2], 10);

      const newX = rule.posX + deltaX;
      const newY = rule.posY + deltaY;

      let apiModel: EditDashboardDeviceApi = { id: rule.id, posX: newX, posY: newY };
      rule.posX = newX;
      rule.posY = newY;

      event.source.reset();
      this.befetcher.editDasboardDevice(this.dashboard.id, apiModel);
    }
  }

  dropGridItem(event: CdkDragDrop<GridItem[]>) {
    moveItemInArray(this.gridItems, event.previousIndex, event.currentIndex);
    this.gridItems.forEach((item, index) => {
      if (item.dashboardDevice.posX !== index) {
        item.dashboardDevice.posX = index;
        this.befetcher.editDasboardDevice(this.dashboard.id, {
          id: item.dashboardDevice.id,
          posX: index,
          posY: 0,
        });
      }
    });
  }
}
