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

  getApps(): LocalApp[] {
    let apps: LocalApp[] = JSON.parse(environment.localApps).services
    apps.forEach(element => {
        element.icon = "https://cdn.jsdelivr.net/gh/simple-icons/simple-icons/icons/"+element.icon+".svg";
    });

    return apps;
  }
}
