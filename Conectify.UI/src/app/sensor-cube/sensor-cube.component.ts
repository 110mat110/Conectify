import { Overlay, OverlayConfig } from '@angular/cdk/overlay';
import { ComponentPortal } from '@angular/cdk/portal';
import { Component, Input, OnInit, ViewContainerRef } from '@angular/core';
import { connect } from 'echarts';
import { BaseInputType } from 'src/models/extendedValue';
import { Metadata } from 'src/models/metadata';
import { Sensor } from 'src/models/sensor';
import { Device } from 'src/models/thing';
import { AutValueInputComponent } from '../aut-value-input/aut-value-input.component';
import { BEFetcherService } from '../befetcher.service';
import { MessagesService } from '../messages.service';
import { SensorDetailComponent } from '../sensor-detail/sensor-detail.component';

@Component({
  selector: 'app-sensor-cube',
  templateUrl: './sensor-cube.component.html',
  styleUrls: ['./sensor-cube.component.css']
})
export class SensorCubeComponent implements OnInit {

  @Input() sensorInput?: {id:string, visible: boolean};
  public sensor?: Sensor;
  public device?: Device;
  public values: BaseInputType[] = [];
  public latestVal?: BaseInputType;
  public mapedValues: number[] = [];
  public metadatas: Metadata[] = [];
  public valsReady: boolean = false;
  chartOption: any;

  constructor(public messenger: MessagesService, private be: BEFetcherService, public overlay: Overlay, public viewContainerRef: ViewContainerRef) {
    this.setOptions();
  }

  onDetailsClick(): void {
  }

  ngOnInit(): void {
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
                this.setOptions();
              }
            });
          this.be.getLatestSensorValue(this.sensor.id).subscribe(x => this.latestVal = x);
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

  setOptions() {

    this.chartOption = {
      xAxis: {
        type: 'category',
      },
      yAxis: {
        type: 'value',
        show: false
      },
      series: [{
        data: this.mapedValues,
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
