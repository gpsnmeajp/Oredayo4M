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
			view.innerHTML = e.data;
	};
	
	setInterval(function(){
		worker.postMessage([true,null]);
	}, 500);
};

function SetBgColor()
{
	let r = document.getElementById ("BG_Color_R").value;
	let g = document.getElementById ("BG_Color_G").value;
	let b = document.getElementById ("BG_Color_B").value;

	let body = {command: "BG_Color", R: r*1.0, G: g*1.0, B:b*1.0}

	worker.postMessage([false,JSON.stringify(body)]);
}