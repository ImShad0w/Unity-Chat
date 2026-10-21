import WebSocket from 'ws';

const ws = new WebSocket("ws://localhost:8080");

ws.on('open', () => {
  console.log(`connectat`);
});

ws.on('message', function message(data) {
  console.log('Client 2 - %s', data);
});

setInterval(randomHehe, 2000)

function randomHehe() {
  let random = Math.floor(Math.random() * 100000);
  ws.send(random)
}
