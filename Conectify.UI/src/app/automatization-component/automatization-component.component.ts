import { AfterViewInit, Component, ElementRef, EventEmitter, Input, OnInit, Output, QueryList, ViewChildren } from '@angular/core';
import { AutomatizationBase, InputApiModel, OutputApiModel } from 'src/models/automatizationComponent';
import { AutomatizationComponent } from '../automatization/automatization.component';
import { BefetchAutomatizationService } from '../befetch-automatization.service';
import { MessagesService } from '../messages.service';
import { MatDialog } from '@angular/material/dialog';
import { AutomatizationEditComponent } from '../automatization-edit/automatization-edit.component';
import { EditRule } from 'src/models/Automatization/EditRule';
import { MatButton } from '@angular/material/button';

@Component({
  selector: 'app-automatization-component',
  templateUrl: './automatization-component.component.html',
  styleUrls: ['./automatization-component.component.css']
})
export class AutomatizationComponentComponent implements OnInit, AfterViewInit {

  @Input() Rule?: AutomatizationBase;
  @Input() Cage?: AutomatizationComponent;
  @ViewChildren('buttonRef') buttonRefs!: QueryList<MatButton>;
  @ViewChildren('outputRef') outputRefs!: QueryList<MatButton>;

  @Output() buttonGenerated = new EventEmitter<{ elementRef: MatButton; id: string }>();
  inputMapping: { [key: number]: string } = {
    0: 'P',
    1: 'T',
    2: 'V'
  };
  constructor(public messenger: MessagesService, public be: BefetchAutomatizationService, private dialog: MatDialog) { }

  ngOnInit(): void {

  }

  ngAfterViewInit(): void {
    // Detect changes when buttons are rendered
    this.buttonRefs.changes.subscribe((buttons: QueryList<MatButton>) => {
      buttons.forEach((button, index) => {
        const id = this.Rule?.inputs[index].id;
        if(id)
        this.buttonGenerated.emit({ elementRef: button, id });
      });
    });

    // Emit already rendered buttons
    this.buttonRefs.forEach((button, index) => {
      const id = this.Rule?.inputs[index].id;
      if(id)
      this.buttonGenerated.emit({ elementRef: button, id });
    });

    this.outputRefs.changes.subscribe((buttons: QueryList<MatButton>) => {
      buttons.forEach((button, index) => {
        const id = this.Rule?.outputs[index].id;
        if(id)
        this.buttonGenerated.emit({ elementRef: button, id });
      });
    });

    // Emit already rendered buttons
    this.outputRefs.forEach((button, index) => {
      const id = this.Rule?.outputs[index].id;
      if(id)
      this.buttonGenerated.emit({ elementRef: button, id });
    });
  }


  SourceClick(id: string){
    if(this.Cage)//&& this.Component instanceof AutomatizationBaseWithTargetGeneric ) //Check this twice!
      this.Cage.SourceClick(id);
  }

  DestinationClick(id: string){
    if(this.Cage)
    this.Cage.DestinationClick(id);
  }

  public editClick(){
    this.dialog.open(AutomatizationEditComponent, {
      width: '500px', // Adjust width as needed
      data: { rule: this.Rule} // Pass any data to the dialog
    });

  }

  public saveClick(){
    if(this.Rule){
      let apiModel: EditRule = {id: this.Rule.id, x: this.Rule.dragPosition.x, y: this.Rule.dragPosition.y, behaviourId: this.Rule.behaviorId, parameters: this.Rule.getParametersJSon() };
      this.be.saveRule(apiModel); 
    }
  }
}
