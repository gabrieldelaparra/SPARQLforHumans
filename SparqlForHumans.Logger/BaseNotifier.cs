namespace SparqlForHumans.Logger
{
    public abstract class BaseNotifier
    {
        private readonly NLog.Logger _logger = SparqlForHumans.Logger.Logger.Init();
        public int NotifyTicks { get; } = 100000;
        public abstract string NotifyMessage { get;  }

        public virtual void LogProgress(long Ticks, bool overrideCheck = false)
        {
            if (Ticks % NotifyTicks == 0 || overrideCheck)
            {
                _logger.Info($"{NotifyMessage}, Count: {Ticks:N0}");
            }
        }
    }
}