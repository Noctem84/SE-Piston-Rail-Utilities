using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        public interface IMovable
        {
            RunState getRunState();
            void setRunState(RunState runState);
            Dictionary<RunState, StateTracker> getStates();
        }
    }
}
