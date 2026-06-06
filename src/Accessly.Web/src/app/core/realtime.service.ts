import { Injectable, inject } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../environments/environment';
import { AuthService } from './auth.service';
import { CheckIn, CheckInSummary } from './models';

export interface CheckInBroadcast {
  summary: CheckInSummary;
  checkIn: CheckIn;
}

@Injectable({ providedIn: 'root' })
export class RealtimeService {
  private readonly auth = inject(AuthService);
  private connection?: signalR.HubConnection;

  async connect(eventId: string, onCheckIn: (payload: CheckInBroadcast) => void): Promise<void> {
    await this.disconnect();

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.apiBaseUrl}/hubs/checkins`, {
        accessTokenFactory: () => this.auth.token ?? '',
      })
      .withAutomaticReconnect()
      .build();

    this.connection.on('CheckInRecorded', (payload: CheckInBroadcast) => onCheckIn(payload));
    await this.connection.start();
    await this.connection.invoke('JoinEvent', eventId);
  }

  async disconnect(): Promise<void> {
    if (this.connection) {
      await this.connection.stop();
      this.connection = undefined;
    }
  }
}
