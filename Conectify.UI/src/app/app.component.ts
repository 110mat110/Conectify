import { Component, ElementRef, OnInit, SimpleChanges, ViewChild } from '@angular/core';
import { Message, WebsocketService } from './websocket.service';
import { environment } from 'src/environments/environment';
import { OnChanges } from '@angular/core';
import { Input } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { BEFetcherService } from './befetcher.service';
import { CookieService } from 'ngx-cookie-service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
  providers: [WebsocketService]
})
export class AppComponent {
  title = 'Conectify';
  received: string[] = [];
  username: string = "anonymous";
  showDasboard: boolean = false;
  isLoggedIn: boolean = false;
  wsStatus: any = this.websocketService.status;
  constructor(private websocketService: WebsocketService, private befetcher: BEFetcherService, private cookieService: CookieService) {
  }

  ngOnInit() {
    const token = this.cookieService.get('userToken');

    if (token) {
      this.username = token;
    }

    this.initilize();
  }

  initilize(){
    this.showDasboard = this.username != "anonymous";
      this.befetcher.getUserId(this.username).subscribe(id => {
        console.warn("User " + this.username + "has id " + id);
        
        this.befetcher.register(id,this.username);
        this.websocketService.SetId(id);
        this.websocketService.Connect();
      });
    this.isLoggedIn = this.username != "anonymous";
  }

  login() {
    this.cookieService.set('userToken', this.username, 365);

    this.initilize();
  }

  logout() {
    this.cookieService.delete('userToken');
    this.username = "anonymous";
    this.initilize();
  }
}
