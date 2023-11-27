import { Overlay, OverlayConfig } from '@angular/cdk/overlay';
import { ComponentPortal } from '@angular/cdk/portal';
import { Component, Input, OnInit, ViewContainerRef } from '@angular/core';
import { UserInputRule } from 'src/models/Automatization/UserInputRule';
import { ValueInitRule } from 'src/models/Automatization/ValueInitRule';
import { BefetchAutomatizationService } from '../befetch-automatization.service';
import { MessagesService } from '../messages.service';
import { SelectDestinationActuatorOverlayComponent } from '../select-destination-actuator-overlay/select-destination-actuator-overlay.component';
import { MatDialog } from '@angular/material/dialog';
import { Actuator } from 'src/models/actuator';

@Component({
  selector: 'app-aut-user-input',
  templateUrl: './aut-user-input.component.html',
  styleUrls: ['./aut-user-input.component.css']
})
export class AutUserInputComponent implements OnInit {
  @Input() Rule?: UserInputRule;

  constructor(  private be: BefetchAutomatizationService, public messenger: MessagesService, public dialog: MatDialog) {

  }
  ngOnInit(): void {
  }
  
  SelectSource(){
    const dialogRef = this.dialog.open(SelectDestinationActuatorOverlayComponent, {
      width: '50%',
      height: '100%',
      restoreFocus: false
    });

    dialogRef.afterClosed().subscribe(result => {
      var actuator = result as Actuator;
      if(this.Rule?.behaviour && actuator){
        this.Rule.behaviour.SourceActuatorId = actuator.id;
      }
    });
  }
}
