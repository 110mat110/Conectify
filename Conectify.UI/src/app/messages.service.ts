import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class MessagesService {
  public allMessages: string[] = [];

  constructor() { this.allMessages.push("ctor of messenger!"); }

  addMessage(message: string): void{
    this.allMessages.push(message);
  }

  clear() : void{
    this.allMessages = [];
  }
}
