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
  selectedSensor: Sensor|null = null;
  sensors: {sensor: Sensor, color: string}[] = [];
  columnNum: number = 4;
  tileSize: number = 150;
  @ViewChild('theContainer') theContainer: any;

  constructor(
    private be: BEFetcherService,
    private messanger: MessagesService
  ) {}

  select(sensor:{sensor: Sensor, color: string}){
    this.sensors.forEach(sensor => sensor.color = "darkblue")

    this.selectedSensor = sensor.sensor;
    sensor.color = "green";
  }

  ngOnInit(): void {
    this.be.getAllSensors().subscribe(
      (x) => {
        this.sensors = x.map(val => { return {sensor: val, color: "darkblue"}});
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
