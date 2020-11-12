/*
MIT License

Copyright (c) 2020 gpsnmeajp

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using System;
using System.Threading;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class HTTP : IDisposable
{
    HttpListener listener;
    Task thread = null;
    public Func<string,string> processor;
    string adr = "";
    string responseBody = "{}";
    public void SetResponse(string res)
    {
        responseBody = res;
    }

    public HTTP(string adr)
    {
        this.adr = adr;
        listener = new HttpListener();
        listener.Prefixes.Add(adr);

        Console.WriteLine("### View server started on " + adr);
        listener.Start();

        //受信処理スレッド
        thread = new Task(() => { ReceiveThread(); });
        thread.Start();

    }
    private async void ReceiveThread()
    {
        while (listener.IsListening)
        {
            HttpListenerContext context = listener.GetContext();
            HttpListenerRequest request = context.Request;
            
            //Debug.Log(request.Url.LocalPath);

            HttpListenerResponse response = context.Response;
            string res = "";
            response.StatusCode = 200;
            response.ContentType = "text/html";

            try
            {
                //Debug.Log(request.Url.LocalPath);
                switch (request.Url.LocalPath)
                {
                    case "/":
                        res = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "index.htm"), new UTF8Encoding(false));
                        response.ContentType = "text/html";
                        break;
                    case "/info.dat":
                        res = responseBody;
                        response.ContentType = "application/json";
                        break;
                    case "/command.dat":
                        res = "200 OK";
                        string content = null;
                        if (request.HasEntityBody)
                        {
                            using (var body = request.InputStream)
                            {
                                var encoding = request.ContentEncoding;
                                using (var reader = new StreamReader(body, encoding))
                                {
                                    content = reader.ReadToEnd();
                                    body.Close();
                                }
                            }
                            Debug.Log(content);
                        }
                        res = processor(content);
                        response.ContentType = "application/json";
                        break;
                    case "/script.js":
                        res = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "script.js"), new UTF8Encoding(false));
                        response.ContentType = "text/javascript";
                        break;
                    case "/worker.js":
                        res = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "worker.js"), new UTF8Encoding(false));
                        response.ContentType = "text/javascript";
                        break;
                    case "/style.css":
                        res = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "style.css"), new UTF8Encoding(false));
                        response.ContentType = "text/css";
                        break;
                    case "/mvp.css":
                        res = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "mvp.css"), new UTF8Encoding(false));
                        response.ContentType = "text/css";
                        break;
                    default:
                        res = "404 Not found";
                        response.StatusCode = 404;
                        response.ContentType = "text/html";
                        break;
                }
            }
            catch (Exception e)
            {
                response.StatusCode = 500;
                res = "Internal Server Error";
                Debug.Log(e);
            }

            byte[] buf = new UTF8Encoding(false).GetBytes(res);
            response.OutputStream.Write(buf, 0, buf.Length);
            response.OutputStream.Close();

            await Task.Delay(30);
        }
    }

    public void Dispose()
    {
        listener.Close();
    }
}
