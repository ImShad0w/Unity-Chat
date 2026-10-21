import { WebSocketServer, WebSocket } from "ws";

const wss = new WebSocketServer({ port: 8000 });
const clients = new Map();

wss.on("connection", (ws) => {
  let nickname;

  ws.on("message", (raw) => {
    let message;

    try {
      message = JSON.parse(raw.toString());
    } catch {
      return;
    }

    if (message.event === "nickname") {
      const name = message.data?.trim();

      if (typeof name !== "string" || !name) return;
      if (clients.has(name)) {
        ws.send(JSON.stringify({ event: "error", data: "Nickname is already in use" }));
        return;
      }

      nickname = name;
      clients.set(nickname, ws);
      return;
    }

    if (message.event === "message") {
      if (!nickname) {
        ws.send(JSON.stringify({ event: "error", data: "Set a nickname first" }));
        return;
      }

      const chatMessage = {
        nickname,
        message: String(message.data ?? "")
      };

      for (const client of wss.clients) {
        if (client.readyState === WebSocket.OPEN) {
          client.send(JSON.stringify(chatMessage));
        }
      }
    }
  });

  ws.on("close", () => {
    if (nickname && clients.get(nickname) === ws) {
      clients.delete(nickname);
    }
  });
});
console.log('Servidor WS al port 8000');
