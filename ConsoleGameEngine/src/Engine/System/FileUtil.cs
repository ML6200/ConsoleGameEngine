using System.Collections.Generic;
using System.IO;

namespace ConsoleGameEngine.Engine.System;

public class FileUtil
{
    public struct FileMap
    {
        public string Name;
        public string FullPath;
        
        public FileMap(string name, string fullPath)
        {
            Name = name;
            FullPath = fullPath;
        }
    }
    
    public static List<FileMap> FileListAllEndsWith(string path, string extension)
    {
        List<FileMap> fileMap = new List<FileMap>();
        if (File.Exists(path))
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            FileInfo[] files = directoryInfo.GetFiles();
            foreach (FileInfo file in files)
            {
                if (file.Extension == extension)
                {
                    fileMap.Add(new FileMap(file.Name, 
                        file.FullName));
                }
            }
        }
        
        return fileMap;
    }
}