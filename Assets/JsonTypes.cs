using System;
using System.Collections;
using System.Collections.Generic;


//ステータス情報
[Serializable]
public class RES_Status
{
    public string command = "Status";
    public string ip = null;
    public bool deviceFound = false;
    public bool connected = false;
    public string lastBrowse = "";
    public string lastBrowseVRM = "";
    public string DVRC_AuthState = null;
    public string DVRC_AuthUri = null;
    public string DVRC_AuthKey = null;
    public string[] DVRC_Avatars = null;
}
//レスポンス
[Serializable]
public class RES_Response
{
    public string command = "Response";
    public bool success;
    public string message;
}

//初期値応答
[Serializable]
public class CMD_InitParam
{
    public string command = "InitParam";
    public string loadvrmPath;
    public string settingPath;
}


//共通: コマンド部分だけ取り出す
[Serializable]
public class CMD_Command
{
    public string command;
}

[Serializable]
public class CMD_BG_Color
{
    public string command;
    public float r;
    public float g;
    public float b;
}

[Serializable]
public class CMD_LoadVRM
{
    public string command;
    public string path;
}

[Serializable]
public class CMD_LoadVRMLicence
{
    public string command;
    public string path;
}

[Serializable]
public class CMD_Camera
{
    public string command;
    public float zoom;
    public float fov;
    public float angle;
    public float tilt;
    public float height;
}

[Serializable]
public class CMD_Save
{
    public string command;
    public string path;
}

[Serializable]
public class CMD_Load
{
    public string command;
    public string path;
}

[Serializable]
public class CMD_PPS
{
    public string command;
    public bool AntiAliasing_Enable;
    public bool Bloom_Enable;
    public float Bloom_Intensity;
    public float Bloom_Threshold;
    public bool DepthOfField_Enable;
    public float DepthOfField_FocusDistance;
    public float DepthOfField_Aperture;
    public float DepthOfField_FocusLength;
    public float DepthOfField_MaxBlurSize;
    public bool ColorGrading_Enable;
    public float ColorGrading_Temperature;
    public float ColorGrading_Saturation;
    public float ColorGrading_Contrast;
    public bool Vignette_Enable;
    public float Vignette_Intensity;
    public float Vignette_Smoothness;
    public float Vignette_Rounded;
    public bool ChromaticAberration_Enable;
    public float ChromaticAberration_Intensity;
}

[Serializable]
public class CMD_Filter
{
    public string command;
    public float bone;
    public float blendShape;
}

[Serializable]
public class CMD_LoadDVRC
{
    public string command;
    public int index;
}

[Serializable]
public class RES_SaveData
{
    public string command = "SaveData";
    public CMD_LoadVRM loadvrm;
    public CMD_Camera camera;
    public CMD_BG_Color bgcolor;
    public CMD_PPS pps;
    public CMD_Filter filter;
}

