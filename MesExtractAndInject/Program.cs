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
 
            var dialog = dialogs.First();
            try
            {
                newFile = TextTools.ReplaceText(newFile, encodedTextBytes, dialog.StartIndex, dialog.EndIndex);
            }
            catch (Exception)
            {
            }

            // Remake dialogs list to get new buffers.
            // Note to view: This is dumb. I should be able to get the offsets from the first insert and
            // adjust from them. But I'm lazy and yet, also dumb.
            dialogs = TextTools.ParseDialog(newFile);
            dialog = dialogs[1];
            try
            {
                newFile = TextTools.ReplaceText(newFile, encodedTextBytes, dialog.StartIndex, dialog.EndIndex);
            }
            catch (Exception)
            {
            }

            File.WriteAllBytes("OPEN_1_TEST.MES", newFile);
        }
    }
}
