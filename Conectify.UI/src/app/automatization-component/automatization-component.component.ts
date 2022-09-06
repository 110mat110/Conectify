import { Component, Input, OnInit } from '@angular/core';
import { ChangeDestinationRule } from 'src/models/Automatization/ChangeDestinationRule';
import { MiddleRule } from 'src/models/Automatization/MiddleRule';
import { ValueInitRule } from 'src/models/Automatization/ValueInitRule';
import { AutomatizationBase, AutomatizationBaseWithTarget } from 'src/models/automatizationComponent';
import { AutomatizationComponent } from '../automatization/automatization.component';
import { BefetchAutomatizationService } from '../befetch-automatization.service';
import { MessagesService } from '../messages.service';

@Component({
  selector: 'app-automatization-component',
  templateUrl: './automatization-component.component.html',
  styleUrls: ['./automatization-component.component.css']
})
export class AutomatizationComponentComponent implements OnInit {

  @Input() Component?: AutomatizationBase;
  @Input() ComponentCage?: AutomatizationComponent;
  ValueInitComponent?: ValueInitRule;
  ChangeDestinationRule?: ChangeDestinationRule;
  isSource: boolean = false;
  isDestination: boolean = false;
  constructor(public messenger: MessagesService) { }

  ngOnInit(): void {
    if(this.Component instanceof ValueInitRule){
      this.ValueInitComponent = this.Component;
      this.isDestination = false;
      this.isSource = true;
    }
    if(this.Component instanceof MiddleRule){

      this.isDestination = true;
      this.isSource = true;
    }
    if(this.Component instanceof ChangeDestinationRule){
      this.ChangeDestinationRule = this.Component;

      console.warn("Found changeDest rule!")

      this.isDestination = true;
      this.isSource = true;
    }

  }



  SourceClick(){
    this.messenger.addMessage("sourceClick");
    if(this.ComponentCage && this.Component && this.Component instanceof AutomatizationBaseWithTarget )
      this.ComponentCage.SourceClick(this.Component);
  }

  DestinationClick(){
    this.messenger.addMessage("Dest click");
    if(this.ComponentCage && this.Component)
    this.ComponentCage.DestinationClick(this.Component);
  }
}
