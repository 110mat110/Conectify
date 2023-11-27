import { Component, Inject, Input, OnInit } from '@angular/core';
import { BaseInputType } from 'src/models/extendedValue';
import { Sensor } from 'src/models/sensor';
import { Device } from 'src/models/thing';
import { BEFetcherService } from '../befetcher.service';
import { MessagesService } from '../messages.service';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';

@Component({
  selector: 'app-sensor-detail',
  templateUrl: './sensor-detail.component.html',
  styleUrls: ['./sensor-detail.component.css']
})
export class SensorDetailComponent implements OnInit {

  chartOption: any;
  mapedValues: (string | number)[][] = [];
  public latestVal?: BaseInputType;
  public latestValTime?: string;

  constructor(public messenger: MessagesService, private be: BEFetcherService,@Inject(MAT_DIALOG_DATA) public data: {sensor: Sensor}) { }

  ngOnInit(): void {
    if (this.data.sensor) {
      this.be.getSensorValues(this.data.sensor.id).subscribe(
        values => {
          if (values.length > 0) {
            let startOfGraph = new Date().getTime() - 86400000;
            let previousTick = startOfGraph;
            let previousValue = values[0].numericValue;
            values.forEach(value => {
              if (value.timeCreated > startOfGraph) {
                for (let i = previousTick; i < value.timeCreated; i = i + 10000) {
                  this.mapedValues.push([new Date(i).toLocaleTimeString(), previousValue]);
                }
                previousValue = value.numericValue;
                previousTick = value.timeCreated;
              }
            });
            this.setOptions();
          }
        });
      this.be.getLatestSensorValue(this.data.sensor.id).subscribe(
        x => {
          this.latestVal = x;
          this.latestValTime = new Date(x.timeCreated).toLocaleTimeString()
        }
      )
    }
  }

  setOptions() {
    const distanceFromEdge = 50;
    this.chartOption = {
      outerWidth: window.innerWidth - 2 * distanceFromEdge,
      xAxis: {
        type: 'category',
        valueType: 'DateTime',
        autoTick: 'true'
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
