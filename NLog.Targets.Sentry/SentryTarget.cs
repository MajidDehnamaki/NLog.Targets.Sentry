using System;
using System.Collections.Generic;
using System.Linq;
using NLog.Common;
using NLog.Config;
using NLog.Targets.Sentry;
using SharpRaven;
using SharpRaven.Data;

// ReSharper disable CheckNamespace
namespace NLog.Targets
// ReSharper restore CheckNamespace
{
    [Target("Sentry")]
    public class SentryTarget : TargetWithLayout
    {
        private Dsn dsn;
        private readonly Lazy<IRavenClient> client;

        /// <summary>
        /// Map of NLog log levels to Raven/Sentry log levels
        /// </summary>
        protected static readonly IDictionary<LogLevel, ErrorLevel> LoggingLevelMap = new Dictionary<LogLevel, ErrorLevel>
        {
            {LogLevel.Debug, ErrorLevel.Debug},
            {LogLevel.Error, ErrorLevel.Error},
            {LogLevel.Fatal, ErrorLevel.Fatal},
            {LogLevel.Info, ErrorLevel.Info},
            {LogLevel.Trace, ErrorLevel.Debug},
            {LogLevel.Warn, ErrorLevel.Warning},
        };

        /// <summary>
        /// The DSN for the Sentry host
        /// </summary>
        [RequiredParameter]
        public string Dsn
        {
            get { return dsn == null ? null : dsn.ToString(); }
            set { dsn = new Dsn(value); }
        }

        /// <summary>
        /// Determines whether events with no exceptions will be send to Sentry or not
        /// </summary>
        public bool IgnoreEventsWithNoException { get; set; }

        /// <summary>
        /// Determines whether event properites will be sent to sentry as Tags or not
        /// </summary>
        public bool SendLogEventInfoPropertiesAsTags { get; set; }

        /// <summary>
        /// Gets the fields collection.
        /// </summary>
        /// <value>
        /// The fields.
        /// </value>
        [ArrayParameter(typeof(SentryField), "field")]
        public IList<SentryField> Fields { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public SentryTarget()
        {
            client = new Lazy<IRavenClient>(() => new RavenClient(dsn));
        }

        /// <summary>
        /// Internal constructor, used for unit-testing
        /// </summary>
        /// <param name="ravenClient">A <see cref="IRavenClient"/></param>
        internal SentryTarget(IRavenClient ravenClient) : this()
        {
            client = new Lazy<IRavenClient>(() => ravenClient);
        }

        /// <summary>
        /// Writes logging event to the log target.
        /// </summary>
        /// <param name="logEvent">Logging event to be written out.</param>
        protected override void Write(LogEventInfo logEvent)
        {
            try
            {
                foreach (var f in Fields)
                    f.Res = new SentryMessage(f.Layout.Render(logEvent));

                var field = Fields.ToDictionary(x => x.Name.ToString(), x => x.Res.ToString());

                client.Value.Logger = logEvent.LoggerName;

                // If the log event did not contain an exception and we're not ignoring
                // those kinds of events then we'll send a "Message" to Sentry
                if (logEvent.Exception == null && !IgnoreEventsWithNoException)
                {
                    var sentryMessage = new SentryMessage(Layout.Render(logEvent));
                    SentryEvent ev = new SentryEvent(sentryMessage);
                    ev.Tags = field;
                    ev.Level = LoggingLevelMap[logEvent.Level];
                    ev.Message = sentryMessage;
                    client.Value.CaptureAsync(ev);
                }
                else if (logEvent.Exception != null)
                {
                    var sentryMessage = new SentryMessage(logEvent.FormattedMessage);
                    SentryEvent ev = new SentryEvent(logEvent.Exception);
                    ev.Tags = field;
                    ev.Level = LoggingLevelMap[logEvent.Level];
                    ev.Message = sentryMessage;
                    client.Value.CaptureAsync(ev);
                }
            }
            catch (Exception e)
            {
                InternalLogger.Error("Unable to send Sentry request: {0}", e.Message);
            }
        }
    }
}

