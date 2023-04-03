import { Overlay, OverlayConfig } from '@angular/cdk/overlay';
import { ComponentPortal } from '@angular/cdk/portal';
import { isNull } from '@angular/compiler/src/output/output_ast';
import { Component, ElementRef, Input, OnChanges, OnInit, SimpleChanges, ViewChild, ViewContainerRef } from '@angular/core';
import { BaseInputType } from 'src/models/extendedValue';
import { Metadata } from 'src/models/metadata';
import { Sensor } from 'src/models/sensor';
import { Device } from 'src/models/thing';
import { BEFetcherService } from '../befetcher.service';
import { MessagesService } from '../messages.service';
import { SensorDetailComponent } from '../sensor-detail/sensor-detail.component';
import { WebsocketService } from '../websocket.service';

@Component({
  selector: 'app-sensor-cube',
  templateUrl: './sensor-cube.component.html',
  styleUrls: ['./sensor-cube.component.css']
})
export class SensorCubeComponent implements OnInit, OnChanges {

  @Input() sensorInput?: { id: string, visible: boolean };
  public sensor?: Sensor;
  public device?: Device;
  //public values: BaseInputType[] = [];
  public latestVal?: BaseInputType;
  public metadatas: Metadata[] = [];
  public valsReady: boolean = false;
  mapedValues: (number | number)[][] = [];

  mergeOptions = {};
  chartOption: any = {};

  constructor(public messenger: MessagesService, private be: BEFetcherService, public overlay: Overlay, public viewContainerRef: ViewContainerRef, private websocketService: WebsocketService) {
  }

  HandleIncomingValue(msg: any): void {
    var id = msg.sourceId;
    if (id && this.sensor && id == this.sensor.id) {
      this.messenger.addMessage("Got value from ws:");
      this.latestVal = msg;
      //this.values.push(msg);
      this.addData(this.latestVal);
    }
  }

  onDetailsClick(): void {
  }

  ngOnChanges(changes: SimpleChanges): void {
  }

  ngOnInit(): void {
    this.websocketService.receivedMessages.subscribe(msg => {
      this.messenger.addMessage("cube has value");
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
    this.messenger.addMessage("Clicked to div in cube");
    this.openOverlay();
  }

  // getChart(): number[] {
  //   return this.values.map(x => x.numericValue);
  // }

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
    let config = new OverlayConfig();

    config.positionStrategy = this.overlay.position()
      .global().centerHorizontally().centerVertically();

    config.hasBackdrop = true;
    config.width = "500px";

    let overlayRef = this.overlay.create(config);
    overlayRef.backdropClick().subscribe(() => {
      overlayRef.dispose();
    });

    let ref = overlayRef.attach(new ComponentPortal(SensorDetailComponent, this.viewContainerRef));
    ref.instance.sensor = this.sensor;
  }
}
