import { Component, Input, OnInit } from '@angular/core';
import { BaseInputType } from 'src/models/extendedValue';
import { Sensor } from 'src/models/sensor';
import { Device } from 'src/models/thing';
import { BEFetcherService } from '../befetcher.service';
import { MessagesService } from '../messages.service';

@Component({
  selector: 'app-sensor-detail',
  templateUrl: './sensor-detail.component.html',
  styleUrls: ['./sensor-detail.component.css']
})
export class SensorDetailComponent implements OnInit {

  @Input() sensor?: Sensor;
  @Input() device?: Device;
  chartOption: any;
  values: BaseInputType[] = [];
  xAxis: string[] = [];
  mapedValues: number[] = [];
  public latestVal?: BaseInputType;
  public latestValTime?: string;
  
  constructor(public messenger: MessagesService, private be: BEFetcherService,) { }

  ngOnInit(): void {
    if(this.sensor){
    this.be.getSensorValues(this.sensor.id).subscribe(
      x =>{ this.values = x;
        if(this.values.length > 0){
          this.mapedValues = this.values.map(v => v.numericValue);
          this.xAxis = this.values.map(x => new Date(x.timeCreated).toLocaleTimeString());
          this.setOptions();
        }
    });
    this.be.getLatestSensorValue(this.sensor.id).subscribe(
      x=> {this.latestVal = x;
        this.latestValTime = new Date(x.timeCreated).toLocaleTimeString()
      }
    )
    }
  }

  setOptions() {

    this.chartOption = {
      xAxis: {
        type: 'category',
        data: this.xAxis,
      },
      tooltip: {
        trigger: 'axis'
      },
      yAxis: {
        type: 'value',
        axisLabel: {
          formatter: '{value}' + this.latestVal?.unit
      },
        axisPointer: {
          snap: true
      }
      },
      series: [{
        data: this.mapedValues,
        type: 'line',
        symbolKeepAspect: false,
      }]
    }
  }
}
