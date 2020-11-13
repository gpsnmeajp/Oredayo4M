"use strict";
onmessage = function(e) {
	var xhr = new XMLHttpRequest();
	xhr.open("POST" , "/command.dat", false);//同期Request
	xhr.setRequestHeader("If-Modified-Since", "Thu, 01 Jan 1970 00:00:00 GMT");
	xhr.timeout = 1000;
	try {
		xhr.send(e.data);
	}catch (e) {
		postMessage(JSON.stringify({"command":"Internal","message":e.message}));
	}
	
	//---return stat---
	if(xhr.readyState != 4)
	{
		postMessage(JSON.stringify({"command":"Internal","message":"load failed."}));
		return -1;
	}
	if(xhr.status == 0){
		postMessage(JSON.stringify({"command":"Internal","message":"Communication Error"}));
		return -1;
	}
	postMessage(xhr.responseText);
}	