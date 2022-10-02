import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/internal/Observable';

@Injectable({
  providedIn: 'root'
})
export class MessagesService {
  public allMessages: string[] = [];
  public lastMessage: Observable<any>;

  constructor() { 
    this.allMessages.push("ctor of messenger!"); 
    this.lastMessage = new Observable<any>();
}

  addMessage(message: string): void{
    console.info(message);
    this.allMessages.push(message);
  }

  clear() : void{
    this.allMessages = [];
  }

  addFromWebsocket(msg: any){
    this.lastMessage.pipe(msg);
  }

}
