using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using UnityEngine;

namespace GTK.Download
{
    internal class Block
    {
        private const short INTERVAL = 5;
        public string Error { get; set; }
        private long start, end;
        private bool working;
        private WebFile webFile;
        private const int BUFFER_SIZE = 1024 * 1024 * 5;
        private string rootDirectory;
        private Stream stream;
        private float progress;
        private long downloadedSize;
        private string filePath;
        private long totalSize;
        private short id;
        public Block(short id, WebFile webFile, long downloadedSize, long s, long e)
        {
            this.id = id;
            this.webFile = webFile;
            this.start = s;
            this.end = e;
            this.totalSize = e - s + 1;
            this.downloadedSize = downloadedSize;
            Start();
        }

        public short ID
        {
            get
            {
                return id;
            }
        }
        public long DownloadedSize
        {
            get
            {
                return downloadedSize;
            }
        }

        public long TotalSize
        {
            get
            {
                return totalSize;
            }
        }

        public float Progress
        {
            get
            {
                return (float)downloadedSize / (float)totalSize;
            }
        }

        public void Start()
        {
            Thread t = new Thread(Download);
            t.Start();
            working = true;
        }

        public void Destory()
        {
            working = false;
        }

        private void Download()
        {
            try
            {
                Debug.LogFormat("Start Download:{0}, startPositon:{1}, endPosition:{2},totalSize:{3}", webFile.Url, this.start, this.end, this.totalSize);
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(webFile.Url);
                MethodInfo method = typeof(WebHeaderCollection).GetMethod("AddWithoutValidate", BindingFlags.Instance | BindingFlags.NonPublic);
                request.Method = "GET";
                request.KeepAlive = true;
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.45";
                int s = (int)(this.start + downloadedSize);
                s = s > this.end ? (int)this.end : s;
                string key = "Range";
                string val = string.Format("bytes={0}-{1}", s, this.end);
                method.Invoke(request.Headers, new object[] { key, val });
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                byte[] buffer = new byte[BUFFER_SIZE];
                using (FileStream stream = new FileStream(webFile.LocalPath + WebFileDownloader.FILE_FORMAT, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    stream.Seek(start + downloadedSize, SeekOrigin.Begin);
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        int size;
                        while (downloadedSize < totalSize && working)
                        {
                            size = responseStream.Read(buffer, 0, (int)buffer.Length);
                            webFile.DownloadedSize += size;
                            downloadedSize += size;
                            stream.Write(buffer, 0, size);

                            Thread.Sleep(INTERVAL);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Error = e.Message;
                working = false;
            }
        }
    }
}

