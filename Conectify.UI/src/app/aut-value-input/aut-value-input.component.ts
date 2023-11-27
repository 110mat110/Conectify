import { Overlay, OverlayConfig } from '@angular/cdk/overlay';
import { ComponentPortal, PortalInjector } from '@angular/cdk/portal';
import { Component, Injectable, InjectionToken, Injector, Input, OnInit, ViewContainerRef } from '@angular/core';
import { ValueInitRule } from 'src/models/Automatization/ValueInitRule';
import { BefetchAutomatizationService } from '../befetch-automatization.service';
import { MessagesService } from '../messages.service';
import { SelectInputSensorOverlayComponent } from '../select-input-sensor-overlay/select-input-sensor-overlay.component';
import { MatDialog } from '@angular/material/dialog';
import { Sensor } from 'src/models/sensor';

@Component({
  selector: 'app-aut-value-input',
  templateUrl: './aut-value-input.component.html',
  styleUrls: ['./aut-value-input.component.css']
})
export class AutValueInputComponent implements OnInit  {
  @Input() Rule?: ValueInitRule;

  constructor(  private be: BefetchAutomatizationService, public messenger: MessagesService, public dialog: MatDialog) {

  }
  ngOnInit(): void {
    
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
