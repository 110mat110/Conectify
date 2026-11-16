import { Component, OnInit } from '@angular/core';
import { BEFetcherService } from '../befetcher.service';
import { MessagesService } from '../messages.service';
import { UiActuator } from 'src/models/UiActuator';

@Component({
  selector: 'app-actuator-overview',
  templateUrl: './actuator-overview.component.html',
  styleUrls: ['./actuator-overview.component.css']
})
export class ActuatorOverviewComponent implements OnInit {
  public actuatorIds: {id: string, visible: boolean, preloaded?: UiActuator}[] = [];

  constructor(private be: BEFetcherService, private messanger: MessagesService) { }

  ngOnInit(): void {
    this.be.getUiActuators().subscribe({
      next: (actuators) => {
        actuators.forEach(a => this.actuatorIds.push({ id: a.id, visible: true, preloaded: a }));
      },
      error: (err) => {
        this.messanger.addMessage("Error!");
        this.messanger.addMessage(JSON.stringify(err));
      }
    });
  }
}
