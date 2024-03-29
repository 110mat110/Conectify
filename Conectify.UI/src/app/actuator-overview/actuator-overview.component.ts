import { Component, HostListener, OnInit, ViewChild } from '@angular/core';
import { Actuator } from 'src/models/actuator';
import { BEFetcherService } from '../befetcher.service';
import { MessagesService } from '../messages.service';

@Component({
  selector: 'app-actuator-overview',
  templateUrl: './actuator-overview.component.html',
  styleUrls: ['./actuator-overview.component.css']
})
export class ActuatorOverviewComponent implements OnInit {
  columnNum: number = 4;
  tileSize: number = 400;
  @ViewChild('theContainer') theContainer: any;
  public actuatorIds: {id: string, visible: boolean}[]= [];
  constructor(private be: BEFetcherService, private messanger: MessagesService) { }

  ngOnInit(): void {
    this.be.getActiveActuatorsIds().subscribe(x => {
      x.forEach(id => this.actuatorIds.push({id: id, visible: true}));
      this.setColNum();
    }, (err) => {
      this.messanger.addMessage("Error!");
      this.messanger.addMessage(JSON.stringify(err));
  });
  }

  setColNum(){
    let width = this.theContainer.nativeElement.offsetWidth;
    this.columnNum = Math.trunc(width/this.tileSize);
  }

      //recalculating upon browser window resize
      @HostListener('window:resize', ['$event'])
      onResize(event: any) {
        this.setColNum();
      }

}
