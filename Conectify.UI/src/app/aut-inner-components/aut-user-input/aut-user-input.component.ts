import { Component, Input, OnInit, ViewContainerRef } from '@angular/core';
import { UserInputRule } from 'src/models/Automatization/UserInputRule';
import { MatDialog } from '@angular/material/dialog';
import { Actuator } from 'src/models/actuator';
import { BefetchAutomatizationService } from 'src/app/befetch-automatization.service';
import { MessagesService } from 'src/app/messages.service';
import { SelectDestinationActuatorOverlayComponent } from 'src/app/select-destination-actuator-overlay/select-destination-actuator-overlay.component';

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
