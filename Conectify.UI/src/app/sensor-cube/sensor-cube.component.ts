import { Overlay, OverlayConfig, OverlayRef, PositionStrategy } from '@angular/cdk/overlay';
import { ComponentPortal } from '@angular/cdk/portal';
import { Component, ElementRef, HostListener, Input, OnChanges, OnInit, SimpleChanges, ViewChild, ViewContainerRef } from '@angular/core';
import { BaseInputType } from 'src/models/extendedValue';
import { Metadata } from 'src/models/metadata';
import { Sensor } from 'src/models/sensor';
import { Device } from 'src/models/thing';
import { BEFetcherService } from '../befetcher.service';
import { MessagesService } from '../messages.service';
import { SensorDetailComponent } from '../sensor-detail/sensor-detail.component';
import { WebsocketService } from '../websocket.service';
import { DashboardParams } from 'src/models/Dashboard/DashboardParams';
import { MatDialog } from '@angular/material/dialog';
import { Router } from '@angular/router';

@Component({
  selector: 'app-sensor-cube',
  templateUrl: './sensor-cube.component.html',
  styleUrls: ['./sensor-cube.component.css']
})
export class SensorCubeComponent implements OnInit, OnChanges {

  @Input() sensorInput?: { id: string, visible: boolean };
  @Input() params?: DashboardParams;
  public sensor?: Sensor;
  public device?: Device;
  public latestVal?: BaseInputType;
  public metadatas: Metadata[] = [];
  public valsReady: boolean = false;
  mapedValues: (number | number)[][] = [];

  mergeOptions = {};
  chartOption: any = {};

  constructor(public messenger: MessagesService, private be: BEFetcherService, public dialog: MatDialog,private websocketService: WebsocketService, private router: Router) {
  }

  HandleIncomingValue(msg: any): void {
    var id = msg.sourceId;
    if (id && this.sensor && id == this.sensor.id) {
      this.latestVal = msg;
      this.addData(this.latestVal);
    }
  }

  onDetailsClick(): void {
  }

  ngOnChanges(changes: SimpleChanges): void {
  }

  ngOnInit(): void {
    this.websocketService.receivedMessages.subscribe(msg => {
      this.HandleIncomingValue(msg);
    });

    if (this.sensorInput) {
      this.be.getSensorDetail(this.sensorInput.id).subscribe(x => {
        this.sensor = x

        if (this.sensor) {

          this.be.getDevice(this.sensor.sourceDeviceId).subscribe(x => this.device = x);
          this.be.getSensorMetadatas(this.sensor.id).subscribe(x => {
            this.metadatas = x;
            var visibilityMetadata = this.metadatas.find(x => x.name === "Visible");
            if (visibilityMetadata && this.sensorInput) {
              this.sensorInput.visible = visibilityMetadata.numericValue > 0;
            }
          });
          this.be.getSensorValues(this.sensor.id).subscribe(
            values => {
              this.valsReady = values.length > 0;
              if (this.valsReady) {
                let previousTick = new Date().getTime() - 86400000;
                let previousValue = values[0].numericValue;
                values.forEach(value => {
                  for (let i = previousTick; i< value.timeCreated; i = i + 30000){
                    this.mapedValues.push([i, previousValue]);
                  }
                  previousValue = value.numericValue;
                  previousTick = value.timeCreated;
                });
              }
            });
          this.chartOption = {
            xAxis: {
              type: 'category',
              show: false
            },
            yAxis: {
              type: 'value',
              show: false
            },
            series: [{
              name: this.sensor?.name,
              data: this.mapedValues.map( x => new Date(x[0]).toLocaleDateString()),
              type: 'line',
              symbolKeepAspect: false,
            }]
          }
          this.be.getLatestSensorValue(this.sensor.id).subscribe(x => {
            this.latestVal = x;
            this.addData(this.latestVal);
          });
        }
      });
    }
  }

  public onClick() {
    if(this.params?.editable){
      return;
    }
    this.openOverlay();
  }

  addData(newVal: BaseInputType | undefined) {
    if (newVal == null) {
      return;
    }
    let newData = this.mapedValues;
    newData.push([newVal.timeCreated, newVal.numericValue]);

    this.mergeOptions = {
      series: [{
        name: this.sensor?.name,
        data: newData,
        type: 'line',
        symbolKeepAspect: false,
      }]
    }
  }

  openOverlay() {
    const dialogRef = this.dialog.open(SensorDetailComponent, {
      width: '70%',
      height: '80%',
      data: {sensor: this.sensor},
      panelClass: "sensor-detail-panel"
    });
  }

  SourceClick(){
    /* The line `this.router.navigate(['/device'])` is navigating to the '/device' route in the
    application. It is typically used to redirect the user to a specific page or component within
    the application when a certain action is triggered, such as a button click or a specific event.
    In this case, it seems like it is intended to navigate to the 'device' route when the
    `SourceClick()` method is called in the `SensorCubeComponent` component. */
    this.router.navigate(['/device/'+this.sensor?.sourceDeviceId])
  }
}
