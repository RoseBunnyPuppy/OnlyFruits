using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyFruitsMod.Features.ReloadHelpers
{
   
    public class ReloadManager
    {
        private enum ReloadActions
        {
            Default = 0,
            ReApply = 1,
        }
        private ReloadActions ReloadAction { get; set; } = ReloadActions.Default;

        /// <summary>
        ///   Returns true if we need to reload the data.
        /// </summary>
        public bool ConsumeReload()
        {
            if (this.ReloadAction == ReloadActions.Default) return false;
            this.ReloadAction = ReloadActions.Default;
            return true;
        }

        /// <summary>
        ///   Mark the data as needing to be reloaded.
        /// </summary>
        public void EnqueueReload()
        {
            this.ReloadAction = ReloadActions.ReApply;
        }
    }
   
}
