import { Component, OnInit } from '@angular/core';
import { MessagesService } from '../messages.service';

@Component({
  selector: 'app-debug-messages',
  templateUrl: './debug-messages.component.html',
  styleUrls: ['./debug-messages.component.css']
})
export class DebugMessagesComponent implements OnInit {

  constructor(public messanger: MessagesService) { }

  ngOnInit(): void {
  }

  clear(): void{
    this.messanger.addMessage("Clear!")
    this.messanger.clear();
  }
}
