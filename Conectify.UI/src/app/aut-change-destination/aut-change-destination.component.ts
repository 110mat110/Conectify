import { Overlay, OverlayConfig } from '@angular/cdk/overlay';
import { ComponentPortal } from '@angular/cdk/portal';
import { ViewContainerRef } from '@angular/core';
import { Component, Input, OnInit } from '@angular/core';
import { ChangeDestinationRule } from 'src/models/Automatization/ChangeDestinationRule';
import { BefetchAutomatizationService } from '../befetch-automatization.service';
import { MessagesService } from '../messages.service';
import { SelectDestinationActuatorOverlayComponent } from '../select-destination-actuator-overlay/select-destination-actuator-overlay.component';
import { MatDialog } from '@angular/material/dialog';
import { Actuator } from 'src/models/actuator';

@Component({
  selector: 'app-aut-change-destination',
  templateUrl: './aut-change-destination.component.html',
  styleUrls: ['./aut-change-destination.component.css']
})
export class AutChangeDestinationComponent implements OnInit {

  @Input() Rule?: ChangeDestinationRule;
  overlayRef: any;

  constructor(  private be: BefetchAutomatizationService, public messenger: MessagesService, public overlay: Overlay, public dialog: MatDialog) {

  }

  ngOnInit(): void {
    console.warn("Change dest rule init")
  }

  closeOverlay(){
    this.overlayRef.dispose();
  }

  selectDestination(){
    const dialogRef = this.dialog.open(SelectDestinationActuatorOverlayComponent, {
      width: '50%',
      height: '100%',
      restoreFocus: false
    });

    dialogRef.afterClosed().subscribe(result => {
      var actuator = result as Actuator;
      if(this.Rule?.behaviour && actuator){
        this.Rule.behaviour.DestinationId = actuator.id;
      }
    });
  }

}
