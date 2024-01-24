using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albelli.MiscUtils.Lib
{
    public class ProgressTracker
    {
        #region field(s)
        private int totalCount;
        private TextWriter log;
        private DateTime dtStart;
        private Object currItemIdxLock = (object)-1;
        private Int32 currItemIdx = -1;
        private const double progressLogIntervalPct = 1.00;
        private int lastTrackedProgress = 0;
        private double progressAchievedPct = 0;
        #endregion

        public void Start(int totalCount, TextWriter log, DateTime dtStart)
        {
            this.totalCount = totalCount;
            this.log = log;
            this.dtStart = dtStart;

            this.currItemIdx = -1;
            this.lastTrackedProgress = 0;
            this.progressAchievedPct = 0;
        }

        public int TotalItemsCount
        {
            get
            {
                return this.totalCount;
            }
        }

        public double CurrentProgress
        {
            get { return progressAchievedPct; }
        }

        public void ItemComplete()
        {
            lock (currItemIdxLock)
                currItemIdx++;

            #region Tracking the progresss
            progressAchievedPct = (double)(((double)(currItemIdx + 1) / totalCount) * 100);
            if ((progressAchievedPct - lastTrackedProgress) >= progressLogIntervalPct)
            {
                lastTrackedProgress = (int)progressAchievedPct;
                DateTime now = DateTime.Now;
                TimeSpan tsElapsed = now - dtStart;
                int secondsElapsed = (int)tsElapsed.TotalSeconds;
                int estimatedTotalSeconds = (int)(secondsElapsed / (progressAchievedPct / 100));
                TimeSpan tsETA = new TimeSpan((long)(estimatedTotalSeconds * (long)Math.Pow(10, 7)));
                DateTime dtETA = dtStart + tsETA;
                TimeSpan tsLeft = dtETA - now;
                log.WriteLine($"{DateTime.Now:s}\t{progressAchievedPct}\t{tsElapsed} % complete\telapsed - {tsElapsed}, left - {tsLeft}, total - {tsETA}, ETA - {dtETA}");
            }
            #endregion
        }

    }
}
