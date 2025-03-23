import { Component, Input, OnInit } from '@angular/core';
import { BefetchAutomatizationService } from 'src/app/befetch-automatization.service';
import { MessagesService } from 'src/app/messages.service';
import { DelayRule } from 'src/models/Automatization/DelayRule';

@Component({
  selector: 'app-aut-delay',
  templateUrl: './aut-delay.component.html',
  styleUrls: ['./aut-delay.component.css']
})
export class AutDelayComponent implements OnInit {
  @Input() Rule?: DelayRule;
  delay: string = "00:00:10";

  constructor(  private be: BefetchAutomatizationService, public messenger: MessagesService) {
  }
  ngOnInit(): void {
    if(this.Rule?.behaviour){
      const utcDate = new Date(this.Rule.behaviour.Delay);
      const hours = utcDate.getHours().toString().padStart(2, '0');
      const minutes = utcDate.getMinutes().toString().padStart(2, '0');
      const seconds = utcDate.getSeconds().toString().padStart(2, '0');
      this.delay = `${hours}:${minutes}:${seconds}`;
    }
  }

  onSelectedTimeChange(newTime: string) {
    if(this.Rule?.behaviour){
      this.Rule.behaviour.Delay = newTime;
    }
  }
}
