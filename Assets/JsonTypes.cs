using System;
using System.Collections;
using System.Collections.Generic;


//ステータス情報
public class CMD_Status
{
    public string command = "Status";
    public string ip;
}
//レスポンス
public class CMD_Response
{
    public string command = "Response";
    public bool success;
    public string message;
}


//共通: コマンド部分だけ取り出す
public class CMD_Command
{
    public string command;
}

public class CMD_BG_Color
{
    public string command;
    public float r;
    public float g;
    public float b;
}

public class CMD_LoadVRM
{
    public string command;
    public string path;
}

public class CMD_LoadVRMLicence
{
    public string command;
    public string path;
}
public class CMD_Camera
{
    public string command;
    public float zoom;
    public float fov;
    public float angle;
    public float tilt;
    public float height;
}
