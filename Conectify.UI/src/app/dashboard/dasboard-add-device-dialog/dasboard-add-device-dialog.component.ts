import { Inject, Input } from '@angular/core';
import { Component, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialog, MatDialogRef } from '@angular/material/dialog';
import { BEFetcherService } from 'src/app/befetcher.service';
import { SelectDestinationActuatorOverlayComponent } from 'src/app/select-destination-actuator-overlay/select-destination-actuator-overlay.component';
import { SelectInputSensorOverlayComponent } from 'src/app/select-input-sensor-overlay/select-input-sensor-overlay.component';
import { DashboardApi } from 'src/models/Dashboard/DashboardApi';
import { DashboardParams } from 'src/models/Dashboard/DashboardParams';
import { Sensor } from 'src/models/sensor';

@Component({
  selector: 'app-dasboard-add-device-dialog',
  templateUrl: './dasboard-add-device-dialog.component.html',
  styleUrls: ['./dasboard-add-device-dialog.component.css']
})
export class DasboardAddDeviceDialogComponent implements OnInit {
  constructor(private beFetcher: BEFetcherService, @Inject(MAT_DIALOG_DATA) public data: {dashboard: DashboardApi, params: DashboardParams},public dialog: MatDialog, private thisDialogRef: MatDialogRef<DasboardAddDeviceDialogComponent>) { }

  ngOnInit(): void {
  }

  save(){
    this.beFetcher.editDashboard(this.data.dashboard.id, {name: this.data.dashboard.name, background: this.data.dashboard.background})
  }

  sensor(){
    const dialogRef = this.dialog.open(SelectInputSensorOverlayComponent, {
      width: '50%',
      height: '100%',
      restoreFocus: false
    });

    dialogRef.afterClosed().subscribe(result => {
      this.thisDialogRef.close({sensor:result});
    });
  }

  actuator(){
    const dialogRef = this.dialog.open(SelectDestinationActuatorOverlayComponent, {
      width: '50%',
      height: '100%',
      restoreFocus: false
    });

    dialogRef.afterClosed().subscribe(result => {
      this.thisDialogRef.close({actuator:result});
    });
  }
}
