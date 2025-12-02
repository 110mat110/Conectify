import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { LocalApp } from 'src/models/local-app.model';

@Component({
  selector: 'app-apps-overview',
  standalone: true,
  imports: [CommonModule],  // <-- import CommonModule here
  templateUrl: './apps-overview.component.html',
  styleUrls: ['./apps-overview.component.css']
})

export class AppsOverviewComponent implements OnInit {
  private configUrl = '/assets/config.json';
  apps: any[] = [];
  fallbackIcon = "https://cdn.jsdelivr.net/npm/@mdi/svg/svg/application.svg"
  constructor(private http: HttpClient) {
  }

  ngOnInit(): void {
      console.log(this.apps); // should print 8 objects
    this.http.get(this.configUrl).subscribe({
      next: (data) => {this.apps = (data as any).services as LocalApp[]
       this.apps.forEach(element => {
        element.icon = "https://cdn.jsdelivr.net/gh/simple-icons/simple-icons/icons/"+element.icon+".svg";
    }); 
      },
      error: (err) => console.error('Failed to load config', err)
    });
  }
}

