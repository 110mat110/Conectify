import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-dasboard-add-device-dialog',
  templateUrl: './dasboard-add-device-dialog.component.html',
  styleUrls: ['./dasboard-add-device-dialog.component.css']
})
export class DasboardAddDeviceDialogComponent implements OnInit {

  edit: boolean = false;

  constructor() { }

  ngOnInit(): void {
  }

}
