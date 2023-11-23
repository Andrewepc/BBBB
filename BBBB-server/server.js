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



var spectators = {};

//=====WEBSOCKET FUNCTIONS======

//Websocket function that managages connection with clients
wss.on('connection', function connection(client){

	//Create Unique User ID for player
	client.id = uuid();

	console.log(`Client ${client.id} Connected!`)

	spectators[client.id] = client;

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
		delete spectators[client.id]
		for(var id in spectators){
			spectators[id].send(`{ "id": "${client.id}", "timestamp": "-1"}`)
		}
		
		console.log(`{ "id": "${client.id}", "timestamp": "-1"}`)
	})

})

wss.on('listening', () => {
	console.log('listening on 8080')
})

setInterval(function() {
	for(var id in playersData){
		testCollisions(id)
	}
	for(var cl in spectators){
		for(var id in playersData){
			spectators[cl].send(JSON.stringify(playersData[id]))
		}
			
	}
	
  }, 16);

  testCollisions = (id) => {
	//console.log((playersData[id].actionStates & 0b01000000))
	if ((playersData[id].actionStates & 0b01000000)) {
		for(var un in playersData){
			
			if (un === id) continue;
			//console.log(addVector(playersData[un].position,{x:0,y:1.75,z:0}))
			if (spheresIntersect(0.25,playersData[id].position,0.5, addVector(playersData[un].position,{x:0,y:1.5,z:0}))) {
				
				playersData[un].health -= 20
				console.log(playersData[un].health)
				playersData[id].fallingSpeed += 10;
				playersData[id].position = addVector(playersData[id].position,{x:0,y:1,z:0})
				if (playersData[un].health <= 0) {
					playersData[id].score += 1
					playersData[id].health = 100
					spectators[un].close()
				}
			}
		}
	}
	if ((playersData[id].actionStates & 0b00010000) && magnitude(playersData[id].movingSpeed) > 11) {
		for(var un in playersData){
			
			if (un === id) continue;
			//console.log(addVector(playersData[un].position,{x:0,y:1.75,z:0}))
			if (spheresIntersect(0.5,addVector(playersData[id].position,{x:0,y:1,z:0}),0.5, addVector(playersData[un].position,{x:0,y:1,z:0}))) {
				
				playersData[un].health -= 10
				console.log(playersData[un].health)
				playersData[un].movingSpeed = scaleVector(playersData[id].movingSpeed,2)
				playersData[id].movingSpeed = 0
				if (playersData[un].health <= 0) {
					playersData[id].score += 1
					playersData[id].health = 100
					spectators[un].close()
				}
				//var d = directionVector(playersData[id].position, playersData[un].position)
				//playersData[id].position = addVector(playersData[id].position,{x:0.6*d.x,y:0.6*d.y,z:0.6*d.z})
				//playersData[un].position = addVector(playersData[un].position,{x:-0.6*d.x,y:-0.6*d.y,z:-0.6*d.z})
			}
		}
	}
	if ((playersData[id].busyStates & 0b00000001) ) {
		
		if (!(playersData[id].busyTimeElapsed > 0.3 && playersData[id].busyTimeElapsed < 0.3333)) return
		for(var un in playersData){
			
			if (un === id) continue;
			console.log(playersData[id].busyTimeElapsed)
			//console.log(playersData[id].aimDirection)
			if (spheresIntersect(1,addVector(playersData[id].position,{x:0,y:3,z:0}),0.5, addVector(playersData[un].position,{x:0,y:1,z:0}))
			|| spheresIntersect(1,addVector(playersData[id].position,addVector(scaleVector(playersData[id].aimDirection,1),{x:0,y:2,z:0})),0.5, addVector(playersData[un].position,{x:0,y:1,z:0}))
			|| spheresIntersect(1,addVector(playersData[id].position,addVector(scaleVector(playersData[id].aimDirection,2),{x:0,y:1,z:0})),0.5, addVector(playersData[un].position,{x:0,y:1,z:0}))) {
				
				playersData[un].health -= 40
				
				playersData[un].position = addVector(playersData[un].position,scaleVector(playersData[id].aimDirection,4 - distanceBetween(playersData[id].position,playersData[un].position)))
				playersData[un].movingSpeed = scaleVector(playersData[id].aimDirection,24)
				if (playersData[un].health <= 0) {
					playersData[id].score += 1
					playersData[id].health = 100
					spectators[un].close()
				}
				//var d = directionVector(playersData[id].position, playersData[un].position)
				//playersData[id].position = addVector(playersData[id].position,{x:0.6*d.x,y:0.6*d.y,z:0.6*d.z})
				//playersData[un].position = addVector(playersData[un].position,{x:-0.6*d.x,y:-0.6*d.y,z:-0.6*d.z})
			}
		}
	}

  }

  spheresIntersect = function(radius1, position1, radius2, position2){
    return distanceBetween(position1, position2) <= (radius1 + radius2)
}

distanceBetween = function(position1, position2){
    return Math.sqrt((position2.x - position1.x) ** 2 + (position2.y - position1.y) ** 2 + (position2.z - position1.z) ** 2)
}
addVector = function(v1, v2){
    return {x:v1.x+v2.x,y:v1.y+v2.y,z:v1.z+v2.z}
}
subVector = function(v1, v2){
    return {x:v2.x-v1.x,y:v2.y-v1.y,z:v2.z-v1.z}
}
scaleVector = function(v1, c){
    return {x:c*v1.x,y:c*v1.y,z:c*v1.z}
}
directionVector = function(v1, v2){
	var dV= subVector(v1,v2)
	var m = magnitude(dV)
    return {x:dV.x/m,y:dV.y/m,z:dV.z/m}
}
magnitude = function(vector){
    return Math.sqrt((vector.x) ** 2 + (vector.y) ** 2 + (vector.z) ** 2)
}

