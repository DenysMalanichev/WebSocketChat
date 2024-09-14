import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { WebSocketService } from '../web-socket.service';
import { MessageModel } from '../message.model';
import { MessageWithOwnerModel } from '../message-with-owner.model';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-chat',
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.css']
})
export class ChatComponent implements OnInit, OnDestroy {
  public messages: MessageWithOwnerModel[] = [];
  public message: string = '';
  public userName: string = '';
  public chatId: string = '';
  private socketSubscription!: Subscription;

  constructor(private webSocketService: WebSocketService, private route: ActivatedRoute) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.userName = params['userName'];
      this.chatId = params['chatId'];
    });

    this.socketSubscription = this.webSocketService
      .connect('ws://localhost:5000', this.chatId, this.userName)
      .subscribe(
        (message: string) => {
          const messageData: MessageModel = JSON.parse(message);
          console.log('msg ' + messageData);
          this.messages.push({ ...messageData, OwnMessage: false });
        },
        (error) => {
          console.error('WebSocket error', error);
          alert('error connecting to the server');
        }
      );
  }

  sendMessage(): void {
    if (this.message.trim()) {
      const messageData: MessageModel = {
        UserName: this.userName,
        Message: this.message,
        TimeSend: new Date().toISOString(),
        ChatId: this.chatId,
      };

      const messageJson = JSON.stringify(messageData);

      this.messages.push({ ...messageData, OwnMessage: true });
      this.webSocketService.sendMessage(messageJson);
      this.message = '';
    }
  }

  ngOnDestroy(): void {
    this.socketSubscription.unsubscribe();
    this.webSocketService.close(this.chatId, this.userName);
  }
}
