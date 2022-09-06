import { Component, Input, OnInit } from '@angular/core';
import { BaseInputType } from 'src/models/extendedValue';
import { Sensor } from 'src/models/sensor';
import { BEFetcherService } from '../befetcher.service';
import { MessagesService } from '../messages.service';

@Component({
  selector: 'app-sensor-detail',
  templateUrl: './sensor-detail.component.html',
  styleUrls: ['./sensor-detail.component.css']
})
export class SensorDetailComponent implements OnInit {

  @Input() sensor?: Sensor;
  chartOption: any;
  values: BaseInputType[] = [];
  xAxis: string[] = [];
  mapedValues: number[] = [];
  public latestVal?: BaseInputType;
  
  constructor(public messenger: MessagesService, private be: BEFetcherService,) { }

  ngOnInit(): void {
    if(this.sensor){
    this.be.getSensorValues(this.sensor.id).subscribe(
      x =>{ this.values = x.entities;
        if(this.values.length > 0){
          this.mapedValues = this.values.map(v => v.numericValue);
          this.xAxis = this.values.map(x => this.trimTimeString(x.timeCreated));
          this.setOptions();
        }
    });
    this.be.getLatestSensorValue(this.sensor.id).subscribe(x => this.latestVal = x.entity);
  }
  }

  trimTimeString(timeString: string): string{
    return timeString.split("T")[1].split(".")[0];
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
