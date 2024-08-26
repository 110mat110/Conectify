import { Component, HostListener, OnInit, ViewChild, ViewContainerRef } from '@angular/core';
import { BEFetcherService } from '../befetcher.service';
import { MessagesService } from '../messages.service';
import { fromEvent } from 'rxjs';
import { debounceTime } from 'rxjs/operators';

@Component({
  selector: 'app-sensor-overview',
  templateUrl: './sensor-overview.component.html',
  styleUrls: ['./sensor-overview.component.css']
})
export class SensorOverviewComponent implements OnInit {
  public sensors: {id: string, visible: boolean}[] = [];
  columnNum: number = 4;
  tileSize: number = 400;
  @ViewChild('theContainer') theContainer: any;

  constructor(private be: BEFetcherService, private messanger: MessagesService ) { 
  }
  ngAfterViewInit(): void {
    this.calculateCols();
  
    fromEvent(window, 'resize')
      .pipe(debounceTime(100))
      .subscribe(() => this.calculateCols());
  }

  ngOnInit(): void {
    this.be.getActiveSensors().subscribe(x => {
      x.forEach(s => this.sensors.push({id: s, visible: true}));
      this.calculateCols();
    }, (err) => {
      this.messanger.addMessage("Error!");
      this.messanger.addMessage(JSON.stringify(err));
  });
  }
  calculateCols(){
    let width = this.theContainer.nativeElement.offsetWidth;
    this.columnNum = Math.trunc(width/this.tileSize);
  }
}
