/*
Copyright (C) 2015 Electronic Arts Inc.  All rights reserved.
 
This software is solely licensed pursuant to the Hackathon License Agreement,
Available at:  http://www.eapathfinders.com/license
All other use is strictly prohibited. 
*/

$(document).ready(function () {

	console.log("Document Loaded");

	// INIT..
	conn = new Connection();
	conn.sendMessage({"type": "connect"});
	
	var powerup_button = new PowerupButton($("#powerup"));
	var touchbox = new TouchBox($("#touchbox"));

	// Process incoming game messages
	$(document).on("game_message", function (e, message) {
		console.log("Received Message: " + JSON.stringify(message));
		var payload = message.payload;
		switch (payload.type) {
			case "enable_powerup_button":
				powerup_button.enablePowerup();
				break;
			case "disable_powerup_button":
				powerup_button.disablePowerup();
				break;
			case "set_powerup_button_time":
				powerup_button.setPowerupTime(payload.time);
				break;
		}
	});
});

