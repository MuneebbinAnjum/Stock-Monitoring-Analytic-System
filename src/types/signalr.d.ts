declare module '@microsoft/signalr' {
  export function HubConnectionBuilder(): any
  export const LogLevel: any
  export class HubConnection {
    start(): Promise<void>
    stop(): Promise<void>
    on(eventName: string, callback: (...args: any[]) => void): void
    off(eventName: string, callback?: (...args: any[]) => void): void
    invoke(methodName: string, ...args: any[]): Promise<any>
    state: string
  }
  export class HubConnectionBuilderClass {
    withUrl(url: string, options?: any): any
    withAutomaticReconnect(): any
    configureLogging(level: any): any
    build(): HubConnection
  }
  export const HubConnectionBuilder: any
}
