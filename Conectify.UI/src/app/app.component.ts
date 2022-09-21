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
  title = 'IoTHomeUI';
  received: string[] = [];
  @ViewChild("fingerprintIdTextBoxRef") myNameElem?: ElementRef;
  wsStatus: any;
  constructor(private websocketService: WebsocketService, private befetcher: BEFetcherService ) {
  }

  onChange(){
    let id = this.myNameElem?.nativeElement.value;

    if (id) {
      this.befetcher.register(id);
      console.warn("Connectiong to ws with id " + id);
      this.websocketService.ConnectById(id);
      this.websocketService.messages?.subscribe(msg => {
        this.received.push(msg);
        console.log("Response from websocket: " + msg);
      });
    } else{
      console.warn("fingerprint is not initialized");
    }
  }
}
