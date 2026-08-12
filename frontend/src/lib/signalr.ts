import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr'

let connection: HubConnection | null = null

export function createConnection(): HubConnection {
  if (connection) return connection
  // @ts-ignore: SignalR types might not have construct signature based on tsconfig
  connection = new HubConnectionBuilder()
    .withUrl((import.meta.env.VITE_API_BASE ?? '/api').replace(/\/api$/, '') + '/hub/notifications', { 
      skipNegotiation: false,
      accessTokenFactory: () => sessionStorage.getItem('token') || ''
    })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Information)
    .build()
  return connection as HubConnection
}

export async function startConnection(): Promise<HubConnection> {
  const c = createConnection()
  if (c.state === 'Disconnected') {
    await c.start()
  }
  return c
}

export function getConnection(): HubConnection | null {
  return connection
}

export async function stopConnection() {
  if (connection) {
    try { await connection.stop() } catch { }
    connection = null
  }
}
