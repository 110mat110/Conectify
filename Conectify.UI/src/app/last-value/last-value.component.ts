import { Component, Input, OnChanges, OnInit, SimpleChanges } from '@angular/core';
import { BaseInputType } from 'src/models/extendedValue';
import { ChartConfiguration, ChartOptions, ChartType } from "chart.js";

@Component({
  selector: 'app-last-value',
  templateUrl: './last-value.component.html',
  styleUrls: ['./last-value.component.css']
})
export class LastValueComponent implements OnInit {
 @Input() chartOption: any;

constructor() {
}

ngOnInit() {
}

}
