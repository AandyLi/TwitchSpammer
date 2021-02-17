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

        public DownloadInfo(int TotalDownloads = 0, int CurrentDownload = 0)
        {
            this.TotalDownloads = TotalDownloads;
            this.CurrentDownload = CurrentDownload;
        }
    }
}
