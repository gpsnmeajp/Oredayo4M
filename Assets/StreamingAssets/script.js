"use strict";
let view = document.getElementById ("view");
let worker;

let lastBrowse = "";
let lastBrowseVRM = "";

window.onload = ()=>{
	if (!window.Worker) {
		alert("Web Worker disabled! Editor won't work!")
	}
	
	try {
		worker = new Worker("worker.js");
	}catch (e) {
		addStat("Exception!(UI): "+e.message);
	}

	//受信処理
	worker.onmessage = function(e) {
		let response = JSON.parse(e.data);
		if(response.command == "Status"){
			document.getElementById ("view_ip").innerHTML = response.ip;
			document.getElementById ("view_AutoConnect").style.display = response.deviceFound ? "block":"none";
			document.getElementById ("view_Connection").style.display = response.connected ? "block":"none";

			if(lastBrowseVRM != response.lastBrowseVRM){
				lastBrowseVRM = response.lastBrowseVRM;
				document.getElementById ("LoadVRM_Path").value = response.lastBrowseVRM;
			}
			if(lastBrowse != response.lastBrowse){
				lastBrowse = response.lastBrowse;
				document.getElementById ("Settings_Path").value = response.lastBrowse;
			}
		}
		if(response.command == "Internal"){
			document.getElementById ("view_Internal").innerHTML = response.message;
		}
		if(response.command == "Response"){
			if(response.success == false){
				document.getElementById ("view_Message").innerHTML = response.message;
			}
		}
		if(response.command == "InitParam"){
			document.getElementById ("LoadVRM_Path").value = response.loadvrmPath;
			document.getElementById ("Settings_Path").value = response.settingPath;
		}
		if(response.command == "SaveData"){
			document.getElementById ("BG_Color_R").value = response.bgcolor.r;
			document.getElementById ("BG_Color_G").value = response.bgcolor.g;
			document.getElementById ("BG_Color_B").value = response.bgcolor.b;

			document.getElementById ("LoadVRM_Path").value = response.loadvrm.path;

			document.getElementById ("Camera_Zoom").value = response.camera.zoom;
			document.getElementById ("Camera_FOV").value = response.camera.fov;
			document.getElementById ("Camera_Angle").value = response.camera.angle;
			document.getElementById ("Camera_Tilt").value = response.camera.tilt;
			document.getElementById ("Camera_Height").value = response.camera.height;

			document.getElementById ("PPS_AntiAliasing_Enable").checked = response.pps.AntiAliasing_Enable;
			document.getElementById ("PPS_Bloom_Enable").checked = response.pps.Bloom_Enable;
			document.getElementById ("PPS_Bloom_Intensity").value = response.pps.Bloom_Intensity;
			document.getElementById ("PPS_Bloom_Threshold").value = response.pps.Bloom_Threshold;
			document.getElementById ("PPS_DepthOfField_Enable").checked = response.pps.DepthOfField_Enable;
			document.getElementById ("PPS_DepthOfField_FocusDistance").value = response.pps.DepthOfField_FocusDistance;
			document.getElementById ("PPS_DepthOfField_Aperture").value = response.pps.DepthOfField_Aperture;
			document.getElementById ("PPS_DepthOfField_FocusLength").value = response.pps.DepthOfField_FocusLength;
			document.getElementById ("PPS_DepthOfField_MaxBlurSize").value = response.pps.DepthOfField_MaxBlurSize;
			document.getElementById ("PPS_ColorGrading_Enable").checked = response.pps.ColorGrading_Enable;
			document.getElementById ("PPS_ColorGrading_Temperature").value = response.pps.ColorGrading_Temperature;
			document.getElementById ("PPS_ColorGrading_Saturation").value = response.pps.ColorGrading_Saturation;
			document.getElementById ("PPS_ColorGrading_Contrast").value = response.pps.ColorGrading_Contrast;
			document.getElementById ("PPS_Vignette_Enable").checked = response.pps.Vignette_Enable;
			document.getElementById ("PPS_Vignette_Intensity").value = response.pps.Vignette_Intensity;
			document.getElementById ("PPS_Vignette_Smoothness").value = response.pps.Vignette_Smoothness;
			document.getElementById ("PPS_Vignette_Rounded").value = response.pps.Vignette_Rounded;
			document.getElementById ("PPS_ChromaticAberration_Enable").checked = response.pps.ChromaticAberration_Enable;
			document.getElementById ("PPS_ChromaticAberration_Intensity").value = response.pps.ChromaticAberration_Intensity;

			SendAll();
			LoadVRM();
		}
	};
	
	//定期データ取得
	setInterval(function(){
		worker.postMessage(null);
	}, 500);

	//初回データ送信
	SendAll();
	Init();
};

