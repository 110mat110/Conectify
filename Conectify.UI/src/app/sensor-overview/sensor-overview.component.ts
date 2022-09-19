import { Component, HostListener, OnInit, ViewChild, ViewContainerRef } from '@angular/core';
import { AutValueInputComponent } from '../aut-value-input/aut-value-input.component';
import { BEFetcherService } from '../befetcher.service';
import { MessagesService } from '../messages.service';

@Component({
  selector: 'app-sensor-overview',
  templateUrl: './sensor-overview.component.html',
  styleUrls: ['./sensor-overview.component.css']
})
export class SensorOverviewComponent implements OnInit {
  public sensors: string[] = [];
  columnNum: number = 4;
  tileSize: number = 400;
  @ViewChild('theContainer') theContainer: any;

  constructor(private be: BEFetcherService, private messanger: MessagesService ) { 
  }

  ngOnInit(): void {
    this.be.getActiveSensors().subscribe(x => {
      this.messanger.addMessage("Subscribe was called");
      this.sensors = x;
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
