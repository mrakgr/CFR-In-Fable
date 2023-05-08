import * as signalr from "@microsoft/signalr"

let connection = new signalr.HubConnectionBuilder()
    .withUrl("test")
    .withAutomaticReconnect()
    .build()