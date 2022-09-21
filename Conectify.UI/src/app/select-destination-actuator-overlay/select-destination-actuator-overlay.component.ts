import { HostListener, Input, ViewChild } from '@angular/core';
import { Component, OnInit } from '@angular/core';
import { Actuator } from 'src/models/actuator';
import { AutChangeDestinationComponent } from '../aut-change-destination/aut-change-destination.component';
import { BEFetcherService } from '../befetcher.service';
import { MessagesService } from '../messages.service';

@Component({
  selector: 'app-select-destination-actuator-overlay',
  templateUrl: './select-destination-actuator-overlay.component.html',
  styleUrls: ['./select-destination-actuator-overlay.component.css']
})
export class SelectDestinationActuatorOverlayComponent implements OnInit {

  @Input() creatorComponent?: AutChangeDestinationComponent;
  actuators: Actuator[] = [];
  columnNum: number = 4;
  tileSize: number = 400;
  @ViewChild('theContainer') theContainer: any;

  constructor(
    private be: BEFetcherService,
    private messanger: MessagesService,
  ) {}

  selectedActuator(id: string){
    if(this.creatorComponent && this.creatorComponent.Rule){
      this.creatorComponent.Rule.destinationId = id as string;
      this.creatorComponent.closeOverlay();
    }
  }

  ngOnInit(): void {
    this.be.getAllActuators().subscribe(
      (x) => {
        this.actuators = x;
        this.setColNum();
      },
      (err) => {
        this.messanger.addMessage('Error!');
        this.messanger.addMessage(JSON.stringify(err));
      }
    );
  }
  setColNum() {
    this.columnNum=2;
    /*
    let width = this.theContainer.nativeElement.offsetWidth;
    this.columnNum = Math.trunc(width / this.tileSize);
    */
  }

  //recalculating upon browser window resize
  @HostListener('window:resize', ['$event'])
  onResize(event: any) {
    this.setColNum();
  }
}
