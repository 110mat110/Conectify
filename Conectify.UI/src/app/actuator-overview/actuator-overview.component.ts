import { Component, HostListener, OnInit, ViewChild } from '@angular/core';
import { BEFetcherService } from '../befetcher.service';
import { MessagesService } from '../messages.service';
import { UiActuator } from 'src/models/UiActuator';

@Component({
  selector: 'app-actuator-overview',
  templateUrl: './actuator-overview.component.html',
  styleUrls: ['./actuator-overview.component.css']
})
export class ActuatorOverviewComponent implements OnInit {
  columnNum: number = 4;
  tileSize: number = 400;
  @ViewChild('theContainer') theContainer: any;
  public actuatorIds: {id: string, visible: boolean, preloaded?: UiActuator}[] = [];

  constructor(private be: BEFetcherService, private messanger: MessagesService) { }

  ngOnInit(): void {
    this.be.getUiActuators().subscribe({
      next: (actuators) => {
        actuators.forEach(a => this.actuatorIds.push({ id: a.id, visible: true, preloaded: a }));
        this.setColNum();
      },
      error: (err) => {
        this.messanger.addMessage("Error!");
        this.messanger.addMessage(JSON.stringify(err));
      }
    });
  }

  setColNum(){
    let width = this.theContainer.nativeElement.offsetWidth;
    this.columnNum = Math.trunc(width/this.tileSize);
  }

  @HostListener('window:resize', ['$event'])
  onResize(event: any) {
    this.setColNum();
  }
}
