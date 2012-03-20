using Alcatraz.Core.Log;

namespace Alcatraz.Core.Receivers
{
    public interface IReceiver
    {
        string SampleClientConfig { get; }
        string DisplayName { get; }

        void Initialize();
        void Terminate();

        void Attach(ILogMessageNotifiable notifiable);
        void Detach();
    }
}