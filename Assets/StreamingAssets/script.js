"use strict";
let view = document.getElementById ("view");
let worker;

window.onload = ()=>{
	if (!window.Worker) {
		alert("Web Worker disabled! Editor won't work!")
	}
	
	try {
		worker = new Worker("worker.js");
	}catch (e) {
		addStat("Exception!(UI): "+e.message);
	}
	
	worker.onmessage = function(e) {
		let response = JSON.parse(e.data);
		if(response.command == "Status"){
			document.getElementById ("view_ip").innerHTML = response.ip;
		}
		if(response.command == "Internal"){
			document.getElementById ("view_Internal").innerHTML = response.message;
		}
		if(response.command == "Response"){
			if(response.success == false){
				alert(response.message);
			}
		}
	};
	
	setInterval(function(){
		worker.postMessage(null);
	}, 500);
};

function SetBgColor()
{
	let r = document.getElementById ("BG_Color_R").value;
	let g = document.getElementById ("BG_Color_G").value;
	let b = document.getElementById ("BG_Color_B").value;

	let body = {"command": "BG_Color", "r": r*1.0, "g": g*1.0, "b":b*1.0}

	worker.postMessage(JSON.stringify(body));
}

function LoadVRM()
{
	let path = document.getElementById ("LoadVRM_Path").value;
	let body = {"command": "LoadVRM", "path": path}

	worker.postMessage(JSON.stringify(body));
}
function LoadVRMLicence()
{
	let path = document.getElementById ("LoadVRM_Path").value;
	let body = {"command": "LoadVRMLicence", "path": path}

	worker.postMessage(JSON.stringify(body));
}
function SetCamera()
{
	let zoom = document.getElementById ("Camera_Zoom").value;
	let fov = document.getElementById ("Camera_FOV").value;
	let angle = document.getElementById ("Camera_Angle").value;
	let tilt = document.getElementById ("Camera_Tilt").value;
	let height = document.getElementById ("Camera_Height").value;

	let body = {"command": "Camera","zoom":zoom,"fov":fov,"angle":angle,"tilt":tilt,"height":height}

	worker.postMessage(JSON.stringify(body));
}
