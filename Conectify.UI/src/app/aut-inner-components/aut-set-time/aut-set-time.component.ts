import { Component, Input, OnInit } from '@angular/core';
import { BefetchAutomatizationService } from 'src/app/befetch-automatization.service';
import { MessagesService } from 'src/app/messages.service';
import { SetTimeRule } from 'src/models/Automatization/SetTimeRule';

@Component({
  selector: 'app-aut-set-time',
  templateUrl: './aut-set-time.component.html',
  styleUrls: ['./aut-set-time.component.css']
})
export class AutSetTimeComponent implements OnInit {
  @Input() Rule?: SetTimeRule;
  timeset: string = "12:00";
  days: string[] = ["Mo", "Tu", "We", "Th", "Fr", "Sa", "Su"];
  colorMap: { [key: string]: boolean } = {};

  constructor(  private be: BefetchAutomatizationService, public messenger: MessagesService) {
    this.days.forEach(day => {
      this.colorMap[day] = false;
    });
  }
  ngOnInit(): void {
    if(this.Rule?.behaviour){
      const utcDate = new Date(this.Rule.behaviour.TimeSet);
      const hours = utcDate.getHours().toString().padStart(2, '0');
      const minutes = utcDate.getMinutes().toString().padStart(2, '0');
      const seconds = utcDate.getSeconds().toString().padStart(2, '0');
      this.timeset = `${hours}:${minutes}:${seconds}`;
      this.Rule.behaviour.Days.split(',').forEach(day => {
        this.colorMap[day] = true;
      });
    }
  }

  onSelectedTimeChange(newTime: string) {
    if(this.Rule?.behaviour){
      const currentDate = new Date();
      const timeParts = newTime.split(':');
      currentDate.setHours(Number(timeParts[0]));
      currentDate.setMinutes(Number(timeParts[1]));
      currentDate.setSeconds(Number(timeParts[2]));
      this.Rule.behaviour.TimeSet = currentDate.toISOString();
    }
  }

  toggleColor(day: string) {
    this.colorMap[day] = !this.colorMap[day];

    if(this.Rule){
      this.Rule.behaviour.Days = this.getGreenLabeledDays();
    }
  }

  getGreenLabeledDays(): string {
    const greenDays: string[] = [];

    for (const day of this.days) {
      if (this.colorMap[day]) {
        greenDays.push(day);
      }
    }

    return greenDays.join(',');
  }

  getColor(day: string): string {
    // Get the current text color for the day
    return this.colorMap[day] ? 'darkgreen' : 'red'
  }
}
