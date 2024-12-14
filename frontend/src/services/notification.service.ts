import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { apiPath } from '../envrionment';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private _hubPath = apiPath + 'notificationHub';

  private connection: signalR.HubConnection;
  public messages: string[] = [];
  private messageCallback?: (message: string) => void;

  constructor() {
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(this._hubPath)
      .build();

    this.connection.on('Receive', (message: string) => {
      this.messages.push(message);
      if (this.messageCallback) {
        this.messageCallback(message);
      }
    });

    this.connection.start().catch(err => console.error(err));
  }

  public getMessages(): string[] {
    return this.messages;
  }

  public setMessageCallback(callback: (message: string) => void) {
    this.messageCallback = callback;
  }
}
