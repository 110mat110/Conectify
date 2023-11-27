import { Component, OnInit } from '@angular/core';
import { BEFetcherService } from 'src/app/befetcher.service';
import { WebsocketService } from 'src/app/websocket.service';
import {MatTabsModule} from '@angular/material/tabs';
import { DashboardApi } from 'src/models/Dashboard/DashboardApi';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {

  public dashboards : DashboardApi[] = [];

  constructor(private befetcher: BEFetcherService, private websocketService: WebsocketService) { }

  ngOnInit(): void {
      var userId = this.websocketService.GetId();    

      if(!userId){
        console.error("User not found in dashboard service");
        return;
      }
      this.befetcher.getDasboards(userId).subscribe(d => this.dashboards = d);
  }

  create(){
    var userId = this.websocketService.GetId();    

    if(!userId){
      console.error("User not found in dashboard service");
      return;
    }

    this.befetcher.addDashboard({position: this.dashboards.length, type:0, userId: userId}).subscribe(d => this.dashboards.push(d));
  }

}
