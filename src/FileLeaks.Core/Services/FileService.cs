using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FileLeaks.Core.Services
{
    public class FileService
    {
        public IEnumerable<string> DirectorySearch(string dir)
        {
            string[] FileList = new string[0];
            string[] DirectoryList = new string[0];
            try
            {
                FileList = Directory.GetFiles(dir, "*.*", SearchOption.TopDirectoryOnly);
            }
            catch (Exception)
            {

                //TODO: Implement log
            }

            try
            {
                DirectoryList = Directory.GetDirectories(dir);
            }
            catch (Exception)
            {

                //TODO: Implement log;
            }


            foreach (var f in (FileList))
            {
                bool canContinue = false;
                try
                {
                    canContinue = !new DirectoryInfo(f).Attributes.HasFlag(FileAttributes.ReparsePoint);
                }
                catch (Exception)
                {
                    //TODO: Implement log;
                }

                if (canContinue)
                    yield return f;
            }

            foreach (string d in DirectoryList)
            {
                bool canContinue = false;

                try
                {
                    canContinue = !new DirectoryInfo(d).Attributes.HasFlag(FileAttributes.ReparsePoint);
                }
                catch (Exception)
                {

                    //TODO: Implement log;
                }
                if (canContinue)
                    foreach (string a in DirectorySearch(d))
                        yield return a;
            }

        }
    }
}
