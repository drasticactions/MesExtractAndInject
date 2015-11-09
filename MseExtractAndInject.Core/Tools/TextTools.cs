using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using MseExtractAndInject.Core.Models;

namespace MseExtractAndInject.Core.Tools
{
    public static class TextTools
    {
        public static byte[] ReplaceText(byte[] file, byte[] replaceText, int startIndex, int endIndex)
        {
            var startByte = file.Slice(0, startIndex);
            var endByte = file.Slice(endIndex, file.Length - 1);
            return Combine(startByte, replaceText, endByte);
        }

        public static string FullWidthConvertor(string unicodeString)
        {
            var sb = new StringBuilder(256);
            LCMapString(LOCALE_SYSTEM_DEFAULT, LCMAP_FULLWIDTH, unicodeString, -1, sb, sb.Capacity);
            return sb.ToString();
        }

        public static string HalfWidthConvertor(string unicodeString)
        {
            var sb = new StringBuilder(256);
            LCMapString(LOCALE_SYSTEM_DEFAULT, LCMAP_HALFWIDTH, unicodeString, -1, sb, sb.Capacity);
            return sb.ToString();
        }

        public static string DecodeText(byte[] file)
        {
            // Ported from Kkuucr mes-decrypt.py
            // https://gist.github.com/kyuucr/990472bdd8ecd1184bf0
            var japaneseEncoding = Encoding.GetEncoding("shift-jis");
            var sym = false;
            var control = false;
            var skip = false;
            var byteList = new List<byte>();
            foreach (var testByte in file)
            {
                if (testByte == '\xBA' && !skip) // control byte
                {
                    control = true;
                }
                else if (control && testByte == '\x23')  // dialog start cole
                {
                    control = false;
                }
                else if (control && testByte == '\x24')  // dialog start doc
                {
                    control = false;
                }
                else if (control && testByte == '\x25')  // dialog start jack?
                {
                    control = false;
                }
                else if (control && testByte == '\x26')  // dialog end
                {
                    control = false;
                }
                else if (control && testByte == '\x28')  // symbol byte
                {
                    control = false;
                    sym = true;
                }
                else if (sym && testByte == '\x0D')  // comma
                {
                    byteList.AddRange(new[] { Convert.ToByte('\x81'), Convert.ToByte('\x41') });
                    sym = false;
                }
                else if (sym && testByte == '\x0E')  // ..
                {
                    byteList.AddRange(new[] { Convert.ToByte('\x81'), Convert.ToByte('\x64') });
                    sym = false;
                }
                else if (sym && testByte == '\x0F')  // period
                {
                    byteList.AddRange(new[] { Convert.ToByte('\x81'), Convert.ToByte('\x42') });
                    sym = false;
                }
                else if (sym && testByte == '\x10')  // space
                {
                    byteList.AddRange(new[] { Convert.ToByte('\x81'), Convert.ToByte('\x40') });
                    sym = false;
                }
                else if (sym && testByte == '\x11')  // !
                {
                    byteList.AddRange(new[] { Convert.ToByte('\x81'), Convert.ToByte('\x49') });
                    sym = false;
                }
                else if (sym && testByte == '\x12')  // ?
                {
                    byteList.AddRange(new[] { Convert.ToByte('\x81'), Convert.ToByte('\x48') });
                    sym = false;
                }
                else if (sym && testByte == '\x13')  // newline for two characted dialog in one box
                {
                    byteList.Add(Convert.ToByte('\n'));
                    sym = false;
                }
                else if (skip)
                {
                    byteList.Add(testByte);
                    skip = false;
                }
                else if (
                    ((testByte >= 129 && testByte <= 131) || (testByte >= 136 && testByte <= 159) || (testByte >= 224 && testByte <= 234))
                    )
                {
                    byteList.Add(testByte);
                    skip = true;
                }
                else if (sym || control)
                {
                    sym = false;
                    control = false;
                }
                else
                {
                    byteList.Add(Convert.ToByte('\x82'));
                    var newcharByte = (byte)(testByte + 114);
                    byteList.Add(newcharByte);
                }
            }
            var newByteArray = japaneseEncoding.GetChars(byteList.ToArray());
            return new string(newByteArray);
        }

        public static byte[] Combine(params byte[][] arrays)
        {
            var rv = new byte[arrays.Sum(a => a.Length)];
            var offset = 0;
            foreach (var array in arrays)
            {
                Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }

        public static List<DialogBlob> ParseDialog(byte[] file)
        {
            var dialogList = new List<DialogBlob>();
            // Get start of known dialog location
            var dialogIndexes = file.FindAllIndexOf(Convert.ToByte(186));
            foreach (var dialogIndex in dialogIndexes)
            {
                // Find the character. If none, continue.
                var dialog = new DialogBlob();
                switch ((Characters)file[dialogIndex + 1])
                {
                    case Characters.Cole:
                        dialog.Character = Characters.Cole;
                        break;
                    case Characters.Doc:
                        dialog.Character = Characters.Doc;
                        break;
                    case Characters.Jack:
                        dialog.Character = Characters.Jack;
                        break;
                    default:
                        continue;
                }
                var endIndex = Array.IndexOf(file, Convert.ToByte('\x26'), dialogIndex);
                var dialogBytes = file.Slice(dialogIndex, endIndex);
                dialog.DialogBytes = dialogBytes;
                dialog.StartIndex = dialogIndex + 2; // The start of the actual dialog.
                dialog.EndIndex = endIndex;
                dialog.Dialog = TextTools.DecodeText(dialogBytes);
                dialogList.Add(dialog);
            }
            return dialogList;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern int LCMapString(uint Locale, uint dwMapFlags, string lpSrcStr, int cchSrc, StringBuilder lpDestStr, int cchDest);

        private const uint LCMAP_FULLWIDTH = 0x00800000;
        private const uint LOCALE_SYSTEM_DEFAULT = 0x0800;
        private const uint LCMAP_HALFWIDTH = 0x00400000;
    }
}
