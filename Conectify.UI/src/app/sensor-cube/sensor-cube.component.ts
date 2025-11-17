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
import { SensorData } from 'src/models/SensorData';

@Component({
  selector: 'app-sensor-cube',
  templateUrl: './sensor-cube.component.html',
  styleUrls: ['./sensor-cube.component.css']
})

export class SensorCubeComponent implements OnInit, OnChanges {

  @Input() sensorInput?: { id: string[], visible: boolean };
  @Input() params?: DashboardParams;
  public sensors: SensorData[] = [];
  public device?: Device;
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
    const sensor = this.sensors?.find(s => s.id === msg.sourceId);
    if (sensor) {
      sensor.latestVal = msg;
      this.getAccentColor();
      this.addData(msg);
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
      this.sensorInput.id.forEach(sensorId => {
        this.be.getSensorDetail(sensorId).subscribe(x => {
          var currentSensor: SensorData = { id: x.id, sensor: x, metadatas: [] as Metadata[] }


          if (currentSensor) {
            this.sensors.push(currentSensor);
            if (!this.device)
              this.be.getDevice(x.sourceDeviceId).subscribe(x => {
            this.device = x;
            if(!this.isSoloSensor()){
              this.showName = this.device.name;
            }
          
          });

            this.be.getSensorMetadatas(x.id).subscribe(x => {
              currentSensor.metadatas = x;
              this.HandleMetadata();
            });
            this.be.getLatestSensorValue(currentSensor.id).subscribe(x => {
              currentSensor.latestVal = x;
              this.getAccentColor();
              if (this.isSoloSensor())
                this.addData(currentSensor.latestVal);
            });

            if (this.isSoloSensor()) {
              this.GenerateChart(x);
            }
          }
        });
      });
    }
  }

  private isSoloSensor() {
    return this.sensorInput?.id.length == 1;
  }

  private GenerateChart(x: Sensor) {
    this.be.getSensorValues(x.id).subscribe(
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
          let lastValue = values[values.length - 1].numericValue;
          this.mapedValues.push([new Date().getTime(), lastValue]);
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
        name: this.sensors[0]?.sensor.name,
        data: this.mapedValues.map(x => new Date(x[0]).toLocaleDateString()),
        type: 'line',
        symbolKeepAspect: false,
      }]
    };
  }

  private HandleMetadata() {
    if (this.isSoloSensor()) {
      var visibilityMetadata = this.sensors[0].metadatas.find(x => x.name === "Visible");
      if (visibilityMetadata && this.sensorInput) {
        this.sensorInput.visible = visibilityMetadata.numericValue > 0;
      }
      var nameMetadata = this.sensors[0].metadatas.find(x => x.name === "Name");
      if (nameMetadata) {
        this.showName = nameMetadata.stringValue;
      }
    }
  }

  private getAccentColor() {
    if (this.isSoloSensor() && this.sensors[0].latestVal?.numericValue) {

      var metadata = this.sensors[0].metadatas.find(x => x.maxVal >= this.sensors[0].latestVal!.numericValue && x.minVal < this.sensors[0].latestVal!.numericValue);
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

    if(this.isSoloSensor()){
    let thresholdPieces = this.sensors[0].metadatas
      .filter(m => m.name === "Threshold") // Filter metadata where type is "Threshold"
      .map(m => ({
        gt: m.minVal,
        lte: m.maxVal,
        color: m.stringValue
      }));
    if (thresholdPieces.length == 0) {
      thresholdPieces.push({ gt: -10000, lte: -9000, color: this.defaultColor })
    }

    this.mergeOptions = {
      series: [{
        name: this.sensors[0].sensor.name,
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
  }

  openOverlay() {
    if(this.isSoloSensor()){
    const dialogRef = this.dialog.open(SensorDetailComponent, {
      width: '70%',
      height: '80%',
      data: { sensor: this.sensors[0], metadata: this.sensors[0].metadatas },
      panelClass: "sensor-detail-panel"
    });
  }
  }

  SourceClick() {
    this.router.navigate(['/device/' + this.device?.id])
  }
}
