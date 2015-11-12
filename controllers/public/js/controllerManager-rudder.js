/* 
* RUDDER CONTROLLER - COMPLEMENTARY, PHYSICAL + TOUCH
* Created by Rachel Sheketoff referencing EA PATHFINDERS example
* EA PATHFINDERS API
* Copyright (C) 2015 Electronic Arts Inc.  All rights reserved. 
* This software is solely licensed pursuant to the Hackathon License Agreement,
* Available at:  www.eapathfinders.com/license
* All other use is strictly prohibited. 
*/

var moveBuffer = 10;
var currPosition = 0.0;
var startLoc;
var jumping = false;
var vibrationSupport = false;

$(document).ready(function () {
	$('.controller').hide();
	if (!window.DeviceOrientationEvent) {
		alert("Device Orientation not supported and is required for this controller.");
		$('#start').html('Go Back');
	}
	
	$('#start').click(function() {
		if (!window.DeviceOrientationEvent)
			window.location = '/index.html';
		else {
			$('.instructions').hide(500);
			$('.controller').show();
			initController();
		}
	})
});

function initController() {
	//see which player we are + our role and if we're in debug mode
	debugMode = /\/debug\//.test(window.location);
	if (!debugMode)
		$('.debug-output').hide();
	var resultObj = /\d*$/.exec(window.location);
	player = resultObj[0];
	$('#player').text("P"+player+' - Navigator');
	
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
	} else {
		alert("Device Orientation not supported and is required for this controller.");
	}
	
	/* RUDDER DIRECTIONS */
	$('body').on('touchstart', function(ev) {
		startLoc = ev.originalEvent.touches.item(0).screenX;
	})
	$('body').on('touchmove', function(ev) {
		ev.preventDefault();
		var currLoc = ev.originalEvent.touches.item(0);				
		var dragDistance = currLoc.screenX - startLoc;
		if (Math.abs(dragDistance) > moveBuffer) {
			var movePercent = (2*dragDistance)/($('body').width()-100);
			movePercent = applyBounds(movePercent, 2.0);
			updateDirection(movePercent);
			startLoc = currLoc.screenX;
		}
	});

	//INIT..
	conn = new Connection();
	conn.sendMessage({"type": "connect"});
	conn.sendMessage({"type" : "role_change", "value": "navigator"}); 
	
	//incoming game messages
	$(document).on("game_message", function (e, message) {
		console.log("Received Message: " + JSON.stringify(message));
		var payload = message.payload;
		if (payload.type == "jump_initiated" && vibrationSupport)
			navigator.vibrate(200);
	});	
}

function updateDirection(amount) {
	var newPosition = applyBounds(parseFloat(currPosition) + parseFloat(amount), 1.0);
	newPosition = newPosition.toFixed(1);
	newPosition = parseFloat(newPosition);
	if (newPosition != currPosition) {
		currPosition = newPosition;
		var sendNotification = {};
		sendNotification.type = "direction";
		sendNotification.value = currPosition;
		conn.sendMessage(sendNotification);
		console.log(sendNotification);
		updateRef(currPosition);
	}	
}

function applyBounds(value, bound) {
	if (value > bound)
		value = bound;
	if (value < -bound)
		value = -bound;
	return value;
}

function updateRef(newPosition) {
	var degrees = 60*newPosition;
	console.log('('+degrees+'deg)');
	$('#feedback').css('transform', 'rotate('+degrees+'deg)');
}

function deviceOrientHandler(eventData) {	
	/* Detect Jumps */
	var rVal = eventData.beta;
	if ((rVal >= -10 && rVal <= 25) && !jumping) {
		//init jump
		jumping = true;
		var sendNotification = {};
		sendNotification.type = "jump";
		sendNotification.action = "jump";
		sendNotification.value = "start";
		conn.sendMessage(sendNotification);
		if (debugMode)
			$('#jumping').text("jumping!");
	} else if ((rVal > 35 || rVal < -20) && jumping) {
		jumping = false;
		var sendNotification = {};
		sendNotification.type = "jump";
		sendNotification.action = "jump";
		sendNotification.value = "end";
		conn.sendMessage(sendNotification);
		if (debugMode)
			$('#jumping').text("NOT jumping!");
	}
}

