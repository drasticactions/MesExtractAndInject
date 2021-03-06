﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
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
#if DEBUG
            var file = File.ReadAllBytes("OPEN_1.MES");
            var pathName = "OPEN_1.MES";
#else
            if (!ArgumentParser(args))
            {
                return;
            }
            var file = File.ReadAllBytes(args[0]);
            var pathName = args[0];
#endif
            //TranslatorFile(args);
            //return;

            var japaneseEncoding = Encoding.GetEncoding(932);
            var bytesForCole = japaneseEncoding.GetBytes("真剣");
            var omgnew = new Byte[] {186, 37};
            foreach (var position in file.Locate(omgnew))
            {
                //var encodedName = japaneseEncoding.GetBytes(TextTools.FullWidthConvertor("Cougar"));
                //file = TextTools.ReplaceText(file, encodedName, position, position + bytesForCole.Length);
                Console.WriteLine(position);
            }

            var dialogs = TextTools.ParseDialogList(file);
            dialogs.RemoveAll(node => node == null);
            var test = dialogs.Where(node => node.Character == Characters.JackOrSheila);
            
            var newFile = file;
            var additionalDialogs = 0;
            for (var i = 0; i < dialogs.Count; i++)
            {
                if (dialogs[i] == null)
                {
                    continue;
                }
                var newDialogs = TextTools.ParseDialogList(newFile);
                newDialogs.RemoveAll(node => node == null);

                // Don't allow changing the initial line.
                if (i != 0)
                {
                    var insertDialog = true;
                    while (insertDialog)
                    {
                        Console.WriteLine("Insert New Line?");
                        var response = Console.ReadKey();
                        Console.Write(Environment.NewLine);
                        if (response.Key == ConsoleKey.Y)
                        {
                            var dialog = new DialogBlob();
                            Console.WriteLine("Character? 1. Cole 2. Doc 3. Other");
                            response = Console.ReadKey();
                            Console.Write(Environment.NewLine);
                            switch (response.Key)
                            {
                                case ConsoleKey.NumPad1:
                                    dialog.Character = Characters.Cole;
                                    break;
                                case ConsoleKey.NumPad2:
                                    dialog.Character = Characters.Doc;
                                    break;
                                case ConsoleKey.NumPad3:
                                    dialog.Character = Characters.JackOrSheila;
                                    break;
                                default:
                                    dialog.Character = Characters.Cole;
                                    break;
                            }
                            Console.Write("New Dialog: ");
                            var brandNewDialog = Console.ReadLine();
                            var newEncodedText = japaneseEncoding.GetBytes(TextTools.HalfWidthConvertor(brandNewDialog));
                            newEncodedText = TextTools.Combine(new[] { Convert.ToByte('\x26'), Convert.ToByte('\xBA'), Convert.ToByte(dialog.Character), Convert.ToByte('\x21') }, newEncodedText, new[] { Convert.ToByte('\x00'), Convert.ToByte('\xBA'), Convert.ToByte('\x26')});
                            var lastDialog = newDialogs[i + additionalDialogs - 1];
                            dialog.StartIndex = lastDialog.EndIndex;
                            dialog.EndIndex = dialog.StartIndex + newEncodedText.Length;
                            newFile = TextTools.AddText(newFile, newEncodedText, dialog.StartIndex);
                            newDialogs = TextTools.ParseDialogList(newFile);
                            newDialogs.RemoveAll(node => node == null);
                            additionalDialogs++;
                        }
                        else
                        {
                            insertDialog = false;
                        }
                    }
                }

                Console.WriteLine("Character Name: " + Enum.GetName(typeof(Characters), dialogs[i].Character));
                Console.WriteLine("Dialog: " + dialogs[i].Dialog + Environment.NewLine);
                Console.Write("New Dialog: ");
                var newDialog = Console.ReadLine();
                Console.WriteLine(Environment.NewLine);

                if (string.IsNullOrEmpty(newDialog)) continue;

                var encodedText = japaneseEncoding.GetBytes(TextTools.HalfWidthConvertor(newDialog));
                encodedText = TextTools.Combine(new[] { Convert.ToByte('\x21')} , encodedText, new[] { Convert.ToByte('\x00'), Convert.ToByte('\xBA') });
                //encodedText = TextTools.Combine( encodedText, new[] { Convert.ToByte('\xBA'), Convert.ToByte('\x26'), Convert.ToByte('\xBA'), Convert.ToByte('\x25')}, encodedText, new [] { Convert.ToByte('\xBA') });
                newFile = TextTools.ReplaceText(newFile, encodedText, newDialogs[i + additionalDialogs].StartIndex, newDialogs[i + additionalDialogs].EndIndex);
            }

            var newFileName = Path.GetFileNameWithoutExtension(pathName) + "_EDIT.MES";
            //var newFileName = "OPEN_1_EDIT.MES";
            File.WriteAllBytes(newFileName, newFile);
            Console.WriteLine($"Done! Add edit {newFileName} to its original name and replace it on the FDI disk.");
            Console.ReadKey();
        }

        static void TranslatorFile(string[] args)
        {
            var japaneseEncoding = Encoding.GetEncoding(932);
            //var file = File.ReadAllBytes(args[0]);
            var path = "OPEN_2";
            var file = File.ReadAllBytes("MES/" + path + ".MES");
            var dialogs = TextTools.ParseDialogList(file);
            dialogs.RemoveAll(node => node == null);
            var newFile = file;
            var offset = 0;
            var newString = "";
            for (var i = 0; i < dialogs.Count; i++)
            {
                if (dialogs[i] == null)
                {
                    continue;
                }
                var newDialogs = TextTools.ParseDialogList(newFile);
                newDialogs.RemoveAll(node => node == null);
                newString += "Character Name: " + Enum.GetName(typeof(Characters), dialogs[i].Character);
                newString += Environment.NewLine;
                newString += "Dialog: " + dialogs[i].Dialog;
                newString += Environment.NewLine;
                newString += "New Dialog: ";
                newString += Environment.NewLine;
                newString += Environment.NewLine;
            }

            //var newFileName = Path.GetFileNameWithoutExtension(args[0]) + "_EDIT.TXT";
            var newFileName = path + "-EDIT.TXT";
            File.WriteAllText(newFileName, newString);
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
