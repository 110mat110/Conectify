import { Overlay, OverlayConfig } from '@angular/cdk/overlay';
import { ComponentPortal } from '@angular/cdk/portal';
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

  @Input() sensorInput?: {id:string, visible: boolean};
  public sensor?: Sensor;
  public device?: Device;
  public values: BaseInputType[] = [];
  public latestVal?: BaseInputType;
  public metadatas: Metadata[] = [];
  public valsReady: boolean = false;
  public mapedValues: number[] = [];
  
  mergeOptions = {};
  chartOption: any = {};

  constructor(public messenger: MessagesService, private be: BEFetcherService, public overlay: Overlay, public viewContainerRef: ViewContainerRef, private websocketService: WebsocketService) {
  }

  HandleIncomingValue(msg: any) : void{
    var id = msg.sourceId;
    if(id && this.sensor && id == this.sensor.id){
      this.messenger.addMessage("Got value from ws:");
      this.latestVal = msg;
      this.values.push(msg);
      this.addData(this.latestVal?.numericValue ?? 0);
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
            if(visibilityMetadata && this.sensorInput){
              this.sensorInput.visible = visibilityMetadata.numericValue > 0;
            }
          });
          this.be.getSensorValues(this.sensor.id).subscribe(
            x => {
              this.values = x;
              this.valsReady = this.values.length > 0;
              if (this.valsReady) {
                this.mapedValues = this.getChart();
              }
            });
          this.be.getLatestSensorValue(this.sensor.id).subscribe(x =>{
            this.latestVal = x;
            this.addData(this.latestVal.numericValue);
          });
          this.chartOption = {
            xAxis: {
              type: 'category',
            },
            yAxis: {
              type: 'value',
              show: false
            },
            series: [{
              name: this.sensor?.name,
              data: this.mapedValues,
              type: 'line',
              symbolKeepAspect: false,
            }]
          }
        }
      });
    }
  }

  public klikaj() {
    this.messenger.addMessage("Clicked to div in cube");
    this.openOverlay();
  }

  getChart(): number[] {
    return this.values.map(x => x.numericValue);
  }

  addData(newVal: number) {
    let newData = this.mapedValues;

    newData.push(newVal);

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
