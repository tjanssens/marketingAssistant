import { Injectable, NgZone, inject } from '@angular/core';
import { Subject } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { BriefingDto, AlertDto, ActionItemDto } from '../models';

@Injectable({ providedIn: 'root' })
export class SignalRService {
  private readonly zone = inject(NgZone);
  private hubConnection: signalR.HubConnection | null = null;

  readonly newBriefing$ = new Subject<BriefingDto>();
  readonly newAlert$ = new Subject<AlertDto>();
  readonly actionUpdated$ = new Subject<ActionItemDto>();

  start(): void {
    if (this.hubConnection) return;

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/dashboard')
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('NewBriefing', (briefing: BriefingDto) => {
      this.zone.run(() => this.newBriefing$.next(briefing));
    });

    this.hubConnection.on('NewAlert', (alert: AlertDto) => {
      this.zone.run(() => this.newAlert$.next(alert));
    });

    this.hubConnection.on('ActionUpdated', (action: ActionItemDto) => {
      this.zone.run(() => this.actionUpdated$.next(action));
    });

    this.hubConnection.start().catch(err => console.error('SignalR connection error:', err));
  }

  stop(): void {
    this.hubConnection?.stop();
    this.hubConnection = null;
  }
}
