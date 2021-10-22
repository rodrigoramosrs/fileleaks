using FileLeaks.Core.Events;
using FileLeaks.Core.Services;
using System;

namespace FileLeaks.Core
{
    public class FileLeakCore : ScanEvents
    {
        private readonly SecretSearchService _SecretSearchService;

        public FileLeakCore(string RegexDirectory, int MaxFileSizeMB = 50)
        {
            _SecretSearchService = new SecretSearchService(RegexDirectory, MaxFileSizeMB);
            _SecretSearchService.OnProgressChange += _SecretSearchService_OnProgressChange; ;
            _SecretSearchService.OnTotalFilesCount += _SecretSearchService_OnTotalFilesCount;
            _SecretSearchService.OnSecretFound += _SecretSearchService_OnSecretFound;
            _SecretSearchService.OnStarted += _SecretSearchService_OnStarted;
            _SecretSearchService.OnFinished += _SecretSearchService_OnFinished;
        }

        private void _SecretSearchService_OnFinished(object sender, System.Collections.Generic.IEnumerable<Models.SecretResult> e)
        {
            this.NotifyFinish(e);
        }

        private void _SecretSearchService_OnStarted(object sender, EventArgs e)
        {
            this.NotifyStart();
        }

        private void _SecretSearchService_OnProgressChange(object sender, Models.Events.FileScanProgress e)
        {
            this.NotifyProgressChange(e);
        }

        private void _SecretSearchService_OnSecretFound(object sender, Models.MatchResult e)
        {
            this.NotifyOnSecretFound(e);
        }

        private void _SecretSearchService_OnTotalFilesCount(object sender, int e)
        {
            this.NotifyTotalFilesToSearch(e);
        }



        public void DoSearch(string Folder)
        {
            _SecretSearchService.SearchSecretInFolder(Folder);
        }

    }
}
