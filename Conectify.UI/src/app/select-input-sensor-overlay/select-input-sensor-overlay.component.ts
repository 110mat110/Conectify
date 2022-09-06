import {
  Component,
  HostListener,
  Injector,
  Input,
  OnInit,
  ViewChild,
} from '@angular/core';
import { Sensor } from 'src/models/sensor';
import { AutValueInputComponent } from '../aut-value-input/aut-value-input.component';
import { BEFetcherService } from '../befetcher.service';
import { MessagesService } from '../messages.service';

@Component({
  selector: 'app-select-input-sensor-overlay',
  templateUrl: './select-input-sensor-overlay.component.html',
  styleUrls: ['./select-input-sensor-overlay.component.css'],
})
export class SelectInputSensorOverlayComponent implements OnInit {
  @Input() creatorComponent?: AutValueInputComponent;
  sensors: Sensor[] = [];
  columnNum: number = 4;
  tileSize: number = 400;
  @ViewChild('theContainer') theContainer: any;

  constructor(
    private be: BEFetcherService,
    private messanger: MessagesService,
    private inj: Injector
  ) {}

  selectedSensor(id: string){
    if(this.creatorComponent && this.creatorComponent.SourceRule){
      this.creatorComponent.SourceRule.sourceId = id as string;
      this.creatorComponent.closeOverlay();
    }
  }

  ngOnInit(): void {
    this.be.getSensors().subscribe(
      (x) => {
        this.sensors = x.entities;
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
