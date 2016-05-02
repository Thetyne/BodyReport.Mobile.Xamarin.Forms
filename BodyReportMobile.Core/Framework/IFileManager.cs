﻿using System;
using System.IO;
using System.Text;

namespace BodyReportMobile.Core.Framework
{
	public interface IFileManager
	{
		String GetResourcesPath();
        String GetDocumentPath();
        bool ResourceFileExist (string filePath);
        Stream OpenResourceFile(string filePath);
        bool FileExist(string filePath);
        Stream OpenFile (string filePath);
		void CloseFile (StreamReader stream);
		string[] ReadAllLinesFile(string filePath, Encoding encoding);
		string ReadAllTextFile(string filePath, Encoding encoding);
        void WriteAllTextFile(string filePath, string contents, Encoding encoding);
        bool DeleteFile(string filePath);
        bool DirectoryExist(string path);
        bool CreateDirectory(string path);
    }
}

