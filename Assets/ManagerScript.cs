/*
 * Oredayo4M
 *
 * MIT License
 * 
 * Copyright (c) 2020 gpsnmeajp
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */
using System;
using System.Net;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EVMC4U;

public class ManagerScript : MonoBehaviour
{
    public ExternalReceiver receiver;
    public MeshRenderer BackgroundSphere;

    SynchronizationContext synchronizationContext;
    HTTP http;
    string ipList = "";

    const string url = "http://127.0.0.1:8000/";
    void Start()
    {
        synchronizationContext = SynchronizationContext.Current; //メインスレッドのコンテキストを保存

        receiver.LoadVRM(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments)+"/default.vrm");
        http = new HTTP(url);
        http.processor = CommandProcessor;

        System.Diagnostics.Process.Start(url);

        //IPアドレスリスト
        IPAddress[] ips = Dns.GetHostAddresses(Dns.GetHostName());
        ipList = "";
        foreach (var ip in ips)
        {
            //IPv4のみ
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                ipList += ip.ToString() + "\n";
            }
        }
        ipList = ipList.Trim();
    }

    void Update()
    {
        
    }

    private void OnApplicationQuit()
    {
        http.Dispose();
    }

    //コマンド処理
    string CommandProcessor(string commandJson)
    {
        //汎用ステータス応答
        if (commandJson == null)
        {
            return JsonUtility.ToJson(new Status
            {
                ip = ipList
            });
        }

        //Jsonをコマンド解析
        var c = JsonUtility.FromJson<Command>(commandJson);
        Debug.Log(c.command);

        //各コマンド処理
        if (c.command == "BG_Color")
        {
            //Jsonを詳細解析
            var d = JsonUtility.FromJson<BG_Color>(commandJson);
            //メインスレッドに渡す
            synchronizationContext.Post(_ => {
                BackgroundSphere.material.color = new Color(d.r, d.g, d.b);
            }, null);
            return "OK";
        }
        else if (c.command == "LoadVRM")
        {
            //Jsonを詳細解析
            var d = JsonUtility.FromJson<LoadVRM>(commandJson);
            //メインスレッドに渡す
            synchronizationContext.Post(_ => {
                receiver.LoadVRM(d.path);
            }, null);
            return "OK";
        }


        return "Command not found";
    }
}
