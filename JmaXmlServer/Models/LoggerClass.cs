using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace JmaXmlServer.Models
{

    public class LoggerClass
    {
        public static void LogInfo(string message)
        {
            Log.Warning(message);
        }

        public static void LogError(string message)
        {
            Log.Error(message);
        }
    }

    class ThreadIdEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                    "topic", "0"));
        }
    }
}