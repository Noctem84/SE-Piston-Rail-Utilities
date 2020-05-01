using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        public class StateTracker
        {
            public int Index { get; set; }
            public IEnumerator<bool> State { get; set; }

            public StateTracker(int index)
            {
                this.Index = index;
            }
        }
    }
}
