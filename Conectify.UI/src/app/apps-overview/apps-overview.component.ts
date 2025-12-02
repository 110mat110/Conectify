import { Component, OnInit } from '@angular/core';
import { LocalAppsService } from '../local-apps.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-apps-overview',
  standalone: true,
  imports: [CommonModule],  // <-- import CommonModule here
  templateUrl: './apps-overview.component.html',
  styleUrls: ['./apps-overview.component.css']
})

export class AppsOverviewComponent implements OnInit {
  apps: any[] = [];
  fallbackIcon = "https://cdn.jsdelivr.net/npm/@mdi/svg/svg/application.svg"
  constructor(private localApps: LocalAppsService) {
  }

  ngOnInit(): void {
    this.apps = this.localApps.getApps();
      console.log(this.apps); // should print 8 objects

  }
}

