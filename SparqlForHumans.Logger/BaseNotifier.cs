namespace SparqlForHumans.Logger
{
    public abstract class BaseNotifier
    {
        private readonly NLog.Logger _logger = Logger.Init();
        public int NotifyTicks { get; set; } = 100000;
        public abstract string NotifyMessage { get;  }

        public virtual void LogProgress(long ticks, bool overrideCheck = false)
        {
            if (ticks % NotifyTicks == 0 || overrideCheck)
            {
                _logger.Info($"{NotifyMessage}, Count: {ticks:N0}");
            }
        }

        public virtual void LogException(long ticks, string message)
        {
            _logger.Error($"{message}, Count: {ticks:N0}");
        }
    }
}