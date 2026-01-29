import * as signalR from '@microsoft/signalr';

export type NewEmailHandler = (email: {
  id: string;
  from: string;
  fromDisplay?: string;
  subject: string;
  receivedAt: string;
  hasAttachments: boolean;
}) => void;

export type EmailUpdatedHandler = (update: {
  id: string;
  isRead: boolean;
}) => void;

export type EmailDeletedHandler = (emailId: string) => void;

export type UnreadCountChangedHandler = (count: number) => void;

class SignalRConnection {
  private connection: signalR.HubConnection | null = null;
  private isConnecting = false;

  async connect(apiUrl: string): Promise<signalR.HubConnection> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      return this.connection;
    }

    if (this.isConnecting) {
      // Wait for existing connection attempt
      while (this.isConnecting) {
        await new Promise(resolve => setTimeout(resolve, 100));
      }
      return this.connection!;
    }

    this.isConnecting = true;

    try {
      const hubUrl = `${apiUrl}/hubs/email`;

      this.connection = new signalR.HubConnectionBuilder()
        .withUrl(hubUrl, {
          withCredentials: true,
        })
        .withAutomaticReconnect()
        .build();

      await this.connection.start();
      console.log('SignalR connected');

      this.connection.onreconnecting(() => {
        console.log('SignalR reconnecting...');
      });

      this.connection.onreconnected(() => {
        console.log('SignalR reconnected');
      });

      this.connection.onclose((error) => {
        console.error('SignalR disconnected:', error);
      });

      return this.connection;
    } finally {
      this.isConnecting = false;
    }
  }

  onNewEmail(handler: NewEmailHandler) {
    if (!this.connection) {
      console.warn('SignalR connection not established');
      return;
    }
    this.connection.on('NewEmail', handler);
  }

  onEmailUpdated(handler: EmailUpdatedHandler) {
    if (!this.connection) {
      console.warn('SignalR connection not established');
      return;
    }
    this.connection.on('EmailUpdated', handler);
  }

  onEmailDeleted(handler: EmailDeletedHandler) {
    if (!this.connection) {
      console.warn('SignalR connection not established');
      return;
    }
    this.connection.on('EmailDeleted', handler);
  }

  onUnreadCountChanged(handler: UnreadCountChangedHandler) {
    if (!this.connection) {
      console.warn('SignalR connection not established');
      return;
    }
    this.connection.on('UnreadCountChanged', handler);
  }

  async disconnect() {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
    }
  }
}

export const signalRConnection = new SignalRConnection();
