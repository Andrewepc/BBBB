var uuid = require('uuid-random');
const WebSocket = require('ws')
const express = require('express');
var compression  = require('compression');
const path = require('path')
const app = express();
 
app.use(compression());
app.use(express.static(path.join(__dirname, 'build')));
 
 
app.listen(8000, () => {
console.log('Server has started!')
});

const wss = new WebSocket.WebSocketServer({port:8080}, ()=> {
	console.log('server started')
})

//Object that stores player data 
var playersData = {}



  var spectators = [];

//=====WEBSOCKET FUNCTIONS======

//Websocket function that managages connection with clients
wss.on('connection', function connection(client){

	//Create Unique User ID for player
	client.id = uuid();

	console.log(`Client ${client.id} Connected!`)

	spectators.push(client);

	//Send default client data back to client for reference
	client.send(`{ "id": "${client.id}"}`)

	//Method retrieves message from client
	client.on('message', (data) => {
		var dataJSON = JSON.parse(data)
		playersData[client.id] = dataJSON
		//console.log("Player Message")
		//console.log(dataJSON)
		
	})

	//Method notifies when client disconnects
	client.on('close', () => {
		console.log('This Connection Closed!')
		console.log("Removing Client: " + client.id)
		delete playersData[client.id]
		spectators.forEach(rem => {
			rem.send(`{ "id": "${client.id}", "timestamp": "-1"}`)
		})
		console.log(`{ "id": "${client.id}", "timestamp": "-1"}`)
	})

})

wss.on('listening', () => {
	console.log('listening on 8080')
})

setInterval(function() {
	
	spectators.forEach(client => {
		for(var id in playersData){
			client.send(JSON.stringify(playersData[id]))
		}
			
	})
	
  }, 16);
