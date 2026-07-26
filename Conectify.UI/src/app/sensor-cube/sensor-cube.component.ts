import { Component, Input, OnChanges, OnInit, SimpleChanges, OnDestroy } from '@angular/core';
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
import { UiIcon, UiPoint, UiThreshold, UiValue } from 'src/models/UiCube';
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
  public icons: UiIcon[] = [];
  public thresholds: UiThreshold[] = [];
  mapedValues: (number | null)[][] = [];
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
    // Everything is pre-computed by the UI service - just wire it into the view.
    this.sensors = preloaded.map(s => ({
      id: s.id,
      sensor: { id: s.id, name: s.name, sourceDeviceId: s.deviceId, metadata: [] } as Sensor,
      latestVal: s.latest ?? undefined,
    }));

    if (preloaded.length > 0) {
      this.device = { id: preloaded[0].deviceId, name: preloaded[0].deviceName } as Device;
      if (!this.isSoloSensor()) {
        this.showName = this.device.name;
      }
    }

    if (this.isSoloSensor()) {
      const s = preloaded[0];
      this.thresholds = s.thresholds ?? [];
      this.icons = s.icons ?? [];
      this.showName = s.showName ?? undefined;
      if (this.sensorInput) {
        this.sensorInput.visible = s.visible;
      }
      this.getAccentColor();
      this.setupChartOption();
      const points: UiPoint[] = s.values?.length
        ? s.values
        : (s.latest ? [[s.latest.timeCreated, s.latest.numericValue]] : []);
      this.renderValues(points);
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
          const meta = metadatas[0] ?? [];
          this.thresholds = meta
            .filter(m => m.name === "Threshold")
            .map(m => ({ min: m.minVal, max: m.maxVal, color: m.stringValue }));
          this.icons = meta.map(m => ({ name: m.name, stringValue: m.stringValue }));

          const nameMetadata = meta.find(m => m.name === "Name");
          if (nameMetadata && this.isSoloSensor()) {
            this.showName = nameMetadata.stringValue;
          }
          const visibilityMetadata = meta.find(m => m.name === "Visible");
          if (visibilityMetadata && this.sensorInput && this.isSoloSensor()) {
            this.sensorInput.visible = visibilityMetadata.numericValue > 0;
          }

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
    this.mapedValues = this.mapedValues.filter(p => (p[0] ?? 0) >= cutoff);

    this.updateChartWithThresholds();
  }

  private isSoloSensor() {
    return this.sensorInput?.id.length == 1;
  }

  private GenerateChart(x: Sensor) {
    // HTTP fallback (used only when the cube was not preloaded, e.g. an inactive sensor).
    this.setupChartOption();
    this.be.getUiSensorValues(x.id).subscribe(values =>
      this.renderValues(values.map(v => [v.timeCreated, v.numericValue] as UiPoint))
    );
  }

  private setupChartOption() {
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

  private renderValues(points: UiPoint[]) {
    this.valsReady = points.length > 0;
    if (!this.valsReady) {
      return;
    }

    let previousTick = new Date().getTime() - 86400000;
    let previousValue = points[0][1];

    this.mapedValues = [];

    points.forEach(point => {
      const time = point[0];
      for (let i = previousTick; i < time; i = i + 30000) {
        this.mapedValues.push([i, previousValue]);
      }
      previousValue = point[1];
      previousTick = time;
    });

    let now = new Date().getTime();
    for (let i = previousTick; i <= now; i = i + 30000) {
      this.mapedValues.push([i, previousValue]);
    }

    this.updateChartWithThresholds();
  }

  private updateChartWithThresholds() {
    if (!this.isSoloSensor()) return;

    let thresholdPieces = this.thresholds
      .map(t => ({
        gt: t.min,
        lte: t.max,
        color: t.color
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

  private getAccentColor() {
    const value = this.sensors[0]?.latestVal?.numericValue;
    if (this.isSoloSensor() && value != null) {
      const threshold = this.thresholds.find(t => t.max >= value && t.min < value);
      if (threshold) {
        this.accentColor = threshold.color;
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

  addData(newVal: UiValue | undefined) {
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
        data: { sensor: this.sensors[0].sensor, thresholds: this.thresholds, latestVal: this.sensors[0].latestVal },
        panelClass: "sensor-detail-panel"
      });
    }
  }

  SourceClick() {
    this.router.navigate(['/device/' + this.device?.id]);
  }
}
