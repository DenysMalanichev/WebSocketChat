import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { ConnectionMessage } from './connection-message-model';

@Injectable({
  providedIn: 'root'
})
export class WebSocketService {
  private socket: WebSocket | undefined;

  connect(url: string, chatId: string, userName: string): Observable<string> {
    this.socket = new WebSocket(url);

    return new Observable<string>(observer => {
      this.socket!.onopen = () => {
        const initialMessage = JSON.stringify({
          type: 'join',
          chatId: chatId,
          userName: userName
        });
        this.socket!.send(initialMessage);
      };

      this.socket!.onmessage = (event) => observer.next(event.data);
      this.socket!.onerror = (event) => observer.error(event);
      this.socket!.onclose = (event) => observer.complete();
    });
  }

  sendMessage(message: string) {
    this.socket!.send(message);
  }

  close(chatId: string, userName: string) {
    this.socket!.send(JSON.stringify({userName, chatId}));
    this.socket!.close();
  }
}