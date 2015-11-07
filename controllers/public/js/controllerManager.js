/* 
* PADDLE CONTROLLER - SYNERGY, PHYSICAL
* Created by Rachel Sheketoff referencing EA PATHFINDERS example
* EA PATHFINDERS API
* Copyright (C) 2015 Electronic Arts Inc.  All rights reserved. 
* This software is solely licensed pursuant to the Hackathon License Agreement,
* Available at:  www.eapathfinders.com/license
* All other use is strictly prohibited. 
*/

var vMotion = {};
vMotion.history = [];
var moveBuffer = 2; //degrees, safety amount for detecting real movement
var gestureMaxDuration = 1000;
var jumping = false;
var vibrationSupport = false;
var debugMode = false;
var player;

$(document).ready(function () {
	//see which player we are and if we're in debug mode
	debugMode = /\/debug\//.test(window.location);
	if (!debugMode)
		$('.debug-output').hide();
	var resultObj = /\d*$/.exec(window.location);
	player = resultObj[0];
	$('#player').text("P"+player);
	
	//set up orientation event listener, determine vibration support
	if (window.DeviceOrientationEvent) {
		$('#orientStatus').text("Device Orientation is supported");
		window.addEventListener('deviceorientation', deviceOrientHandler, false);
		if ('vibrate' in navigator) {
			vibrationSupport = true;
			$('#vibStatus').text("Vibration is supported");
		} else {
			$('#vibStatus').text("Vibration NOT supported");
		}
		/*var fakeEventData = {};
		fakeEventData.gamma = 100;
		for (var i = 0; i < 10; i++) {
			(function(j){setTimeout(function(){deviceOrientHandler(fakeEventData);},j*500);}(i));
		}*/
	} else {
		$('#orientStatus').text("Device Orientation not supported and is required for this game.");
	}

	//INIT..
	conn = new Connection();
	conn.sendMessage({"type": "connect"});
	
	//incoming game messages
	$(document).on("game_message", function (e, message) {
		console.log("Received Message: " + JSON.stringify(message));
		var payload = message.payload;
		switch (payload.type) {
			//your code here
		}
	});
});

function deviceOrientHandler(eventData) {	
	var thisInstant = Date.now();
	var gammaVal = (-1)*(eventData.gamma); //reverse it so directions match perceptual logic
	
	/* Detect Strokes */
	if (vMotion.history.length == 0) {
		writeMove(gammaVal, thisInstant);
	} else {
		var prevPoint = vMotion.history[vMotion.history.length-1];
		var thisInterval = thisInstant - prevPoint.time;
		console.log(prevPoint);
		if (Math.abs(gammaVal-prevPoint.val) > moveBuffer ) { 
			var newStartIndex = vMotion.history.length-1;
			while(newStartIndex > 0 && thisInstant - vMotion.history[newStartIndex].time < gestureMaxDuration ) {
				newStartIndex--;
			}
			vMotion.history = vMotion.history.slice(newStartIndex);
			writeMove(gammaVal, thisInstant);
			checkForGesture();
		}
	}
	
	/* Detect Jumps */
	var alphaVal = eventData.beta;
	if ((alphaVal >= 60 || alphaVal <= -60) && !jumping) {
		//init jump
		jumping = true;
		var sendNotification = {};
		sendNotification.type = "jump";
		sendNotification.action = "jump";
		sendNotification.value = "start";
		conn.sendMessage(sendNotification);
		console.log(sendNotification);
		$('#jumping').text("jumping!");
	} else if (alphaVal < 60 && alphaVal > -60 && jumping) {
		jumping = false;
		var sendNotification = {};
		sendNotification.type = "jump";
		sendNotification.action = "jump";
		sendNotification.value = "end";
		conn.sendMessage(sendNotification);
		console.log(sendNotification);
		$('#jumping').text("NOT jumping!");
	}
}

function writeMove(val, thisInstant) {
	var currMove = {};
	currMove.time = thisInstant;
	currMove.val = val;
	console.log(currMove);
	if (vMotion.history.length > 0) {
		var prevPoint = vMotion.history[vMotion.history.length-1].val; //get last move's value
		if (prevPoint >= 0) {
			if (currMove.val < 0)
				currMove.move = "d";
			else { //gamma is still pos
				if (currMove.val > prevPoint) {
					currMove.move = "u";	
				} else if (currMove.val == prevPoint) {
					currMove.move = "s"; //for still, because of buffer shouldn't be possible
				} else { //must be less so moving down
					currMove.move = "d";
				}
			}
		} else { //prevPoint was less than 0
			if (currMove.val > 0)
				currMove.move = "u";
			else { //gamma is still neg
				if (prevPoint < currMove.val) {
					currMove.move = "u";	
				} else if (prevPoint == currMove.val) {
					currMove.move = "s";	
				} else { //prevPoint must be greater, so we're moving down
					currMove.move = "d";
				}
			}			
		}		
	} else {
		currMove.move = "z" //placeholder for initial read point	
	}
	vMotion.history.push(currMove);
}

function checkForGesture() {
	//adapts the SiGeR method for recognizing a particular rotation sequence 
	var histString = '';	
	for (var i = 0; i < vMotion.history.length; i++) {
			histString += vMotion.history[i].move;
	}
	if (debugMode)
		$('#gestureString').text(histString);
	var strokePattern = /u{4,30}d{3,30}/;
	var foundIndex = histString.search(strokePattern);
	if (foundIndex > -1) {
		var matches = strokePattern.exec(histString);
		var stopIndex = matches[0].length-1;
		var firstMatchMove = vMotion.history[foundIndex];
		var lastMatchMove = vMotion.history[foundIndex + stopIndex];
		var moveDuration = lastMatchMove.time - firstMatchMove.time;
		if (debugMode)
			$('#strokeTime').text($('#strokeTime').text()+" "+moveDuration);
		moveSpeed = (gestureMaxDuration - moveDuration) / gestureMaxDuration;
		//take the continuous var and map it to categories for slow, medium, fast
		var moveSpeedCat;
		if (moveSpeed < .35)
			moveSpeedCat = 1;
		else if (moveSpeed >= .35 && moveSpeed <= .6)
			moveSpeedCat = 0.5;
		else
			moveSpeedCat = 0.25;		 
		var sendNotification = {};
		sendNotification.type = "stroke";
		sendNotification.action = "stroke";
		sendNotification.value = moveSpeedCat;
		conn.sendMessage(sendNotification);
		console.log(sendNotification);
		//once we've recognized the first stroke, this whole history can be reset
		vMotion.history = [];
	}
}

