// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Microsoft.Azure.Functions.Worker
{
    using System;

    using global::Azure.Monitor.OpenTelemetry.AspNetCore;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    using OpenTelemetry.Logs;
    using OpenTelemetry.Metrics;
    using OpenTelemetry.Trace;

    public static class FunctionsOpenTelemetryExtensions
    {
        /// <summary>
        /// Configures OpenTelemetry for Functions.
        /// </summary>
        /// <param name="services">The collection of services.</param>
        /// <param name="context">The host builder context.</param>
        /// <returns>The collection of services.</returns>
        public static IServiceCollection ConfigureFunctionsOpenTelemetry(this IServiceCollection services, HostBuilderContext context)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (bool.TryParse(Environment.GetEnvironmentVariable("OTEL_SDK_DISABLED") ?? bool.TrueString, out var b) && !b)
            {
                // This block should all go in an extension method w/in Functions worker lib
                var otBuilder = services.AddOpenTelemetry().UseAzureMonitor();

                // This line of code should be put
                IConfigurationSection newRelicExporterConfigSection = context.Configuration.GetSection(ConfigurationPath.Combine("openTelemetry", "exporters", "newrelic"));
                if (newRelicExporterConfigSection.Exists())
                {
                    otBuilder
                        .WithTracing(o => o.AddOtlpExporter(newRelicExporterConfigSection.Bind))
                        .WithMetrics(o => o.AddOtlpExporter(newRelicExporterConfigSection.Bind));

                    services.AddLogging(b => b.AddOpenTelemetry(o => o.AddOtlpExporter(newRelicExporterConfigSection.Bind)));
                }

                // Lets the host know that the worker is sending logs to App Insights. The host will now ignore these.
                services.Configure<WorkerOptions>(workerOptions => workerOptions.Capabilities["WorkerApplicationInsightsLoggingEnabled"] = bool.TrueString);
            }

            return services;
        }
    }
}
