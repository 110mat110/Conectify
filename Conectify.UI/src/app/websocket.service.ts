import { Injectable } from "@angular/core";
import { Observable, Observer } from 'rxjs';
import { AnonymousSubject } from 'rxjs/internal/Subject';
import { Subject } from 'rxjs';
import { map } from 'rxjs/operators';

const CHAT_URL = "ws://server.home:5000/api/websocket/";

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

    constructor() {

    }

    public ConnectById(id: string){
        this.messages = <Subject<string>>this.connect(CHAT_URL + id).pipe(
            map(
                (response: MessageEvent): any => {
                    console.log(response.data);
                    let data = JSON.parse(response.data)
                    return data;
                }
            )
        );
    }

    private connect(url: string): AnonymousSubject<MessageEvent> {
        if (!this.subject) {
            this.subject = this.create(url);
            console.log("Successfully connected: " + url);
        }
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
               console.log(err);
            },
            complete: ()=>{
              console.log("ws complete");
           },
            next: (data: Object) => {
                console.log('Message sent to websocket: ', data);
                if (ws.readyState === WebSocket.OPEN) {
                    ws.send(JSON.stringify(data));
                }
            }
        };
        return new AnonymousSubject<MessageEvent>(observer, observable);
    }
}