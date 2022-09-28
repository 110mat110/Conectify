import { Component, Input, OnChanges, OnInit, SimpleChanges } from '@angular/core';
import { BaseInputType } from 'src/models/extendedValue';
import { ChartConfiguration, ChartOptions, ChartType } from "chart.js";

@Component({
  selector: 'app-last-value',
  templateUrl: './last-value.component.html',
  styleUrls: ['./last-value.component.css']
})
export class LastValueComponent implements OnInit {
 @Input() values: BaseInputType[] = [];

 public lineChartData: ChartConfiguration<'line'>['data'] = {
  labels: [
    'January',
    'February',
    'March',
    'April',
    'May',
    'June',
    'July'
  ],
  datasets: [
    {
      data: this.values.map(x => x.numericValue),
      label: 'Series A',
      fill: true,
      tension: 0.5,
      borderColor: 'black',
      backgroundColor: 'rgba(255,0,0,0.3)'
    }
  ]
};
public lineChartOptions: ChartOptions<'line'> = {
  responsive: false
};
public lineChartLegend = false;

constructor() {
}

ngOnInit() {
}

}
