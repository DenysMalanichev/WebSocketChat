import { Component, Input } from '@angular/core';
import { MessageWithOwnerModel } from '../message-with-owner.model';

@Component({
  selector: 'app-user-connected-msg',
  templateUrl: './user-connected-msg.component.html',
  styleUrls: ['./user-connected-msg.component.css']
})
export class UserConnectedMsgComponent {
  @Input() message: MessageWithOwnerModel = null!; 
}
