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
import { color } from 'echarts';

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
  public showName?: string = undefined;
  public accentColor: string = "#2a2a2a";
  mapedValues: (number | number)[][] = [];
  mergeOptions = {};
  chartOption: any = {};

  defaultColor: string = "#4fc3f7";

  constructor(public messenger: MessagesService, private be: BEFetcherService, public dialog: MatDialog, private websocketService: WebsocketService, private router: Router) {
  }

  HandleIncomingValue(msg: any): void {
    var id = msg.sourceId;
    if (id && this.sensor && id == this.sensor.id) {
      this.latestVal = msg;
      this.getAccentColor();
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
            this.HandleMetadata();
          });
          this.be.getSensorValues(this.sensor.id).subscribe(
            values => {
              this.valsReady = values.length > 0;
              if (this.valsReady) {
                let previousTick = new Date().getTime() - 86400000;
                let previousValue = values[0].numericValue;
                values.forEach(value => {
                  for (let i = previousTick; i < value.timeCreated; i = i + 30000) {
                    this.mapedValues.push([i, previousValue]);
                  }
                  previousValue = value.numericValue;
                  previousTick = value.timeCreated;
                });
                let lastValue = values[values.length-1].numericValue;
                this.mapedValues.push([new Date().getTime(), lastValue])
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
              data: this.mapedValues.map(x => new Date(x[0]).toLocaleDateString()),
              type: 'line',
              symbolKeepAspect: false,
            }]
          }
          this.be.getLatestSensorValue(this.sensor.id).subscribe(x => {
            this.latestVal = x;
            this.getAccentColor();
            this.addData(this.latestVal);
          });
        }
      });
    }
  }

  private HandleMetadata() {
    var visibilityMetadata = this.metadatas.find(x => x.name === "Visible");
    if (visibilityMetadata && this.sensorInput) {
      this.sensorInput.visible = visibilityMetadata.numericValue > 0;
    }
    var nameMetadata = this.metadatas.find(x => x.name === "Name");
    if (nameMetadata) {
      this.showName = nameMetadata.stringValue;
    }
  }

  private getAccentColor() {
    if (this.latestVal?.numericValue) {

      var metadata = this.metadatas.find(x => x.maxVal >= this.latestVal!.numericValue && x.minVal < this.latestVal!.numericValue);
      if (metadata) {
        this.accentColor = metadata.stringValue;
        return;
      }
    }

    this.accentColor = '#2a2a2a';
  }

  public onClick() {
    if (this.params?.editable) {
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

    let thresholdPieces = this.metadatas
    .filter(m => m.name === "Threshold") // Filter metadata where type is "Threshold"
    .map(m => ({
      gt: m.minVal,
      lte: m.maxVal,
      color: m.stringValue
    }));
    if(thresholdPieces.length == 0){
      thresholdPieces.push({gt: -10000, lte: -9000, color: this.defaultColor })
    }

    this.mergeOptions = {
      series: [{
        name: this.sensor?.name,
        data: newData,
        type: 'line',
        symbolKeepAspect: false,
      }],
      visualMap: {
        show: false,
        ...(thresholdPieces.length > 0 ? { pieces: thresholdPieces } : {}),
        outOfRange: {
          color: this.defaultColor
        }
      }
    }
  }

  openOverlay() {
    const dialogRef = this.dialog.open(SensorDetailComponent, {
      width: '70%',
      height: '80%',
      data: { sensor: this.sensor, metadata: this.metadatas },
      panelClass: "sensor-detail-panel"
    });
  }

  SourceClick() {
    /* The line `this.router.navigate(['/device'])` is navigating to the '/device' route in the
    application. It is typically used to redirect the user to a specific page or component within
    the application when a certain action is triggered, such as a button click or a specific event.
    In this case, it seems like it is intended to navigate to the 'device' route when the
    `SourceClick()` method is called in the `SensorCubeComponent` component. */
    this.router.navigate(['/device/' + this.sensor?.sourceDeviceId])
  }
}
