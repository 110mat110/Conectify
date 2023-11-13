import { Component, Input, OnInit } from '@angular/core';
import { ChangeDestinationRule } from 'src/models/Automatization/ChangeDestinationRule';
import { EditRule } from 'src/models/Automatization/EditRule';
import { UserInputRule } from 'src/models/Automatization/UserInputRule';
import { ValueInitRule } from 'src/models/Automatization/ValueInitRule';
import { AutomatizationBase } from 'src/models/automatizationComponent';
import { AutomatizationComponent } from '../automatization/automatization.component';
import { BefetchAutomatizationService } from '../befetch-automatization.service';
import { MessagesService } from '../messages.service';
import { SetTimeRule } from 'src/models/Automatization/SetTimeRule';
import { SetValueRule } from 'src/models/Automatization/SetValueRule';
import { DecisionRule } from 'src/models/Automatization/DecisionRule';
import { AndRule } from 'src/models/Automatization/AndRule';

@Component({
  selector: 'app-automatization-component',
  templateUrl: './automatization-component.component.html',
  styleUrls: ['./automatization-component.component.css']
})
export class AutomatizationComponentComponent implements OnInit {

  @Input() Component?: AutomatizationBase;
  @Input() ComponentCage?: AutomatizationComponent;
  ValueInitRule?: ValueInitRule;
  ChangeDestinationRule?: ChangeDestinationRule;
  UserInputRule?: UserInputRule;
  SetTimeRule?: SetTimeRule;
  SetValueRule?: SetValueRule;
  DecisionRule?: DecisionRule;
  AndRule?: AndRule;
  isSource: boolean = false;
  isDestination: boolean = false;
  hasParameters: boolean = false;
  constructor(public messenger: MessagesService, public be: BefetchAutomatizationService) { }

  ngOnInit(): void {
    if(this.Component instanceof ValueInitRule){
      this.ValueInitRule = this.Component;
      this.isDestination = false;
      this.isSource = true;
    }
    if(this.Component instanceof ChangeDestinationRule){
      this.ChangeDestinationRule = this.Component;
      this.isDestination = true;
      this.isSource = true;
    }

    if(this.Component instanceof UserInputRule){
      this.UserInputRule = this.Component;
      this.isDestination = false;
      this.isSource = true;
    }

    if(this.Component instanceof SetTimeRule){
      this.SetTimeRule = this.Component;
      this.isDestination = false;
      this.isSource = true;
    }

    if(this.Component instanceof SetValueRule){
      this.SetValueRule = this.Component;
      this.isDestination = true;
      this.isSource = true;
    }

    if(this.Component instanceof DecisionRule){
      this.DecisionRule = this.Component;
      this.isDestination = true;
      this.isSource = true;
      this.hasParameters = true;
    }

    if(this.Component instanceof AndRule){
      this.AndRule = this.Component;
      this.isDestination = true;
      this.isSource = true;
      this.hasParameters = false;
    }
  }

  SourceClick(){
    if(this.ComponentCage && this.Component )//&& this.Component instanceof AutomatizationBaseWithTargetGeneric ) //Check this twice!
      this.ComponentCage.SourceClick(this.Component);
  }

  DestinationClick(){
    if(this.ComponentCage && this.Component)
    this.ComponentCage.DestinationClick(this.Component);
  }

  ParameterClick(){
    if(this.ComponentCage && this.Component)
    this.ComponentCage.ParameterClick(this.Component);
  }

  public saveClick(){
    if(this.Component){
      let apiModel: EditRule = {id: this.Component.id, x: this.Component.dragPosition.x, y: this.Component.dragPosition.y, behaviourId: this.Component.behaviorId, parameters: this.Component.getParametersJSon() };
      this.be.saveRule(apiModel); 
    }
  }
}
