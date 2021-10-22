using FileLeaks.Core.Models;
using FileLeaks.Core.Models.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileLeaks.Core.Events
{
    public class ScanEvents
    {
        public event EventHandler<int> OnTotalFilesCount;
        public event EventHandler<FileScanProgress> OnProgressChange;
        public event EventHandler<MatchResult> OnSecretFound;
        public event EventHandler OnStarted;
        public event EventHandler<IEnumerable<SecretResult>> OnFinished;

        protected virtual void NotifyTotalFilesToSearch(int TotalFiles)
        {
            this.OnTotalFilesCount?.Invoke(this, TotalFiles);
        }

        protected virtual void NotifyProgressChange(FileScanProgress fileProgress)
        {
            this.OnProgressChange?.Invoke(this, fileProgress);
        }

        protected virtual void NotifyOnSecretFound(MatchResult matchResult)
        {
            this.OnSecretFound?.Invoke(this, matchResult);
        }

        protected virtual void NotifyFinish(IEnumerable<SecretResult> secretResult)
        {
            this.OnFinished?.Invoke(this, secretResult);
        }

        protected virtual void NotifyStart()
        {
            this.OnStarted?.Invoke(this, null);
        }
    }
}
