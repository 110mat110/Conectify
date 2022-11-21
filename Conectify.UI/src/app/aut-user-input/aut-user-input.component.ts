import { Overlay, OverlayConfig } from '@angular/cdk/overlay';
import { ComponentPortal } from '@angular/cdk/portal';
import { Component, Input, OnInit, ViewContainerRef } from '@angular/core';
import { UserInputRule } from 'src/models/Automatization/UserInputRule';
import { ValueInitRule } from 'src/models/Automatization/ValueInitRule';
import { BefetchAutomatizationService } from '../befetch-automatization.service';
import { MessagesService } from '../messages.service';
import { SelectDestinationActuatorOverlayComponent } from '../select-destination-actuator-overlay/select-destination-actuator-overlay.component';

@Component({
  selector: 'app-aut-user-input',
  templateUrl: './aut-user-input.component.html',
  styleUrls: ['./aut-user-input.component.css']
})
export class AutUserInputComponent implements OnInit {
  @Input() Rule?: UserInputRule;
  overlayRef: any;

  constructor(  private be: BefetchAutomatizationService, public messenger: MessagesService, public overlay: Overlay, public viewContainerRef: ViewContainerRef) {

  }
  ngOnInit(): void {
  }

  public closeOverlay(){
    this.overlayRef.dispose();
  }

  SelectSource(){
    let config = new OverlayConfig();

    config.positionStrategy = this.overlay.position()
        .global().centerHorizontally();

    config.hasBackdrop = true;
    config.width = "500px";
    config.height = "500px";

    this.overlayRef  = this.overlay.create(config);
    this.overlayRef.backdropClick().subscribe(() => {
      this.overlayRef.dispose();
    });
  
    let ref = this.overlayRef.attach(new ComponentPortal(SelectDestinationActuatorOverlayComponent, this.viewContainerRef));
    ref.instance.creatorComponent = this;
    // if(this.Rule){
    //   this.Rule.destinationId = "c524bd2f-7124-436b-aa7e-12d8ef5ad8aa";
    // }
  }

}
