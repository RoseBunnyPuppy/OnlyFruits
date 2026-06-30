using OnlyFruitsMod.ModParts.Models;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace OnlyFruitsMod.Features.Dialogue
{
    public class DialogueMonitor
    {
        public bool HasActiveDialogue { get; private set; }
        private class PendingAction
        {
            public Action? Action { get; set; }
            public Guid ActionId { get; set; }
            public Action? Cancel { get; set; }
            public bool IsCancelled { get; set; }

            public PendingAction(
                Action action,
                Guid actionId,
                Action cancel
            )
            {
                this.Action = action;
                this.ActionId = actionId;
                Cancel = cancel;
                this.IsCancelled = false;
            }

            public void Cleanup()
            {
                this.Action = null;
                this.Cancel = null;
            }
        }

        private readonly Dictionary<Guid, PendingAction> actionLookups = new();
        private readonly Queue<PendingAction> pendingDialogueActions = new();

        public ModPartContext Context { get; }

        public DialogueMonitor(
            ModPartContext context
        )
        {
            this.Context = context;
            this.Context.Helper.Events.Display.MenuChanged += Display_MenuChanged;
        }

        private void Display_MenuChanged(object? sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu != null)
            {
                this.HasActiveDialogue = true;
                return;
            }
            else
            {
                this.HasActiveDialogue = false;
            }
            if (this.DoesGameHavePendingMenus())
            {
                return;
            }
            this.TryStartNext();
        }

        private void TryStartNext()
        {
            while (true)
            {
                if (this.DoesGameHavePendingMenus()) return;

                // abort if nothing is queued
                if (!this.pendingDialogueActions.TryDequeue(out var nextPending)) return;
                // skip if cancelled
                else if (nextPending.IsCancelled)
                {
                    this.Cleanup(nextPending.ActionId);
                    continue;
                }

                this.Context.Monitor.Log($"Starting!", LogLevel.Debug);
                nextPending.Action?.Invoke();
                this.Cleanup(nextPending.ActionId);
            }
        }

        private void Cleanup(Guid id)
        {
            if (!this.actionLookups.TryGetValue(id, out var existing)) return;
            this.actionLookups.Remove(id);
            existing.Cleanup();
        }

        private bool DoesGameHavePendingMenus()
        {
            if (Game1.activeClickableMenu != null) return true;
            else if (Game1.nextClickableMenu.Any()) return true;
            return false;
        }
        /// <summary>
        ///   Invoke the action once all dialogue menus are closed.
        /// </summary>
        public void Enqueue(Action action)
        {
            var id = Guid.NewGuid();
            void Wrapper()
            {
                try
                {
                    action();
                }
                finally
                {
                    this.Cleanup(id);
                }
            }
            void Cancel()
            {
                this.Cleanup(id);
            }

            // create and enqueue the action
            this.pendingDialogueActions.Enqueue(this.actionLookups[id] = new PendingAction(Wrapper, id, Cancel));
            // dont progress the queue if there are any menus currently being shown
            if (this.DoesGameHavePendingMenus()) return;
            // dont progress the queue if we are aware of any active dialogues
            if (this.HasActiveDialogue) return;
            this.TryStartNext();
        }
    }
}
