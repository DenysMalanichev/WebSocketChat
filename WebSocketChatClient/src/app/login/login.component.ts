import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  userName: string = '';
  chatId: string = '';

  constructor(private router: Router) {}

  onSubmit() {

    if(this.userName.trim() === '' || this.chatId.trim() === '') {
      alert('Field can not be empty');
      return;
    }

    this.router.navigate(['/app-chat'], { 
      queryParams: { userName: this.userName, chatId: this.chatId } 
    });
  }
}
