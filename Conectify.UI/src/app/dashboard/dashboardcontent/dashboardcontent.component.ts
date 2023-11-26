import { Component, Input, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { BEFetcherService } from 'src/app/befetcher.service';
import { DashboardApi, DashboardDeviceApi } from 'src/models/Dashboard/DashboardApi';
import { DasboardAddDeviceDialogComponent } from '../dasboard-add-device-dialog/dasboard-add-device-dialog.component';

@Component({
  selector: 'app-dashboardcontent',
  templateUrl: './dashboardcontent.component.html',
  styleUrls: ['./dashboardcontent.component.css']
})
export class DashboardcontentComponent implements OnInit {

  @Input() id: string = "dashboard id not loaded";
  devices: DashboardDeviceApi[] = [];

  constructor(private befetcher: BEFetcherService, public dialog: MatDialog) { }

  ngOnInit(): void {
    this.befetcher.getDashboard(this.id).subscribe(data => {
      this.devices = data.dashboardDevices;
    });
  }


  addDevice(){
      const dialogRef = this.dialog.open(DasboardAddDeviceDialogComponent, {
        width: '40%',
        height: '30%',
      });
  
      dialogRef.afterClosed().subscribe(result => {
        console.log(`Dialog result: ${result}`);
      });
  }
}
