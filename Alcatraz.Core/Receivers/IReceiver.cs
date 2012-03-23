using System;
using Alcatraz.Core.Log;

namespace Alcatraz.Core.Receivers
{
    public interface IReceiver
    {
        string SampleClientConfig { get; }
        string DisplayName { get; }
        Action<LogMessage> OnLogMessageReceived { get; set; }

        void Initialize();
        void Terminate();
    }
}