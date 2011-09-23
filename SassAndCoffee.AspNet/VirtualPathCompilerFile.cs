﻿using System;
using System.IO;
using System.Web.Hosting;

using SassAndCoffee.Core;

namespace SassAndCoffee.AspNet {
    internal class VirtualPathCompilerFile: ICompilerFile {
        private static readonly DateTime unknownFileTime = DateTime.UtcNow;

        private readonly string virtualPath;

        public VirtualPathCompilerFile(string virtualPath) {
            if (string.IsNullOrEmpty(virtualPath)) {
                throw new ArgumentNullException("virtualPath");
            }
            this.virtualPath = virtualPath;
        }

        public DateTime LastWriteTimeUtc {
            get {
                if (!Exists) {
                    return default(DateTime);
                }
                using (Stream stream = Open()) {
                    FileStream file = stream as FileStream;
                    if (file != null) {
                        return File.GetLastWriteTimeUtc(file.Name);
                    }
                }
                // if the stream is not a file stream, we cannot determine the last write time and take the app startup time instead to avoid caching issues
                return unknownFileTime;
            }
        }

        public Stream Open() 
        {
            return HostingEnvironment.VirtualPathProvider.GetFile(virtualPath).Open();
        }

	    public string Name {
	        get {
	            return virtualPath;
	        }
	    }

	    public bool Exists {
	        get {
	            return HostingEnvironment.VirtualPathProvider.FileExists(virtualPath);
	        }
	    }

        public ICompilerFile GetRelativeFile(string relativePath)
        {
	        return new VirtualPathCompilerFile(HostingEnvironment.VirtualPathProvider.CombineVirtualPaths(virtualPath, relativePath.Replace('\\', '/')));
	    }
	}
}
