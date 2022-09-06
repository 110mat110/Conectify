import { Overlay, OverlayConfig } from '@angular/cdk/overlay';
import { ComponentPortal } from '@angular/cdk/portal';
import { ViewContainerRef } from '@angular/core';
import { Component, Input, OnInit } from '@angular/core';
import { ChangeDestinationRule } from 'src/models/Automatization/ChangeDestinationRule';
import { BefetchAutomatizationService } from '../befetch-automatization.service';
import { MessagesService } from '../messages.service';
import { SelectDestinationActuatorOverlayComponent } from '../select-destination-actuator-overlay/select-destination-actuator-overlay.component';

@Component({
  selector: 'app-aut-change-destination',
  templateUrl: './aut-change-destination.component.html',
  styleUrls: ['./aut-change-destination.component.css']
})
export class AutChangeDestinationComponent implements OnInit {

  @Input() Rule?: ChangeDestinationRule;
  overlayRef: any;

  constructor(  private be: BefetchAutomatizationService, public messenger: MessagesService, public overlay: Overlay, public viewContainerRef: ViewContainerRef) {

  }

  ngOnInit(): void {
    console.warn("Change dest rule init")
  }

  closeOverlay(){
    this.overlayRef.dispose();
  }

  selectDestination(){
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
  }

  saveClick(){
    if(this.Rule)
      this.be.saveChangeDestRule(this.Rule);
  }

}
