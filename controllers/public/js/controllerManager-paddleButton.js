/* 
* PADDLE CONTROLLER - COMPLEMENTARY, TOUCH
* Referencing EA PATHFINDERS example
* EA PATHFINDERS API
* Copyright (C) 2015 Electronic Arts Inc.  All rights reserved. 
* This software is solely licensed pursuant to the Hackathon License Agreement,
* Available at:  www.eapathfinders.com/license
* All other use is strictly prohibited. 
*/

var concurrentTouches = 0;
var currentTimer;
var currentTimerActive = false;
var vibrationSupport = false;
var strokeInit = false;
var strokeInitTime;
var gestureMaxDuration = 1000;

$(document).ready(function () {
	$('.controller').hide();
	
	$('#start').click(function() {
		$('.instructions').remove();
		$('.controller').show();
		initController();
	})
});

function initController() {
	//see which player and state our role
	var resultObj = /\d*$/.exec(window.location);
	player = resultObj[0];
	$('#player').text("P"+player+" - Paddler");
	
	if ('vibrate' in navigator)
		vibrationSupport = true;
	
	/* STROKE and JUMP */
	//mobile - there is 300ms delay on click, so have to use touchstart/end
	$('button').on('touchstart', function(ev) {
		concurrentTouches++;
		$(this).addClass('pressing');
		if (!currentTimerActive) {
			currentTimer = setTimeout(function() {execActionType(ev.target.id)}, 50); //introduce small delay so we can tell if it's a jump or not
			currentTimerActive = true;
		} else {
			execActionType(ev.target.id);	
		}
	})
	$('button').on('touchend', function(ev) {
		if (concurrentTouches > 1) {
			//end double button press = jumping
			var sendNotification = {};
			sendNotification.type = "jump";
			sendNotification.value = "end";
			conn.sendMessage(sendNotification);
		}
		concurrentTouches--;
		$(this).removeClass('pressing');
	})

	//INIT..
	conn = new Connection();
	conn.sendMessage({"type": "connect"});
	
	//incoming game messages
	$(document).on("game_message", function (e, message) {
		console.log("Received Message: " + JSON.stringify(message));
		var payload = message.payload;
		if (payload.type == "jump_initiated" && vibrationSupport)
			navigator.vibrate(200);
	});
}

function execActionType(whichButton) {
	currentTimerActive = false;
	if (concurrentTouches > 1) {
		clearTimeout(currentTimer);
		//double button press = jumping
		var sendNotification = {};
		sendNotification.type = "jump";
		sendNotification.value = "start";
		conn.sendMessage(sendNotification);
	} else {
		if (whichButton == 'up') {
			strokeInit = true;
			strokeInitTime = Date.now();
		}
		else if (whichButton == 'down' && strokeInit == true) {
			var moveDuration = Date.now() - strokeInitTime;
			var moveSpeed = moveDuration/gestureMaxDuration;
			if (moveSpeed < 1) { //ignore if it took longer than max duration
				var moveSpeedCat;
				if (moveSpeed >= .5)
					moveSpeedCat = .35;
				else if (moveSpeed < .5 && moveSpeed >= .15)
					moveSpeedCat = .75;
				else
					moveSpeedCat = 1.0;
				var sendNotification = {};
				sendNotification.type = "stroke";
				sendNotification.value = moveSpeedCat; 
				conn.sendMessage(sendNotification);
			}
			strokeInit = false;
		}
	}	
	
} 
