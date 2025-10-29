using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinAGI.Engine {
    internal class WinAGIFileWatcher {
        // TODO: cleanup filesystem watcher to catch changes in logic source files,
        // object/words.tok, others?
        FileSystemWatcher watcher;
        int blah;
        public WinAGIFileWatcher(string gamedir) {
            watcher = new FileSystemWatcher(gamedir);
            watcher.NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size;

            watcher.Changed += OnChanged;
            watcher.Created += OnCreated;
            watcher.Deleted += OnDeleted;
            watcher.Renamed += OnRenamed;
            watcher.Error += OnError;

            watcher.Filter = "*.*";
            // TODO: need separate watchers for each file, filetype...
            watcher.IncludeSubdirectories = true;
            // watcher.EnableRaisingEvents = true;
        }

        public void Dispose() {
            watcher.Changed -= OnChanged;
            watcher.Created -= OnCreated;
            watcher.Deleted -= OnDeleted;
            watcher.Renamed -= OnRenamed;
            watcher.Error -= OnError;
            watcher.Dispose();
        }

        internal bool Enabled {
            get => watcher.EnableRaisingEvents;
            set => watcher.EnableRaisingEvents = value;
        } 
        private static void OnChanged(object sender, FileSystemEventArgs e) {
            if (e.ChangeType != WatcherChangeTypes.Changed) {
                return;
            }
            Debug.Print($"Changed: {e.FullPath}");
        }

        private static void OnCreated(object sender, FileSystemEventArgs e) {
            string value = $"Created: {e.FullPath}";
            Debug.Print(value);
        }

        private static void OnDeleted(object sender, FileSystemEventArgs e) =>
            Debug.Print($"Deleted: {e.FullPath}");

        private static void OnRenamed(object sender, RenamedEventArgs e) {
            Debug.Print($"Renamed:");
            Debug.Print($"    Old: {e.OldFullPath}");
            Debug.Print($"    New: {e.FullPath}");
        }

        private static void OnError(object sender, ErrorEventArgs e) =>
            PrintException(e.GetException());

        private static void PrintException(Exception ex) {
            if (ex is not null) {
                Debug.Print($"Message: {ex.Message}");
                Debug.Print("Stacktrace:");
                Debug.Print(ex.StackTrace);
                Debug.Print("");
                PrintException(ex.InnerException);
            }
        }
    }
}
