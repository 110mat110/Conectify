import { Component, ElementRef, HostListener, Input, OnChanges, OnInit, SimpleChanges, ViewChild, ViewContainerRef, OnDestroy } from '@angular/core';
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
import { SensorData } from 'src/models/SensorData';
import { UiSensor } from 'src/models/UiSensor';
import { forkJoin, Subscription } from 'rxjs';

@Component({
  selector: 'app-sensor-cube',
  templateUrl: './sensor-cube.component.html',
  styleUrls: ['./sensor-cube.component.css']
})

export class SensorCubeComponent implements OnInit, OnChanges, OnDestroy {

  @Input() sensorInput?: { id: string[], visible: boolean, preloaded?: UiSensor[] };
  @Input() params?: DashboardParams;
  public sensors: SensorData[] = [];
  public device?: Device;
  public valsReady: boolean = false;
  public showName?: string = undefined;
  public accentColor: string = "#2a2a2a";
  mapedValues: (number | number)[][] = [];
  mergeOptions = {};
  chartOption: any = {};

  private wsSubscription?: Subscription;
  private timeShiftInterval?: any;
  defaultColor: string = "#4fc3f7";

  constructor(
    public messenger: MessagesService,
    private be: BEFetcherService,
    public dialog: MatDialog,
    private websocketService: WebsocketService,
    private router: Router
  ) {}

  HandleIncomingValue(msg: any): void {
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
    this.wsSubscription = this.websocketService.receivedMessages.subscribe(msg => {
      this.HandleIncomingValue(msg);
    });

    this.timeShiftInterval = setInterval(() => this.shiftTime(), 60000);

    if (!this.sensorInput || !this.sensorInput.id.length) return;

    if (this.sensorInput.preloaded?.length) {
      this.initFromPreloaded(this.sensorInput.preloaded);
    } else {
      this.initFromHttp(this.sensorInput.id);
    }
  }

  private initFromPreloaded(preloaded: UiSensor[]): void {
    this.sensors = preloaded.map(s => ({
      id: s.id,
      sensor: { id: s.id, name: s.name, sourceDeviceId: s.sourceDeviceId, metadata: [] } as Sensor,
      metadatas: s.metadata as Metadata[],
      latestVal: s.latestValue ?? undefined,
    }));

    if (preloaded.length > 0) {
      this.device = { id: preloaded[0].sourceDeviceId, name: preloaded[0].deviceName } as Device;
      if (!this.isSoloSensor()) {
        this.showName = this.device.name;
      }
    }

    this.HandleMetadata();
    this.getAccentColor();

    if (this.isSoloSensor()) {
      const latestVal = preloaded[0].latestValue;
      if (latestVal) {
        this.addData(latestVal);
      }
      this.GenerateChart(this.sensors[0].sensor);
    }
  }

  private initFromHttp(ids: string[]): void {
    const sensorDetailRequests = ids.map(sensorId =>
      this.be.getSensorDetail(sensorId)
    );

    forkJoin(sensorDetailRequests).subscribe(sensorDetails => {
      this.sensors = sensorDetails.map(sensor => ({
        id: sensor.id,
        sensor: sensor,
        metadatas: [] as Metadata[]
      }));

      if (sensorDetails.length > 0) {
        this.be.getDevice(sensorDetails[0].sourceDeviceId).subscribe(device => {
          this.device = device;
          if (!this.isSoloSensor()) {
            this.showName = this.device.name;
          }
        });
      }

      forkJoin(this.sensors.map(sensor => this.be.getSensorMetadatas(sensor.id)))
        .subscribe(metadatas => {
          this.sensors.forEach((sensor, i) => {
            sensor.metadatas = metadatas[i];
          });
          this.HandleMetadata();
          this.getAccentColor();
          if (this.isSoloSensor()) {
            this.GenerateChart(this.sensors[0].sensor);
          }
        });

      forkJoin(this.sensors.map(sensor => this.be.getLatestSensorValue(sensor.id)))
        .subscribe(latestValues => {
          this.sensors.forEach((sensor, i) => {
            sensor.latestVal = latestValues[i];
          });
          this.getAccentColor();
          if (this.isSoloSensor()) {
            this.addData(this.sensors[0].latestVal);
          }
        });
    });
  }

