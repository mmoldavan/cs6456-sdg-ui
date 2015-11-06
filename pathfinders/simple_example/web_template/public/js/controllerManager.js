/*
Copyright (C) 2015 Electronic Arts Inc.  All rights reserved.
 
This software is solely licensed pursuant to the Hackathon License Agreement,
Available at:  www.eapathfinders.com/license
All other use is strictly prohibited. 
*/

$(document).ready(function () {

	console.log("Document Loaded");

	// INIT..
	conn = new Connection();
	conn.sendMessage({"type": "connect"});
	
	// Process incoming game messages
	$(document).on("game_message", function (e, message) {
		console.log("Received Message: " + JSON.stringify(message));
		var payload = message.payload;
		switch (payload.type) {
			//your code here
		}
	});
	
	//send a game message
	$('.control').click(function() {
		//console.log({ "type" : "connect", "jump" : $(this).val() });
		var controlVal = parseFloat($(this).val());
		if ($(this).attr("id") == 'back') {
			controlVal = controlVal * -1;
		}
		conn.sendMessage({ "type" : "connect", "forward" : controlVal });
		return false;
	})
	//conn.sendMessage({ "type" : "connect", "example" : "hello there"});
});

