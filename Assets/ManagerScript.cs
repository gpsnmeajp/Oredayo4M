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
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using EVMC4U;
using VRM;
using VRMLoader;
using SimpleFileBrowser;

public class ManagerScript : MonoBehaviour
{
    public ExternalReceiver receiver;
    public MeshRenderer BackgroundSphere;

    public Canvas m_canvas;
    public GameObject m_modalWindowPrefab;
    private GameObject modalObject = null;

    public Transform cameraBase;
    public Camera cameraBody;

    public PostProcessLayer PPSLayer;
    public PostProcessVolume PPSVolume;

    public EasyDeviceDiscoveryProtocolClient.Requester requester;
    public EVMC4U.CommunicationValidator communicationValidator;

    CMD_SaveData saveData = new CMD_SaveData(); //コマンドであり、セーブデータ構造体である

    SynchronizationContext synchronizationContext;
    HTTP http;

    float lastPacketTime = 0.0f;

    CMD_Status status = new CMD_Status();

    void Start()
    {
        synchronizationContext = SynchronizationContext.Current; //メインスレッドのコンテキストを保存

        receiver.LoadVRM(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments),"default.vrm"));
        http = new HTTP("http://127.0.0.1:39451/");
        http.processor = CommandProcessor;

        System.Diagnostics.Process.Start("http://127.0.0.1:39451/");

        //IPアドレスリスト
        IPAddress[] ips = Dns.GetHostAddresses(Dns.GetHostName());
        status.ip = "";
        foreach (var ip in ips)
        {
            //IPv4のみ
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                status.ip += ip.ToString() + "<br>";
            }
        }
        status.ip = status.ip.Trim();


        //初期化
        saveData.loadvrm = new CMD_LoadVRM();
        saveData.camera = new CMD_Camera();
        saveData.bgcolor = new CMD_BG_Color();
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
            status.connected = false;
            if (lastPacketTime != communicationValidator.time)
            {
                status.connected = true;
                lastPacketTime = communicationValidator.time;
            }

            return JsonUtility.ToJson(status);
        }

        //Jsonをコマンド解析
        var c = JsonUtility.FromJson<CMD_Command>(commandJson);
        Debug.Log(c.command);

        //各コマンド処理
        if (c.command == "Load")
        {
            //Jsonを詳細解析
            var d = JsonUtility.FromJson<CMD_Load>(commandJson);
            synchronizationContext.Post(_ => {
                FileBrowser.HideDialog();
                if (modalObject != null)
                {
                    Destroy(modalObject);
                    modalObject = null;
                }
            }, null);

            if (File.Exists(d.path))
            {
                string data = File.ReadAllText(d.path);
                Debug.Log("Load: " + d.path);
                saveData = JsonUtility.FromJson<CMD_SaveData>(data);
                return JsonUtility.ToJson(saveData);
            }
            else {
                return JsonUtility.ToJson(new CMD_Response
                {
                    success = false,
                    message = "File not found",
                });
            }
        }
        else if (c.command == "Save")
        {
            //Jsonを詳細解析
            var d = JsonUtility.FromJson<CMD_Save>(commandJson);
            synchronizationContext.Post(_ => {
                FileBrowser.HideDialog();
            }, null);

            File.WriteAllText(d.path, JsonUtility.ToJson(saveData), new UTF8Encoding(false));
            Debug.Log("Save: " + d.path);
            return JsonUtility.ToJson(new CMD_Response
            {
                success = true,
                message = "OK",
            });
        }
        else if (c.command == "Browse")
        {
            synchronizationContext.Post(_ => {
                FileBrowser.SetFilters(true, new FileBrowser.Filter("json",".json"));
                FileBrowser.SetDefaultFilter(".json");
                StartCoroutine(ShowLoadDialogCoroutine());
            }, null);
            return JsonUtility.ToJson(new CMD_Response
            {
                success = true,
                message = "OK",
            });
        }
        else if (c.command == "BrowseVRM")
        {
            synchronizationContext.Post(_ => {
                FileBrowser.SetFilters(true, new FileBrowser.Filter("VRM",".vrm"));
                FileBrowser.SetDefaultFilter(".vrm");
                StartCoroutine(ShowLoadDialogCoroutineForVRM());
            }, null);
            return JsonUtility.ToJson(new CMD_Response
            {
                success = true,
                message = "OK",
            });
        }
        else if (c.command == "Init")
        {
            synchronizationContext.Post(_ => {
                FileBrowser.HideDialog();
            }, null);
            return JsonUtility.ToJson(new CMD_InitParam
            {
                loadvrmPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "default.vrm"),
                settingPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "Oredayo4V_Setting.json"),
            });
        }
        else if (c.command == "AutoConnect")
        {
            status.deviceFound = false;
            //メインスレッドに渡す
            synchronizationContext.Post(_ => {
                requester.StartDiscover(() => {
                    status.deviceFound = true;
                });
            }, null);
            return JsonUtility.ToJson(new CMD_Response
            {
                success = true,
                message = "OK",
            });
        }
        else if (c.command == "BG_Color")
        {
            //Jsonを詳細解析
            var d = JsonUtility.FromJson<CMD_BG_Color>(commandJson);
            //記録する
            saveData.bgcolor = d;

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
            //空パスならデフォルトを読み込む
            if (d.path == "")
            {
                d.path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/default.vrm";
            }
            //記録する
            saveData.loadvrm = d;

            if (File.Exists(d.path))
            {
                //メインスレッドに渡す
                synchronizationContext.Post(_ =>
                {
                    FileBrowser.HideDialog();

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
            //空パスならデフォルトを読み込む
            if (d.path == "")
            {
                d.path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/default.vrm";
            }

            if (File.Exists(d.path))
            {
                //メインスレッドに渡す
                synchronizationContext.Post(_ =>
                {
                    FileBrowser.HideDialog();

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
            //記録する
            saveData.camera = d;

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
        else if (c.command == "PPS")
        {
            //Jsonを詳細解析
            var d = JsonUtility.FromJson<CMD_PPS>(commandJson);
            //記録する
            saveData.pps = d;

            //メインスレッドに渡す
            synchronizationContext.Post(_ => {
                //アンチエイリアス
                if (d.AntiAliasing_Enable)
                {
                    PPSLayer.antialiasingMode = PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing;
                }
                else
                {
                    PPSLayer.antialiasingMode = PostProcessLayer.Antialiasing.None;
                }

                var p = PPSVolume.sharedProfile;

                //ブルーム
                var bloom = p.GetSetting<Bloom>();
                bloom.active = true;
                bloom.enabled.value = d.Bloom_Enable;
                bloom.intensity.value = d.Bloom_Intensity;
                bloom.threshold.value = d.Bloom_Threshold;

                //DoF
                var dof = p.GetSetting<DepthOfField>();
                dof.active = true;
                dof.enabled.value = d.DepthOfField_Enable;
                dof.focusDistance.value = d.DepthOfField_FocusDistance;
                dof.aperture.value = d.DepthOfField_Aperture;
                dof.focalLength.value = d.DepthOfField_FocusLength;
                switch (d.DepthOfField_MaxBlurSize)
                {
                    case 1:
                        dof.kernelSize.value = KernelSize.Small; break;
                    case 2:
                        dof.kernelSize.value = KernelSize.Medium; break;
                    case 3:
                        dof.kernelSize.value = KernelSize.Large; break;
                    case 4:
                        dof.kernelSize.value = KernelSize.VeryLarge; break;
                    default:
                        dof.kernelSize.value = KernelSize.Medium; break;
                }

                //CG
                var cg = p.GetSetting<ColorGrading>();
                cg.active = true;
                cg.enabled.value = d.ColorGrading_Enable;
                cg.temperature.value = d.ColorGrading_Temperature;
                cg.saturation.value = d.ColorGrading_Saturation;
                cg.contrast.value = d.ColorGrading_Contrast;

                var v = p.GetSetting<Vignette>();
                v.active = true;
                v.enabled.value = d.Vignette_Enable;
                v.intensity.value = d.Vignette_Intensity;
                v.smoothness.value = d.Vignette_Smoothness;
                v.roundness.value = d.Vignette_Rounded;

                var ca = p.GetSetting<ChromaticAberration>();
                ca.active = true;
                ca.enabled.value = d.ChromaticAberration_Enable;
                ca.intensity.value = d.ChromaticAberration_Intensity;

                PPSVolume.sharedProfile = p;

            }, null);

            return JsonUtility.ToJson(new CMD_Response
            {
                success = true,
                message = "OK",
            });
        }
        else if (c.command == "Filter")
        {
            //Jsonを詳細解析
            var d = JsonUtility.FromJson<CMD_Filter>(commandJson);
            //記録する
            saveData.filter = d;

            //メインスレッドに渡す
            synchronizationContext.Post(_ => {
                receiver.BoneFilter = d.bone;
                receiver.BlendShapeFilter = d.blendShape;
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

    IEnumerator ShowLoadDialogCoroutine()
    {
        yield return FileBrowser.WaitForSaveDialog(false, true, null, "Setting File", "Select");
        Debug.Log(FileBrowser.Success);
        if (FileBrowser.Success)
        {
            status.lastBrowse = FileBrowser.Result[0];
        }
    }

    IEnumerator ShowLoadDialogCoroutineForVRM()
    {
        yield return FileBrowser.WaitForLoadDialog(false, true, null, "VRM File", "Select");
        Debug.Log(FileBrowser.Success);
        if (FileBrowser.Success)
        {
            status.lastBrowseVRM = FileBrowser.Result[0];
        }
    }
}

