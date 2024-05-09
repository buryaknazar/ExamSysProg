using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExamSysProg.Models
{
    public class ProgramResults
    {
        public string fileName { get; set; }
        public string filePath { get; set; }
        public int countOfMatches { get; set; }

        public override string ToString()
        {
            return $"File Name: {fileName}\nFile Path: {filePath}\nCount Of Matches: {countOfMatches}";
        }
    }
}
