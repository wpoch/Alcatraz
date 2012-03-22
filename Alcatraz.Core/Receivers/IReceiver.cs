using System;
using Alcatraz.Core.Log;

namespace Alcatraz.Core.Receivers
{
    public interface IReceiver
    {
        string SampleClientConfig { get; }
        string DisplayName { get; }

        void Initialize();
        void Terminate();

        Action<LogMessage> OnLogMessageReceived { get; set; }
    }
}