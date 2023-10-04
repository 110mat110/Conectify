import { Injectable } from "@angular/core";
import { Observable, Observer } from 'rxjs';
import { AnonymousSubject } from 'rxjs/internal/Subject';
import { Subject } from 'rxjs';
import { map } from 'rxjs/operators';
import { MessagesService } from "./messages.service";
import { environment } from "src/environments/environment";


export interface Message {
    source: string;
    content: string;
}

@Injectable({
  providedIn: 'root'
})
export class WebsocketService {
    private subject?: AnonymousSubject<MessageEvent>;
    public messages?: Subject<any>;
    public receivedMessages: Observable<any>;
    private id?: string;
    public status: boolean = false;
    private subs: any[] = []
    
    constructor(private messageService: MessagesService) {
        this.receivedMessages = new Observable<any>((s) => {
            console.warn("Cube subed to websocket");
            this.subs.push(s)
          })
        //this.Connect();
    }

    public SendMessage(message: any){
        if(!this.status){
            this.Connect();
        }
        this.messages?.next(message);
    }
    public SetId(id: string){
        this.id = id;
    }
    public Connect(){
        if(this.status) return;
        if(!this.id)
            this.id = "14546e7f-f9e1-47d1-b667-72033b862706"; //TODO obtain new fingerprint here
        this.ConnectById(this.id);
    }

    private trigger = (v: any) => {
        console.warn("Trigger triggered!");
        this.subs.forEach((sub) => {
          sub.next(v)
        })
      }

    private ConnectById(id: string){
        this.messages = <Subject<string>>this.connect(environment.websocketUrl + id).pipe(
            map(
                (response: MessageEvent): any => {
                    console.log(response.data);
                    let data = JSON.parse(response.data)
                    return data;
                }
            )
        );

        this.messages.subscribe(x => {
            console.log("Received message from WS");
            this.trigger(x);
        });
    }

    private connect(url: string): AnonymousSubject<MessageEvent> {
        if (!this.status || !this.subject) {
            this.subject = this.create(url);
            console.log("Successfully connected: " + url);
            this.status = true;
        }
        this.subject.subscribe(x => {
            console.log("Received message from WS");
            this.trigger(x);
        });
        return this.subject;
    }

    private create(url: string): AnonymousSubject<MessageEvent> {
        let ws = new WebSocket(url);
        let observable = new Observable((obs: Observer<MessageEvent>) => {
            ws.onmessage = obs.next.bind(obs);
            ws.onerror = obs.error.bind(obs);
            ws.onclose = obs.complete.bind(obs);
            return ws.close.bind(ws);
        });
        let observer = null;
        observer = {
            error: (err: any)=>{
                this.status = false;
                console.log(err);
            },
            complete: ()=>{
              this.status = false;
              console.log("ws complete");
           },
            next: (data: Object) => {
                console.log('Message sent to websocket: ', data);
                if (ws.readyState === WebSocket.OPEN) {
                    ws.send(JSON.stringify(data));
                } else{
                    this.status = false;
                }
            }
        };
        return new AnonymousSubject<MessageEvent>(observer, observable);
    }
}