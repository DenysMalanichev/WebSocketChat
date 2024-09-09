import { Component, Input, OnInit } from '@angular/core';
import { MessageModel } from '../message.model';
import { MessageWithOwnerModel } from '../message-with-owner.model';

@Component({
  selector: 'app-message',
  templateUrl: './message.component.html',
  styleUrls: ['./message.component.css']
})
export class MessageComponent implements OnInit {
  @Input() messageModel!: MessageWithOwnerModel;
  isOwnMessage: boolean = false;

  public ngOnInit() {
    this.isOwnMessage = this.messageModel.OwnMessage;
    console.log(this.messageModel);
  }
}
