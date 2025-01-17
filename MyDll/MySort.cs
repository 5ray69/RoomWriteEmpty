using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomWriteEmpty.Infrastructure.MyDll
{
    public class MySort
    {
        public List<string> LevelNames(List<string> levelNames)
        {
            // Первый участок: строки с содержанием LU
            var firstSection = levelNames.Where(s => s.Contains("LU")).ToList();

            // Второй участок: строки с содержанием L, но не содержащие U, T, R
            var secondSection = levelNames.Where(s => s.Contains('L') && !s.Contains('U') && !s.Contains('T') && !s.Contains('R')).ToList();

            // Третий участок: строки с содержанием LT
            var thirdSection = levelNames.Where(s => s.Contains("LT")).ToList();

            // Четвертый участок: строки с содержанием LR
            var fourthSection = levelNames.Where(s => s.Contains("LR")).ToList();

            var sortedList = new List<string>();
            sortedList.AddRange(firstSection);
            sortedList.Add("");
            sortedList.AddRange(secondSection);
            sortedList.Add("");
            sortedList.AddRange(thirdSection);
            sortedList.Add("");
            sortedList.AddRange(fourthSection);

            return sortedList;
        }
    }
}
