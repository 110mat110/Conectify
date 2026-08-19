import { Component, Inject, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { FunctionModel } from 'src/models/Automatization/FunctionModel';
import { BefetchAutomatizationService } from '../befetch-automatization.service';

/** Name and note of a function. Ports are added on the board of the function itself. */
@Component({
  selector: 'app-automatization-function-edit',
  templateUrl: './automatization-function-edit.component.html',
  styleUrl: './automatization-function-edit.component.css'
})
export class AutomatizationFunctionEditComponent {
  name: string = '';
  description: string = '';

  readonly dialogRef = inject(MatDialogRef<AutomatizationFunctionEditComponent>);

  constructor(public be: BefetchAutomatizationService, @Inject(MAT_DIALOG_DATA) public data: { function: FunctionModel }) { }

  ngOnInit(): void {
    this.name = this.data.function.name;
    this.description = this.data.function.description;
  }

  Save() {
    const model = {
      name: this.name,
      description: this.description,
      x: this.data.function.x,
      y: this.data.function.y,
    };

    // Closing only after the save landed keeps the board from reloading the old name.
    this.be.saveFunction(this.data.function.id, model).subscribe({
      next: () => this.dialogRef.close(),
      error: () => this.dialogRef.close(),
    });
  }

  Close() {
    this.dialogRef.close();
  }
}
