import { Overlay, OverlayConfig } from '@angular/cdk/overlay';
import { ComponentPortal, PortalInjector } from '@angular/cdk/portal';
import { Component, Injectable, InjectionToken, Injector, Input, OnInit, ViewContainerRef } from '@angular/core';
import { ValueInitRule } from 'src/models/Automatization/ValueInitRule';
import { BefetchAutomatizationService } from '../befetch-automatization.service';
import { MessagesService } from '../messages.service';
import { SelectInputSensorOverlayComponent } from '../select-input-sensor-overlay/select-input-sensor-overlay.component';

@Component({
  selector: 'app-aut-value-input',
  templateUrl: './aut-value-input.component.html',
  styleUrls: ['./aut-value-input.component.css']
})
export class AutValueInputComponent implements OnInit  {
  @Input() SourceRule?: ValueInitRule;
  overlayRef: any;

  constructor(  private be: BefetchAutomatizationService, public messenger: MessagesService, public overlay: Overlay, public viewContainerRef: ViewContainerRef) {

  }
  ngOnInit(): void {
  }

  public saveClick(){
    if(this.SourceRule)
      this.be.saveInputRule(this.SourceRule); 
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
  
    let ref = this.overlayRef.attach(new ComponentPortal(SelectInputSensorOverlayComponent, this.viewContainerRef));
    ref.instance.creatorComponent = this;
  }
}
