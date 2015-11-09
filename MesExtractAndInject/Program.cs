using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MseExtractAndInject.Core.Models;
using MseExtractAndInject.Core.Tools;

namespace MesExtractAndInject
{
    class Program
    {
        static void Main()
        {
              MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            var japaneseEncoding = Encoding.GetEncoding(932);
            var encodedTextBytes = japaneseEncoding.GetBytes("ｍｙ　ｔｅｘｔ　ｉｓ　ｎｏｗ　ｓｕｐｅｒ　ｌｏｎｇ");
            encodedTextBytes = TextTools.Combine(encodedTextBytes, new[] { Convert.ToByte('\xBA') });
            var file = File.ReadAllBytes("OPEN_1.MES");
            var dialogs = TextTools.ParseDialog(file);
            var newFile = file;
            var count = 0;
            var dialog = dialogs.First();
            try
            {
                newFile = TextTools.ReplaceText(newFile, encodedTextBytes, dialog.StartIndex, dialog.EndIndex);
            }
            catch (Exception)
            {
                Console.Write(count);
            }
            count++;
            File.WriteAllBytes("OPEN_1_TEST.MES", newFile);
        }
    }
}
