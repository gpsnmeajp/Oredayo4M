/*
 * ExternalReceiver
 * https://sabowl.sakura.ne.jp/gpsnmeajp/
 *
 * MIT License
 * 
 * Copyright (c) 2021 gpsnmeajp
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

#pragma warning disable 0414,0219
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Profiling;
using VRM;

using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Rendering.PostProcessing;
using EVMC4U;
using VRMLoader;
using SimpleFileBrowser;

using DVRSDK.Auth;
using DVRSDK.Utilities;
using DVRSDK.Serializer;
using DVRSDK.Avatar;


namespace EVMC4U
{
    public class RemoteReceiver : MonoBehaviour, IExternalReceiver
    {
        [Header("RemoteReceiver v1.0")]
        public ManagerScript manager;
        private string user_id;
        private string avatar_id;

        [SerializeField]
        private string StatusMessage = "";  //Inspector表示用

        [Header("Daisy Chain")]
        public GameObject[] NextReceivers = new GameObject[1];

        private ExternalReceiverManager externalReceiverManager = null;
        bool shutdown = false;

        Vector3 pos;
        Quaternion rot;
        Color col;

        SynchronizationContext synchronizationContext;

        void Start()
        {
            synchronizationContext = SynchronizationContext.Current; //メインスレッドのコンテキストを保存
            externalReceiverManager = new ExternalReceiverManager(NextReceivers);
            StatusMessage = "Waiting for Master...";
        }

        //デイジーチェーンを更新
        public void UpdateDaisyChain()
        {
            externalReceiverManager.GetIExternalReceiver(NextReceivers);
        }

        void Update()
        {
        }

        public void MessageDaisyChain(ref uOSC.Message message, int callCount)
        {
            //Startされていない場合無視
            if (externalReceiverManager == null || enabled == false || gameObject.activeInHierarchy == false)
            {
                return;
            }

            if (shutdown)
            {
                return;
            }

            StatusMessage = "OK";

            //異常を検出して動作停止
            try
            {
                ProcessMessage(ref message);
            }
            catch (Exception e)
            {
                StatusMessage = "Error: Exception";
                Debug.LogError(" --- Communication Error ---");
                Debug.LogError(e.ToString());
                shutdown = true;
                return;
            }

            if (!externalReceiverManager.SendNextReceivers(message, callCount))
            {
                StatusMessage = "Infinite loop detected!";
                shutdown = true;
            }
        }

        class dmmvrconnect {
            public string user_id;
            public string avatar_id;
        }

        private void ProcessMessage(ref uOSC.Message message)
        {
            //メッセージアドレスがない、あるいはメッセージがない不正な形式の場合は処理しない
            if (message.address == null || message.values == null)
            {
                StatusMessage = "Bad message.";
                return;
            }

            if (manager.receiver.isLoading) {
                //ローカル読込中は処理しない
                return;
            }

            if (manager.status.DVRC_AuthState != "AUTHENTICATION_OK")
            {
                //ログインしていない場合は受け付けない
                return;
            }

            if (message.address == "/VMC/Ext/Remote"
                && (message.values[0] is string) //service
                && (message.values[1] is string) //json
                )
            {
                string service = message.values[0] as string;
                string json = message.values[1] as string;

                if (service == "dmmvrconnect")
                {
                    var connect = JsonUtility.FromJson<dmmvrconnect>(json);
                    if (user_id != connect.user_id || avatar_id != connect.avatar_id) {
                        user_id = connect.user_id;
                        avatar_id = connect.avatar_id;

                        //メインスレッドに渡す
                        synchronizationContext.Post(async _ => {
                            Debug.Log("Avatar loading from Connect...");
                            var current_user = await Authentication.Instance.Okami.GetCurrentUserAsync();
                            if (user_id == current_user.id)
                            {
                                var avatar = await Authentication.Instance.Okami.GetAvatarAsync(current_user.id, avatar_id);
                                Debug.Log(avatar);
                                if (avatar != null)
                                {
                                    await manager.LoadAvatarFromDVRSDK(avatar);
                                }
                                else
                                {
                                    Debug.LogError("Avatar loading from Connect... Failed!");
                                }
                                Debug.Log("Load from connect OK");
                            }
                            else {
                                Debug.Log("User id unmatch");
                            }
                        }, null);
                    }

                }
                else {
                    StatusMessage = "Unknown service: " + service;
                }

            }
        }
    }
}