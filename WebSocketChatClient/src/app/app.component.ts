import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { WebSocketService } from './web-socket.service';
import { MessageModel } from './message.model';
import { MessageWithOwnerModel } from './message-with-owner.model';

@Component({
  selector: 'app-chat',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
})
export class AppComponent implements OnInit, OnDestroy {
  public messages: MessageWithOwnerModel[] = [];
  public message: string = '';
  public userId: number = Math.floor(Math.random() * (99999 - 10000) + 10000);
  private socketSubscription!: Subscription;

  constructor(private webSocketService: WebSocketService) {}

  ngOnInit(): void {
    this.socketSubscription = this.webSocketService
      .connect('ws://localhost:5000')
      .subscribe(
        (message: string) => {
          const messageData: MessageModel = JSON.parse(message);
          this.messages.push({ ...messageData, OwnMessage: false });
        },
        (error) => {
          console.error('WebSocket error', error);
        }
      );
  }

  sendMessage(): void {
    if (this.message.trim()) {
      const messageData: MessageModel = {
        UserId: this.userId,
        Message: this.message,
        TimeSend: new Date().toISOString(),
      };

      const messageJson = JSON.stringify(messageData);

      this.messages.push({ ...messageData, OwnMessage: true });
      this.webSocketService.connect('ws://localhost:5000').next(messageJson);
      this.message = '';
    }
  }

  ngOnDestroy(): void {
    this.socketSubscription.unsubscribe();
    this.webSocketService.disconnect();
  }
}
