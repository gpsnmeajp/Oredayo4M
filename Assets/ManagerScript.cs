﻿/*
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EVMC4U;

public class ManagerScript : MonoBehaviour
{
    public ExternalReceiver receiver;
    HTTP http;
    void Start()
    {
        receiver.LoadVRM(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments)+"/default.vrm");
        http = new HTTP("http://127.0.0.1:8000/");
        http.processor = (s) =>
        {
            if (s == null) {
                return "OK";
            }
            var c = JsonUtility.FromJson<Command>(s);
            Debug.Log(c.command);

            return c.command;
        };
    }

    void Update()
    {
        
    }

    private void OnApplicationQuit()
    {
        http.Dispose();
    }
}
