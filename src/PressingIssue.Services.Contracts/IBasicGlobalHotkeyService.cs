using System;

namespace PressingIssue.Services.Contracts
{
    public interface IBasicGlobalHotkeyService : IDisposable
    {
        bool ProcessingHotkeys { get; set; }
        bool Running { get; }

        void Start(bool processingHotkeys = true);
        void Stop();

        void RemoveAllHotkeys();
    }
}
