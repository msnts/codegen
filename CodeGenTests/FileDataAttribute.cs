using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xunit.Sdk;

namespace CodeGenTests
{
    public class FileDataAttribute : DataAttribute
    {
        private readonly string[] _filesPath;

        public FileDataAttribute(params string[] filesPath)
        {
            _filesPath = filesPath;
        }

        private string GetFileData(string filePath)
        {
            var path = Path.IsPathRooted(filePath) ? filePath : Path.GetRelativePath(Directory.GetCurrentDirectory(), filePath);

            if (!File.Exists(path))
            {
                throw new ArgumentException($"Could not find file at path: {path}");
            }

            return File.ReadAllText(filePath);
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            if (testMethod == null) 
            { 
                throw new ArgumentNullException(nameof(testMethod));
            }

            var filesData = new List<object>(_filesPath.Length);

            foreach (var filePath in _filesPath)
            {
                filesData.Add(GetFileData(filePath));
            }

            return new List<object[]>() {  filesData.ToArray() };
        }
    }
}
