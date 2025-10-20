import { Injectable, NgZone } from "@angular/core";
import { Observable, Observer, Subject } from 'rxjs';
import { AnonymousSubject } from 'rxjs/internal/Subject';
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
  private subs: any[] = [];
  private reconnectDelay = 2000;
  private reconnectTimer?: any;
  private manualClose = false;

  constructor(private messageService: MessagesService, private zone: NgZone) {
        this.receivedMessages = new Observable<any>((s) => {
            this.subs.push(s)
          })
    // Auto reconnect when tab becomes visible again
    document.addEventListener('visibilitychange', () => {
      if (document.visibilityState === 'visible' && !this.status && this.id) {
        this.reconnect();
      }
    });
  }

  public SendMessage(message: any) {
    if (!this.status) this.Connect();
    this.messages?.next(message);
  }

  public SetId(id: string) {
    this.id = id;
    this.Connect();
  }

  public GetId(): string | undefined {
    return this.id;
  }

  private trigger = (v: any) => {
    this.subs.forEach((sub) => sub.next(v));
  }

  public Connect() {
    if (this.status || !this.id) return;
    this.manualClose = false;
    this.ConnectById(this.id);
  }

  private ConnectById(id: string) {
    this.messages = <Subject<string>>this.connect(environment.websocketUrl + id).pipe(
      map((response: MessageEvent): any => {
        try {
          const data = JSON.parse(response.data);
          return data;
        } catch {
          return response.data;
        }
      })
    );

    this.messages.subscribe(x => {
      this.trigger(x);
    });
  }

  private connect(url: string): AnonymousSubject<MessageEvent> {
    if (!this.status || !this.subject) {
      this.subject = this.create(url);
      console.log("Connected to: " + url);
      this.status = true;
    }

    this.subject.subscribe({
      next: (x: any) => {this.trigger(x);},
      error: (err: any) => {
        console.warn("WebSocket error", err);
        this.handleDisconnect();
      },
      complete: () => {
        console.warn("WebSocket closed");
        this.handleDisconnect();
      }
    });

    return this.subject;
  }

  private create(url: string): AnonymousSubject<MessageEvent> {
    const ws = new WebSocket(url);

    const observable = new Observable((obs: Observer<MessageEvent>) => {
      ws.onmessage = obs.next.bind(obs);
      ws.onerror = obs.error.bind(obs);
      ws.onclose = obs.complete.bind(obs);
      return ws.close.bind(ws);
    });

    const observer = {
      next: (data: Object) => {
        if (ws.readyState === WebSocket.OPEN) {
          ws.send(JSON.stringify(data));
        } else {
          console.warn("WebSocket not open, message skipped");
        }
      },
      error: (err: any) => {
        console.error("WebSocket observer error", err);
      },
      complete: () => {
        console.log("WebSocket observer complete");
      }
    };

    ws.onopen = () => {
      console.log("WebSocket open");
      this.zone.run(() => this.status = true);
    };

    ws.onclose = () => {
      console.log("WebSocket closed");
      this.zone.run(() => this.handleDisconnect());
    };

    ws.onerror = (err) => {
      console.error("WebSocket error", err);
      this.zone.run(() => this.handleDisconnect());
    };

    return new AnonymousSubject<MessageEvent>(observer, observable);
  }

  private handleDisconnect() {
    if (this.manualClose) return;
    if (this.status) this.status = false;
    this.subject = undefined;

    clearTimeout(this.reconnectTimer);
    this.reconnectTimer = setTimeout(() => this.reconnect(), this.reconnectDelay);
  }

  private reconnect() {
    if (!this.id) return;
    console.log("Reconnecting WebSocket...");
    this.ConnectById(this.id);
  }

  public Close() {
    this.manualClose = true;
    this.subject = undefined;
    this.status = false;
  }
}
