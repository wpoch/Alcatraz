using System;
using System.ComponentModel;
using Alcatraz.Core.Log;

namespace Alcatraz.Core.Receivers
{
    [Serializable]
    public abstract class BaseReceiver : MarshalByRefObject, IReceiver
    {
        [NonSerialized]
        private string _displayName;

        public abstract Action<LogMessage> OnLogMessageReceived { get; set; }

        #region IReceiver Members

        public abstract string SampleClientConfig { get; }

        [Browsable(false)]
        public string DisplayName
        {
            get { return _displayName; }
            protected set { _displayName = value; }
        }

        public abstract void Initialize();
        public abstract void Terminate();

        #endregion
    }
}