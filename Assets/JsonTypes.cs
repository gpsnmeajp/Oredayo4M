using System;
using System.Collections;
using System.Collections.Generic;


//ステータス情報
public class Status
{
    public string ip;
}

//共通: コマンド部分だけ取り出す
public class Command {
    public string command;
}

public class BG_Color
{
    public string command;
    public float r;
    public float g;
    public float b;
}

public class LoadVRM
{
    public string command;
    public string path;
}
