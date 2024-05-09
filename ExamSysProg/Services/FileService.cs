using ExamSysProg.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExamSysProg.Services
{
    static public class FileService
    {
        static public async Task<List<string>> ReadFileBanWordsAsync(string path)
        {
            List<string> words = new List<string>();

            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader streamReader = new StreamReader(fileStream, Encoding.UTF8))
                {
                    while (streamReader.Peek() > 0)
                    {
                        words.Add(await streamReader.ReadLineAsync());
                    }
                }
            }

            return words;
        }

        static public async Task<ProgramResults> FileContainsWord(string filePath, List<string> searchWords)
        {
            ProgramResults result = new ProgramResults();
            result.filePath = filePath;
            result.fileName = System.IO.Path.GetFileName(filePath);
            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = await reader.ReadLineAsync();

                        foreach (string searchWord in searchWords)
                        {
                            if (line.Contains(searchWord))
                            {
                                result.countOfMatches++;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                
            }

            return result;
        }

        static public async Task CopyAndReplaceWordsAsync(string sourcePath, string destinationPath, List<string> searchWords, string replacement)
        {
            try
            {
                using (StreamReader reader = new StreamReader(sourcePath))
                using (StreamWriter writer = new StreamWriter(destinationPath))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = await reader.ReadLineAsync();

                        foreach (string searchWord in searchWords)
                        {
                            line = line.Replace(searchWord, replacement);
                        }

                        await writer.WriteLineAsync(line);
                    }
                }
            }
            catch (Exception ex)
            {
                
            }
        }
    }
}
