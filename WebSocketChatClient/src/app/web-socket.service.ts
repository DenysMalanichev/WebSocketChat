import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class WebSocketService {
  private socket!: WebSocket;
  private subject!: Subject<string>;

  constructor() { }

  public connect(url: string): Subject<string> {
    if (!this.subject) {
      this.subject = this.create(url);
      console.log('Successfully connected: ' + url);
    }
    return this.subject;
  }

  private create(url: string): Subject<string> {
    this.socket = new WebSocket(url);

    const observable = new Observable<string>(observer => {
      this.socket.onmessage = (event) => observer.next(event.data);
      this.socket.onerror = (event) => observer.error(event);
      this.socket.onclose = () => observer.complete();

      return () => {
        this.socket.close();
      };
    });

    const observer = {
      next: (data: string) => {
        if (this.socket.readyState === WebSocket.OPEN) {
          this.socket.send(data);
        }
      }
    };

    return Subject.create(observer, observable);
  }

  public disconnect() {
    if (this.socket) {
      this.socket.close();
    }
  }
}
