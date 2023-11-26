import { Component, ElementRef, OnInit, SimpleChanges, ViewChild } from '@angular/core';
import { Message, WebsocketService } from './websocket.service';
import { environment } from 'src/environments/environment';
import { OnChanges } from '@angular/core';
import { Input } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { BEFetcherService } from './befetcher.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
  providers: [WebsocketService]
})
export class AppComponent {
  title = 'Conectify';
  received: string[] = [];
  @ViewChild("fingerprintIdTextBoxRef") myNameElem?: ElementRef;
  @ViewChild("mailTextBoxRef") mail?: ElementRef;
  currentUser: string = "anonymous";
  showDasboard: boolean = false;
  wsStatus: any = this.websocketService.status;
  constructor(private websocketService: WebsocketService, private befetcher: BEFetcherService) {
    this.initilize();
  }

  initilize(){
    this.showDasboard = this.currentUser != "anonymous";
      this.befetcher.getUserId(this.currentUser).subscribe(id => {
        console.warn("User " + this.currentUser + "has id " + id);
        

        this.befetcher.register(id,this.currentUser);
        this.websocketService.SetId(id);
        this.websocketService.Connect();
      });
  }

  onChangeMail() {
    this.currentUser = this.mail?.nativeElement.value;

    this.initilize();
  }
}
