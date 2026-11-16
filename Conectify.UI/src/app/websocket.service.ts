import { Injectable, NgZone } from "@angular/core";
import { Observable, Subject } from 'rxjs';
import { MessagesService } from "./messages.service";
import { environment } from "src/environments/environment";
import { AdressesService } from "./adresses.service";

export interface Message {
  source: string;
  content: string;
}

@Injectable({
  providedIn: 'root'
})
export class WebsocketService {
  private ws?: WebSocket;
  private readonly received = new Subject<any>();
  public receivedMessages: Observable<any> = this.received.asObservable();
  private id?: string;
  // True only while the socket is really open.
  public status: boolean = false;
  private reconnectDelay = 2000;
  private reconnectTimer?: any;
  private manualClose = false;
  private pendingMessages: any[] = [];
  private hiddenAt?: number;
  // A phone freezes a page in the background and the connection often dies without the browser noticing - it keeps
  // saying OPEN and whatever is sent goes nowhere. After this long in the background the connection is not trusted.
  private staleAfterHidden = 5000;

  constructor(private messageService: MessagesService, private zone: NgZone) {
    document.addEventListener('visibilitychange', () => {
      if (document.visibilityState === 'hidden') {
        this.hiddenAt = Date.now();
        return;
      }

      const hiddenFor = this.hiddenAt ? Date.now() - this.hiddenAt : 0;
      this.hiddenAt = undefined;
      if (hiddenFor >= this.staleAfterHidden) {
        this.reopen();
      } else {
        this.Connect();
      }
    });

    // Page restored from the back/forward cache (Safari) - its old connection is gone.
    window.addEventListener('pageshow', (event: PageTransitionEvent) => {
      if (event.persisted) {
        this.reopen();
      }
    });
  }

  public SendMessage(message: any) {
    if (this.ws?.readyState === WebSocket.OPEN) {
      this.ws.send(JSON.stringify(message));
      return;
    }

    // Not open (yet) - keep it until the connection is up instead of dropping it.
    this.pendingMessages.push(message);
    this.Connect();
  }

  private flushPendingMessages() {
    const msgs = this.pendingMessages.splice(0);
    msgs.forEach(msg => this.SendMessage(msg));
  }

  public SetId(id: string) {
    this.id = id;
    this.Connect();
  }

  public GetId(): string | undefined {
    return this.id;
  }

  public Connect() {
    if (!this.id) return;
    if (this.ws && (this.ws.readyState === WebSocket.OPEN || this.ws.readyState === WebSocket.CONNECTING)) return;
    this.manualClose = false;
    this.open(this.id);
  }

  private open(id: string) {
    clearTimeout(this.reconnectTimer);
    const url = AdressesService.replaceServerHost(environment.websocketUrl + id);
    const ws = new WebSocket(url);
    this.ws = ws;
    console.log("Connecting to: " + url);

    // Handlers are set once here; a socket that was already replaced by a newer one is ignored.
    ws.onopen = () => {
      this.zone.run(() => {
        if (this.ws !== ws) return;
        console.log("WebSocket open");
        this.status = true;
        this.flushPendingMessages();
      });
    };

    ws.onmessage = (event: MessageEvent) => {
      this.zone.run(() => this.received.next(this.parse(event.data)));
    };

    // An error is always followed by close, reconnect is handled there.
    ws.onerror = (err) => {
      console.warn("WebSocket error", err);
    };

    ws.onclose = () => {
      this.zone.run(() => this.handleDisconnect(ws));
    };
  }

  private parse(data: any): any {
    try {
      return JSON.parse(data);
    } catch {
      return data;
    }
  }

  private handleDisconnect(ws: WebSocket) {
    if (this.ws !== ws) return;
    console.warn("WebSocket closed");
    this.ws = undefined;
    this.status = false;
    if (this.manualClose) return;

    clearTimeout(this.reconnectTimer);
    this.reconnectTimer = setTimeout(() => this.reconnect(), this.reconnectDelay);
  }

  private reconnect() {
    console.log("Reconnecting WebSocket...");
    this.Connect();
  }

  /** Drops the current connection, even one that says it is open, and connects again. */
  private reopen() {
    if (this.manualClose) return;
    console.log("Reopening WebSocket...");
    const old = this.ws;
    // Cleared first, so the close of the old socket is ignored and does not schedule another reconnect.
    this.ws = undefined;
    this.status = false;
    old?.close();
    this.Connect();
  }

  public Close() {
    this.manualClose = true;
    clearTimeout(this.reconnectTimer);
    const ws = this.ws;
    this.ws = undefined;
    this.status = false;
    ws?.close();
  }
}
