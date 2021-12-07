using System.Collections;
using System.Collections.Generic;

public class WebFile{

    public string Name { get; set; }
    public string Url { get; set; }
    public string LocalPath { get; set; }
    public long DownloadedSize { get; set; }
    public long TotalSize { get; set; }
    public override string ToString()
    {
        return string.Format("Url:{0}, Name:{1}, {2}/{3}", Url, Name, DownloadedSize, TotalSize);
    }
}
