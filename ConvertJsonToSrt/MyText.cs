using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertJsonToSrt
{
    internal sealed class MyContent
    {
        public string Id { get; set; }
        public int RenderIndex { get; set; }
        public string Text { get; set; }
        //public string TimeRange { get; set; }
    }
}
