using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchSpammer
{
    class DownloadInfo
    {
        public int TotalDownloads { get; set; }
        public int CurrentDownload { get; set; }

        public DownloadInfo(int TotalDownloads, int CurrentDownload)
        {
            this.TotalDownloads = TotalDownloads;
            this.CurrentDownload = CurrentDownload;
        }
    }
}
