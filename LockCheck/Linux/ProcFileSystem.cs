﻿using System;
using System.Collections.Generic;
using System.IO;

namespace LockCheck.Linux
{
    internal static class ProcFileSystem
    {
        public static IEnumerable<ProcessInfo> GetLockingProcessInfos(params string[] paths)
        {
            if (paths == null)
                throw new ArgumentNullException(nameof(paths));

            Dictionary<long, string> inodesToPaths = null;

            using (var reader = new StreamReader("/proc/locks"))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (inodesToPaths == null)
                    {
                        inodesToPaths = GetInodeToPaths(paths);
                    }

                    var lockInfo = LockInfo.ParseLine(line);
                    if (inodesToPaths.ContainsKey(lockInfo.InodeInfo.INodeNumber))
                    {
                        var processInfo = ProcessInfoLinux.Create(lockInfo);
                        if (processInfo != null)
                        {
                            yield return processInfo;
                        }
                    }
                }
            }
        }

        private static Dictionary<long, string> GetInodeToPaths(string[] paths)
        {
            var inodesToPaths = new Dictionary<long, string>();
            foreach (string path in paths)
            {
                long inode = NativeMethods.GetInode(path);
                if (inode != -1)
                {
                    inodesToPaths.Add(inode, path);
                }
            }

            return inodesToPaths;
        }
    }
}
