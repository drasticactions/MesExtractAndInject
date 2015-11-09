using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MseExtractAndInject.Core.Tools;

namespace MseExtractAndInject.Core.Models
{
    public class DialogBlob
    {
        public Characters Character { get; set; }

        public byte[] DialogBytes { get; set; }

        public string Dialog { get; set; }

        public int StartIndex { get; set; }

        public int EndIndex { get; set; }
    }
}
