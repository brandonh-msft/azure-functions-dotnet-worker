// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Microsoft.Azure.Functions.Worker.OpenTelemetry
{
    using System;

    using global::Azure.Core;
    using global::Azure.Monitor.OpenTelemetry.Exporter;
    using global::Azure.Monitor.OpenTelemetry.LiveMetrics;
    using global::OpenTelemetry;
    using global::OpenTelemetry.Logs;
    using global::OpenTelemetry.Metrics;
    using global::OpenTelemetry.Trace;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public static class FunctionsOpenTelemetryExtensions
    {
        /// <summary>
        /// Configures OpenTelemetry for Functions.
        /// </summary>
        /// <param name="builder">The <see cref="OpenTelemetryBuilder"/> to configure.</param>
        /// <param name="azureMonitorCredential">The <see cref="TokenCredential"/> to use for Azure Monitor if you are using Managed Identity to push telemetry.</param>
        /// <returns>The <paramref name="builder"/> enabled for OpenTelemetry.</returns>
        public static OpenTelemetryBuilder UseFunctionsDefaults(this OpenTelemetryBuilder builder, TokenCredential? azureMonitorCredential = null)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder
                .WithTracing(o => o
                    .AddOtlpExporter()
                    .AddAzureMonitorTraceExporter(credential: azureMonitorCredential)
                    .AddLiveMetrics(o => o.Credential = azureMonitorCredential))
                .WithMetrics(o => o
                    .AddOtlpExporter()
                    .AddAzureMonitorMetricExporter(credential: azureMonitorCredential))
                .Services.AddLogging(b => b
                    .AddOpenTelemetry(o => o
                        .AddOtlpExporter()
                        .AddAzureMonitorLogExporter(credential: azureMonitorCredential)))

                // Lets the host know that the worker is sending logs to App Insights. The host will now ignore these.
                .Configure<WorkerOptions>(workerOptions => workerOptions.Capabilities["WorkerApplicationInsightsLoggingEnabled"] = bool.TrueString);

            return builder;
        }
    }
}
