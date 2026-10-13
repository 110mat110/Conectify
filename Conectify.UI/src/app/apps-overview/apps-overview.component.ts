import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { LocalApp } from 'src/models/local-app.model';
import { AdressesService } from '../adresses.service';

@Component({
  selector: 'app-apps-overview',
  standalone: true,
  imports: [CommonModule],  // <-- import CommonModule here
  templateUrl: './apps-overview.component.html',
  styleUrls: ['./apps-overview.component.css']
})

export class AppsOverviewComponent implements OnInit {
  private configUrl = '/assets/config.json';
  apps: LocalApp[] = [];
  fallbackIcon = "https://cdn.jsdelivr.net/npm/@mdi/svg/svg/application.svg"
  constructor(private http: HttpClient) {
  }

  ngOnInit(): void {
    this.http.get(this.configUrl).subscribe({
      next: (data) => {
        const services = (data as any).services as LocalApp[];
        services.forEach(element => {
          element.icon = "https://cdn.jsdelivr.net/gh/simple-icons/simple-icons/icons/" + element.icon + ".svg";
          element.url = AdressesService.replaceServerHost(element.url);
        });
        this.apps = services;
      },
      error: (err) => console.error('Failed to load config', err)
    });
  }
}

