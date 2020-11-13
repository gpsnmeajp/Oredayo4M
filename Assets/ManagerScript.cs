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
using System.IO;
using System.Net;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EVMC4U;
using VRM;
using VRMLoader;

public class ManagerScript : MonoBehaviour
{
    public ExternalReceiver receiver;
    public MeshRenderer BackgroundSphere;

    public Canvas m_canvas;
    public GameObject m_modalWindowPrefab;
    private GameObject modalObject = null;

    public Transform cameraBase;
    public Camera cameraBody;

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
                ipList += ip.ToString() + "<br>";
            }
        }
        ipList = ipList.Trim();
    }

    void Update()
    {
        
    }

    private void OnApplicationQuit()
    {
        if (http != null) {
            http.Dispose();
        }
    }

    //コマンド処理
    string CommandProcessor(string commandJson)
    {
        //汎用ステータス応答
        if (commandJson == null)
        {
            return JsonUtility.ToJson(new CMD_Status
            {
                ip = ipList
            });
        }

        //Jsonをコマンド解析
        var c = JsonUtility.FromJson<CMD_Command>(commandJson);
        Debug.Log(c.command);

        //各コマンド処理
        if (c.command == "BG_Color")
        {
            //Jsonを詳細解析
            var d = JsonUtility.FromJson<CMD_BG_Color>(commandJson);
            //メインスレッドに渡す
            synchronizationContext.Post(_ => {
                BackgroundSphere.material.color = new Color(d.r, d.g, d.b);
            }, null);

            return JsonUtility.ToJson(new CMD_Response
            {
                success = true,
                message = "OK",
            });
        }
        else if (c.command == "LoadVRM")
        {
            //Jsonを詳細解析
            var d = JsonUtility.FromJson<CMD_LoadVRM>(commandJson);
            if (File.Exists(d.path))
            {
                //メインスレッドに渡す
                synchronizationContext.Post(_ =>
                {
                    if (modalObject != null)
                    {
                        Destroy(modalObject);
                        modalObject = null;
                    }

                    receiver.LoadVRM(d.path);
                }, null);

                return JsonUtility.ToJson(new CMD_Response
                {
                    success = true,
                    message = "OK",
                });
            }
            else
            {
                return JsonUtility.ToJson(new CMD_Response
                {
                    success = false,
                    message = "File not found",
                });
            }

        }
        else if (c.command == "LoadVRMLicence")
        {
            //Jsonを詳細解析
            var d = JsonUtility.FromJson<CMD_LoadVRMLicence>(commandJson);

            if (File.Exists(d.path))
            {
                //メインスレッドに渡す
                synchronizationContext.Post(_ =>
                {
                    byte[] bytes = File.ReadAllBytes(d.path);

                    var context = new VRMImporterContext();
                    context.ParseGlb(bytes);
                    var meta = context.ReadMeta(true);

                    if (modalObject != null)
                    {
                        Destroy(modalObject);
                        modalObject = null;
                    }

                    modalObject = Instantiate(m_modalWindowPrefab, m_canvas.transform) as GameObject;
                    var modalLocale = modalObject.GetComponentInChildren<VRMPreviewLocale>();
                    modalLocale.SetLocale("en");

                    var modalUI = modalObject.GetComponentInChildren<VRMPreviewUI>();
                    modalUI.setMeta(meta);
                }, null);

                return JsonUtility.ToJson(new CMD_Response
                {
                    success = true,
                    message = "OK",
                });
            }
            else {
                return JsonUtility.ToJson(new CMD_Response
                {
                    success = false,
                    message = "File not found",
                });
            }
        }
        else if (c.command == "Camera")
        {
            //Jsonを詳細解析
            var d = JsonUtility.FromJson<CMD_Camera>(commandJson);
            //メインスレッドに渡す
            synchronizationContext.Post(_ => {
                //カメラ制御
                cameraBase.transform.localRotation = Quaternion.Euler(d.tilt, d.angle, 0);
                cameraBody.transform.localRotation = Quaternion.Euler(0, 180, 0);
                cameraBody.transform.localPosition = new Vector3(0, d.height, d.zoom);
                cameraBody.fieldOfView = d.fov;
            }, null);

            return JsonUtility.ToJson(new CMD_Response
            {
                success = true,
                message = "OK",
            });
        }

        return JsonUtility.ToJson(new CMD_Response
        {
            success = false,
            message = "Command not found",
        });
    }
}

