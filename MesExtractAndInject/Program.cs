﻿using System;
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
        static void Main(string[] args)
        {
            // TODO: Use console parser nuget to make this less shitty.
            if (!ArgumentParser(args))
            {
                return;
            }

            var japaneseEncoding = Encoding.GetEncoding(932);
            var file = File.ReadAllBytes(args[0]);
            //var file = File.ReadAllBytes("OPEN_1.MES");
            var dialogs = TextTools.ParseDialogList(file);
            dialogs.RemoveAll(node => node == null);
            var newFile = file;
            var offset = 0;
            for (var i = 0; i < dialogs.Count; i++)
            {
                if (dialogs[i] == null)
                {
                    continue;
                }
                var newDialogs = TextTools.ParseDialogList(newFile);
                newDialogs.RemoveAll(node => node == null);
                Console.WriteLine("Character Name: " + Enum.GetName(typeof(Characters), dialogs[i].Character));
                Console.WriteLine("Dialog: " + dialogs[i].Dialog + Environment.NewLine);
                Console.WriteLine("Write New Dialog (leave empty to skip): ");
                var newDialog = Console.ReadLine();
                Console.WriteLine(Environment.NewLine);
                if (string.IsNullOrEmpty(newDialog)) continue;
                var encodedText = japaneseEncoding.GetBytes(TextTools.FullWidthConvertor(newDialog));
                encodedText = TextTools.Combine(encodedText, new[] { Convert.ToByte('\xBA') });
                newFile = TextTools.ReplaceText(newFile, encodedText, newDialogs[i].StartIndex, newDialogs[i].EndIndex);
            }

            var newFileName = Path.GetFileNameWithoutExtension(args[0]) + "_EDIT.MES";
            //var newFileName = "OPEN_1_EDIT.MES";
            File.WriteAllBytes(newFileName, newFile);
            Console.WriteLine($"Done! Add edit {newFileName} to its original name and replace it on the FDI disk.");
            Console.ReadKey();
        }

        static bool ArgumentParser(string[] args)
        {
            if (!args.Any())
            {
                Console.WriteLine("You must enter the path to the MES file!");
                return false;
            }

            var path = Path.GetExtension(args[0]);
            if (path == ".MES") return true;
            Console.WriteLine("You must enter an MES file!");
            return false;
        }
    }
}