  ngOnDestroy(): void {
    if (this.wsSubscription) {
      this.wsSubscription.unsubscribe();
    }
    if (this.timeShiftInterval) {
      clearInterval(this.timeShiftInterval);
    }
  }

  private shiftTime(): void {
    if (!this.isSoloSensor() || this.mapedValues.length === 0) return;

    const now = new Date().getTime();
    const cutoff = now - 86400000;
    const lastValue = this.mapedValues[this.mapedValues.length - 1][1];

    this.mapedValues.push([now, lastValue]);
    this.mapedValues = this.mapedValues.filter(p => p[0] >= cutoff);

    this.updateChartWithThresholds();
  }

  private isSoloSensor() {
    return this.sensorInput?.id.length == 1;
  }

  private GenerateChart(x: Sensor) {
    this.be.getUiSensorValues(x.id).subscribe(values => {
      this.valsReady = values.length > 0;
      if (this.valsReady) {
        let previousTick = new Date().getTime() - 86400000;
        let previousValue = values[0].numericValue;

        this.mapedValues = [];

        values.forEach(value => {
          for (let i = previousTick; i < value.timeCreated; i = i + 30000) {
            this.mapedValues.push([i, previousValue]);
          }
          previousValue = value.numericValue;
          previousTick = value.timeCreated;
        });

        let now = new Date().getTime();
        for (let i = previousTick; i <= now; i = i + 30000) {
          this.mapedValues.push([i, previousValue]);
        }

        this.updateChartWithThresholds();
      }
    });

    this.chartOption = {
      grid: { top: 0, right: 0, bottom: 0, left: 0, containLabel: false },
      xAxis: {
        type: 'time',
        show: false
      },
      yAxis: {
        type: 'value',
        show: false
      },
      series: [{
        name: this.sensors[0]?.sensor.name,
        data: [],
        type: 'line',
        symbol: 'none',
        symbolKeepAspect: false,
      }]
    };
  }

  private updateChartWithThresholds() {
    if (!this.isSoloSensor()) return;

    let thresholdPieces = this.sensors[0].metadatas
      .filter(m => m.name === "Threshold")
      .map(m => ({
        gt: m.minVal,
        lte: m.maxVal,
        color: m.stringValue
      }));

    if (thresholdPieces.length == 0) {
      thresholdPieces.push({ gt: -10000, lte: -9000, color: this.defaultColor });
    }

    this.mergeOptions = {
      grid: { top: 0, right: 0, bottom: 0, left: 0, containLabel: false },
      series: [{
        name: this.sensors[0].sensor.name,
        data: this.mapedValues,
        type: 'line',
        symbol: 'none',
        symbolKeepAspect: false,
      }],
      visualMap: {
        show: false,
        ...(thresholdPieces.length > 0 ? { pieces: thresholdPieces } : {}),
        outOfRange: {
          color: this.defaultColor
        }
      }
    };
  }

  private HandleMetadata() {
    if (this.isSoloSensor()) {
      const visibilityMetadata = this.sensors[0].metadatas.find(x => x.name === "Visible");
      if (visibilityMetadata && this.sensorInput) {
        this.sensorInput.visible = visibilityMetadata.numericValue > 0;
      }
      const nameMetadata = this.sensors[0].metadatas.find(x => x.name === "Name");
      if (nameMetadata) {
        this.showName = nameMetadata.stringValue;
      }
    }
  }

  private getAccentColor() {
    if (this.isSoloSensor() && this.sensors[0].latestVal?.numericValue) {
      const metadata = this.sensors[0].metadatas.find(x =>
        x.maxVal >= this.sensors[0].latestVal!.numericValue &&
        x.minVal < this.sensors[0].latestVal!.numericValue
      );
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

    this.mapedValues.push([newVal.timeCreated, newVal.numericValue]);

    if (this.isSoloSensor()) {
      this.updateChartWithThresholds();
    }
  }

  openOverlay() {
    if (this.isSoloSensor()) {
      const dialogRef = this.dialog.open(SensorDetailComponent, {
        width: '70%',
        height: '80%',
        data: { sensor: this.sensors[0], metadata: this.sensors[0].metadatas, latestVal: this.sensors[0].latestVal },
        panelClass: "sensor-detail-panel"
      });
    }
  }

  SourceClick() {
    this.router.navigate(['/device/' + this.device?.id]);
  }
}
