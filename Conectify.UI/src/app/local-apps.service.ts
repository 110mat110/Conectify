import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../environments/environment';
import { Observable, of } from 'rxjs';
import { catchError, map, shareReplay } from 'rxjs/operators';
import { LocalApp } from '../models/local-app.model';

@Injectable({
  providedIn: 'root'
})
export class LocalAppsService {
private configUrl = '/assets/config.json';
private data: LocalApp[] = [];
constructor(private http: HttpClient) { 
    this.loadConfig();   
}

  getApps(): LocalApp[] {
    
    this.data.forEach(element => {
        element.icon = "https://cdn.jsdelivr.net/gh/simple-icons/simple-icons/icons/"+element.icon+".svg";
    });

    return this.data;
  }

    loadConfig() {
    this.http.get(this.configUrl).subscribe({
      next: (data) => this.data = (data as any).services as LocalApp[],
      error: (err) => console.error('Failed to load config', err)
    });
}
}