function SetBgColor()
{
	let r = document.getElementById ("BG_Color_R").value;
	let g = document.getElementById ("BG_Color_G").value;
	let b = document.getElementById ("BG_Color_B").value;

	let body = {"command": "BG_Color", "r": r*1.0, "g": g*1.0, "b":b*1.0};

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

	let body = {"command": "Camera","zoom":zoom,"fov":fov,"angle":angle,"tilt":tilt,"height":height};

	worker.postMessage(JSON.stringify(body));
}

function Save()
{
	//反映
	SendAll();

	let path = document.getElementById ("Settings_Path").value;
	let body = {"command": "Save","path": path};
	worker.postMessage(JSON.stringify(body));
}

function Load()
{
	let path = document.getElementById ("Settings_Path").value;
	let body = {"command": "Load","path": path};
	worker.postMessage(JSON.stringify(body));
}

function AutoConnect()
{
	let body = {"command": "AutoConnect"}
	worker.postMessage(JSON.stringify(body));
}

function SetPPS()
{
	let body = {"command": "PPS"}
	body["AntiAliasing_Enable"] = document.getElementById ("PPS_AntiAliasing_Enable").checked;
	body["Bloom_Enable"] = document.getElementById ("PPS_Bloom_Enable").checked;
	body["Bloom_Intensity"] = document.getElementById ("PPS_Bloom_Intensity").value;
	body["Bloom_Threshold"] = document.getElementById ("PPS_Bloom_Threshold").value;
	body["DepthOfField_Enable"] = document.getElementById ("PPS_DepthOfField_Enable").checked;
	body["DepthOfField_FocusDistance"] = document.getElementById ("PPS_DepthOfField_FocusDistance").value;
	body["DepthOfField_Aperture"] = document.getElementById ("PPS_DepthOfField_Aperture").value;
	body["DepthOfField_FocusLength"] = document.getElementById ("PPS_DepthOfField_FocusLength").value;
	body["DepthOfField_MaxBlurSize"] = document.getElementById ("PPS_DepthOfField_MaxBlurSize").value;
	body["ColorGrading_Enable"] = document.getElementById ("PPS_ColorGrading_Enable").checked;
	body["ColorGrading_Temperature"] = document.getElementById ("PPS_ColorGrading_Temperature").value;
	body["ColorGrading_Saturation"] = document.getElementById ("PPS_ColorGrading_Saturation").value;
	body["ColorGrading_Contrast"] = document.getElementById ("PPS_ColorGrading_Contrast").value;
	body["Vignette_Enable"] = document.getElementById ("PPS_Vignette_Enable").checked;
	body["Vignette_Intensity"] = document.getElementById ("PPS_Vignette_Intensity").value;
	body["Vignette_Smoothness"] = document.getElementById ("PPS_Vignette_Smoothness").value;
	body["Vignette_Rounded"] = document.getElementById ("PPS_Vignette_Rounded").value;
	body["ChromaticAberration_Enable"] = document.getElementById ("PPS_ChromaticAberration_Enable").checked;
	body["ChromaticAberration_Intensity"] = document.getElementById ("PPS_ChromaticAberration_Intensity").value;

	worker.postMessage(JSON.stringify(body));
}
function ResetPPS()
{
	document.getElementById ("PPS_ColorGrading_Temperature").value = 0.0;
	document.getElementById ("PPS_ColorGrading_Saturation").value = 0.0;
	document.getElementById ("PPS_ColorGrading_Contrast").value = 0.0;
	SetPPS();
}

function Browse()
{
	let body = {"command": "Browse"}
	worker.postMessage(JSON.stringify(body));
}

function BrowseVRM()
{
	let body = {"command": "BrowseVRM"}
	worker.postMessage(JSON.stringify(body));
}

function Init()
{
	let body = {"command": "Init"};
	worker.postMessage(JSON.stringify(body));
}

function SendAll()
{
	SetBgColor();
	SetCamera();
	SetPPS();
	//VRMは送ると再ロードが走るためナシ
}