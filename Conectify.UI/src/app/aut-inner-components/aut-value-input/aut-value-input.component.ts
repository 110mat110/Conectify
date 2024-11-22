import { Component, Injectable, InjectionToken, Injector, Input, OnInit, ViewContainerRef } from '@angular/core';
import { ValueInitRule } from 'src/models/Automatization/ValueInitRule';
import { MatDialog } from '@angular/material/dialog';
import { Sensor } from 'src/models/sensor';
import { BefetchAutomatizationService } from 'src/app/befetch-automatization.service';
import { MessagesService } from 'src/app/messages.service';
import { SelectInputSensorOverlayComponent } from 'src/app/select-input-sensor-overlay/select-input-sensor-overlay.component';

@Component({
  selector: 'app-aut-value-input',
  templateUrl: './aut-value-input.component.html',
  styleUrls: ['./aut-value-input.component.css']
})
export class AutValueInputComponent implements OnInit  {
  @Input() Rule?: ValueInitRule;
  eventtype: string = "all";
  constructor(  private be: BefetchAutomatizationService, public messenger: MessagesService, public dialog: MatDialog) {

  }
  ngOnInit(): void {
    if(this.Rule?.behaviour){
      this.eventtype = this.Rule.behaviour.Event;
    }
  }

  onChange(): void{
    if(this.Rule?.behaviour){
      this.Rule.behaviour.Event = this.eventtype;
    }
  }

  SelectSource(){
    const dialogRef = this.dialog.open(SelectInputSensorOverlayComponent, {
      width: '50%',
      height: '100%',
      restoreFocus: false
    });

    dialogRef.afterClosed().subscribe(result => {
      var sensor = result as Sensor;
      if(this.Rule?.behaviour && sensor){
        this.Rule.behaviour.SourceSensorId = sensor.id;
      }
    });
  }
}
