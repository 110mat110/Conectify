import { HostListener, Input, ViewChild } from '@angular/core';
import { Component, OnInit } from '@angular/core';
import { Actuator } from 'src/models/actuator';
import { AutChangeDestinationComponent } from '../aut-change-destination/aut-change-destination.component';
import { BEFetcherService } from '../befetcher.service';
import { MessagesService } from '../messages.service';

@Component({
  selector: 'app-',
  templateUrl: './select-destination-actuator-overlay.component.html',
  styleUrls: ['./select-destination-actuator-overlay.component.css']
})
export class SelectDestinationActuatorOverlayComponent implements OnInit {
  actuators: {actuator: Actuator, color: string}[] = [];
  selectedActuator: Actuator|null = null;
  columnNum: number = 4;
  tileSize: number = 150;
  @ViewChild('theContainer') theContainer: any;

  constructor(
    private be: BEFetcherService,
    private messanger: MessagesService,
  ) {}

  select(actuator:{actuator: Actuator, color: string}){
    this.actuators.forEach(actuator => actuator.color = "darkblue")

    this.selectedActuator = actuator.actuator;
    actuator.color = "green";
  }

  ngOnInit(): void {
    this.be.getAllActuators().subscribe(
      (x) => {
        this.actuators = x.map(val => { return {actuator: val, color: "darkblue"}});
        this.setColNum();
      },
      (err) => {
        this.messanger.addMessage('Error!');
        this.messanger.addMessage(JSON.stringify(err));
      }
    );
  }
  setColNum() {
    let width = this.theContainer.nativeElement.offsetWidth;
    this.columnNum = Math.trunc(width / this.tileSize);
  }

  //recalculating upon browser window resize
  @HostListener('window:resize', ['$event'])
  onResize(event: any) {
    this.setColNum();
  }
}
