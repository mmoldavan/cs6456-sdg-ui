/* 
* PADDLE CONTROLLER - COMPLEMENTARY, TOUCH
* Created by Rachel Sheketoff referencing EA PATHFINDERS example
* EA PATHFINDERS API
* Copyright (C) 2015 Electronic Arts Inc.  All rights reserved. 
* This software is solely licensed pursuant to the Hackathon License Agreement,
* Available at:  www.eapathfinders.com/license
* All other use is strictly prohibited. 
*/

var concurrentTouches = 0;
var currentTimer;
var currentTimerActive = false;

$(document).ready(function () {
	//see which player and state our role
	var resultObj = /\d*$/.exec(window.location);
	player = resultObj[0];
	$('#player').text("P"+player+" - Paddler");
	
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
		switch (payload.type) {
			//your code here
		}
	});
});

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
		var sendNotification = {};
		sendNotification.type = "stroke";
		if (whichButton == 'up')
			sendNotification.value = "up";
		else
			sendNotification.value = "down";
		conn.sendMessage(sendNotification);	
	}	
	
} 
