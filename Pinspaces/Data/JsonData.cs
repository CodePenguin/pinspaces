using Pinspaces.Core.Data;
using System.Collections.Generic;

namespace Pinspaces.Data
{
    public class JsonData
    {
        public JsonData()
        {
            Pinspaces = new List<Pinspace>();
            Windows = new List<PinWindow>();
        }

        public IList<Pinspace> Pinspaces { get; set; }
        public IList<PinWindow> Windows { get; set; }
    }
}
