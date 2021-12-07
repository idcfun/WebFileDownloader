using GTK.Download;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DownloadFileExample : MonoBehaviour {
    [SerializeField] private string[] urls;
    [SerializeField] private Text downloadSpeedText;
    [SerializeField] private Text progressText;
    WebFileDownloader httpDownloader;
    int accumulatedSize = 0;

    // Use this for initialization
    void Start () {
        //downloader = new WebFileDownloader(url);
        //StartCoroutine(downloader.Start("D:/DownloadFiles"));

        startTime = Time.time;
        for(int i = 0; i < urls.Length; i++)
        {
            WebFileDownloader.StartDownload(urls[i],"D:/", 3);
        }
    }

    private float startTime;
    // Update is called once per frame
    void Update()
    {
        // Debug.Log(downloader.Progress);
        if (Time.time - startTime >= 1)
        {
            //downloadSpeedText.text = (httpDownloader.IncreasedSize / 1024) + "kb/s";
            //progressText.text =( httpDownloader.Progress * 100.0f )+ "";
            startTime = Time.time;
        }
    }

    private void OnApplicationQuit()
    {
        WebFileDownloader.StopDownload();
    }
}
