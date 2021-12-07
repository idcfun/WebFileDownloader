using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using UnityEngine;

namespace GTK.Download
{
    public class WebFileDownloader
    {

        public static List<WebFileDownloader> Downloaders = new List<WebFileDownloader>();
        public static WebFileDownloader StartDownload(string url, string directory, short threadCount)
        {
            WebFileDownloader d = new WebFileDownloader(url, directory, threadCount);
            d.Start();
            Downloaders.Add(d);
            return d;
        }

        public static void StopDownload()
        {
            for (int i = 0; i < Downloaders.Count; i++)
            {
                Downloaders[i].Destory();
            }

            Downloaders.Clear();
        }
        
        public const string FILE_FORMAT = ".download";

        private List<Block> blocks = new List<Block>();
        private short downloadThreadCount = 1;
        private string url;
        private bool running;
        private WebFile webFile;
        private FileStream stream;
        private string downloadDirectory;
        public WebFileDownloader(string url,string directory,short downloadThreadCount = 2)
        {
            this.url = url;
            this.downloadDirectory = directory;
            this.downloadThreadCount = downloadThreadCount;
            webFile = new WebFile();
            ServicePointManager.ServerCertificateValidationCallback = (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) =>
            {
                return true;
            };

            ServicePointManager.DefaultConnectionLimit = 512;
        }

        public string Name
        {
            get
            {
                return webFile.Name;
            }
        }

        public int GetBlockCount
        {
            get
            {
                return blocks.Count;
            }
        }

        public float GetProgress(int index)
        {
            return blocks[index].Progress;
        }

        public void Start()
        {
            running = true;
            Thread t = new Thread(Run);
            t.Start();
        }

        private void Run()
        {
            Download(this.url);

            while (running && Progress < 1)
            {
                Thread.Sleep(5);
            }

            if (Progress >= 1)
            {
                try
                {
                    string s = "";
                    for (int i = 0; i < blocks.Count; i++)
                    {
                        s += "[" + blocks[i].ID + " " + (float)blocks[i].DownloadedSize + "/" + (float)blocks[i].TotalSize + " ] ";
                    }
                    Debug.Log(s);

                    string tempPath = webFile.LocalPath + FILE_FORMAT;
                    if (File.Exists(tempPath))
                    {
                        if (File.Exists(webFile.LocalPath))
                        {
                            File.Delete(webFile.LocalPath);
                        }

                        Debug.Log(tempPath);
                        using (FileStream stream = new FileStream(webFile.LocalPath + WebFileDownloader.FILE_FORMAT, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                        {
                            stream.SetLength(webFile.TotalSize);
                        }

                        File.Move(tempPath, webFile.LocalPath);
                        Debug.Log(webFile.LocalPath);
                    }
                }
                catch (IOException e)
                {
                    Debug.Log(e);
                }
            }
            else {
                using (FileStream stream = new FileStream(webFile.LocalPath + WebFileDownloader.FILE_FORMAT, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    stream.Seek(webFile.TotalSize, SeekOrigin.Begin);
                    byte[] bytes;
                    for (int i = 0; i < blocks.Count; i++)
                    {
                        bytes = BitConverter.GetBytes(blocks[i].DownloadedSize);
                        stream.Write(bytes, 0, bytes.Length);
                        Debug.Log(blocks[i].DownloadedSize);
                    }
                    bytes = BitConverter.GetBytes(blocks.Count);
                    Debug.Log(blocks.Count);
                    stream.Write(bytes, 0, bytes.Length);
                }
            }
        }

        public void Download(string url)
        {
            long totalSize = GetTotalSize(new Uri(url));
            webFile.Url = url;
            webFile.TotalSize = totalSize;
            webFile.Name = Path.GetFileName(url);
            webFile.LocalPath = this.downloadDirectory + webFile.Name;
            Debug.Log("Start to download file [source: " + url + "total size:" + totalSize + "]");

            long offset = 0;
            long end = 0;
            long blockSize = 0;
            if (File.Exists(webFile.LocalPath + FILE_FORMAT))
            {
                using (FileStream stream = new FileStream(webFile.LocalPath + FILE_FORMAT, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    stream.Seek(-4, SeekOrigin.End);
                    byte[] bytes = new byte[4];
                    stream.Read(bytes, 0, 4);
                    downloadThreadCount = (short)BitConverter.ToInt32(bytes,0);
                    Debug.Log(downloadThreadCount);
                    stream.Seek(-(4 + 8 * downloadThreadCount), SeekOrigin.End);
                    blockSize = totalSize / downloadThreadCount;
                    if (totalSize % downloadThreadCount > 0)
                    {
                        blockSize += 1;
                    }

                    for (int i = 0; i < downloadThreadCount; i++)
                    {
                        if (i < downloadThreadCount - 1)
                        {
                            end = offset + blockSize - 1;
                        }
                        else
                        {
                            end = totalSize - 1;
                        }
                        bytes = new byte[8];
                        stream.Read(bytes, 0, 8);
                        long size = BitConverter.ToInt64(bytes,0);
                        Block block = new Block((short)i, webFile, size, offset, end);
                        blocks.Add(block);
                        offset += blockSize;
                    }
                    bytes = BitConverter.GetBytes(blocks.Count);
                }
            }
            else
            {
                blockSize = totalSize / downloadThreadCount;
                if(totalSize % downloadThreadCount > 0)
                {
                    blockSize += 1;
                }
                
                for (int i = 0; i < downloadThreadCount; i++)
                {
                    if(i < downloadThreadCount - 1)
                    {
                        end = offset + blockSize - 1;
                    }
                    else
                    {
                        end = totalSize - 1;
                    }

                    Block block = new Block((short)i, webFile,0, offset, end);
                    blocks.Add(block);
                    offset += blockSize;
                }
            }
        }

        private long GetTotalSize(Uri uri)
        {
            string path = uri.GetLeftPart(UriPartial.Path);
            try
            {
                HttpWebRequest request = HttpWebRequest.Create(uri) as HttpWebRequest;
                request.Method = "HEAD";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.45";
                request.Timeout = 10000;
                long size = 0;
                using (WebResponse response = request.GetResponse())
                {
                    size = response.ContentLength;
                }

                return size;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public float Progress
        {
            get
            {
                float progress = 0;
                for (int i = 0; i < blocks.Count; i++)
                {
                    progress += blocks[i].Progress;
                }

                return progress / (float)blocks.Count;
            }
        }

        public void Destory()
        {
            running = false;
            for (int i = 0; i < blocks.Count; i++)
            {
                blocks[i].Destory();
            }
        }
    }
}

