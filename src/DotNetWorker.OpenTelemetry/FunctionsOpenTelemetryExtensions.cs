// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Microsoft.Azure.Functions.Worker
{
    using System;
    using System.Diagnostics;

    using global::Azure.Monitor.OpenTelemetry.AspNetCore;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    using OpenTelemetry;
    using OpenTelemetry.Logs;
    using OpenTelemetry.Metrics;
    using OpenTelemetry.Resources;
    using OpenTelemetry.Trace;

    public static class FunctionsOpenTelemetryExtensions
    {
        /// <summary>
        /// Configures OpenTelemetry for Functions.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
        /// <param name="context">The <see cref="HostBuilderContext"/>.</param>
        /// <returns>An instance of <see cref="OpenTelemetryBuilder"/> if OpenTelemetry is enabled, otherwise null.</returns>
        public static OpenTelemetryBuilder? ConfigureFunctionsOpenTelemetry(this IServiceCollection services, HostBuilderContext context)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (bool.TryParse(Environment.GetEnvironmentVariable("OTEL_SDK_DISABLED") ?? bool.TrueString, out var b) && !b)
            {
                return services.AddLogging(b => b.AddOpenTelemetry(o => o.AddOtlpExporter()))
                    // Lets the host know that the worker is sending logs to App Insights. The host will now ignore these.
                    .Configure<WorkerOptions>(workerOptions => workerOptions.Capabilities["WorkerApplicationInsightsLoggingEnabled"] = bool.TrueString)
                    .AddOpenTelemetry()
                    .ConfigureResource(r =>
                    {
                        var envVars = Environment.GetEnvironmentVariables();
                        // Set the AI SDK to a key so we know all the telemetry came from the Functions Host
                        // NOTE: This ties to \azure-sdk-for-net\sdk\monitor\Azure.Monitor.OpenTelemetry.Exporter\src\Internals\ResourceExtensions.cs :: AiSdkPrefixKey used in CreateAzureMonitorResource()
                        var version = typeof(WorkerOptions).Assembly.GetName().Version!.ToString();
                        r.AddService("azureFunctions", serviceVersion: version);
                        r.AddAttributes([
                            new("ai.sdk.prefix", $@"azurefunctionscoretools: {version} "),
                            new("azurefunctionscoretools_version", version),
                            //new("RoleInstanceId", hostOptions?.CurrentValue.InstanceId ?? string.Empty),
                            new("ProcessId", Process.GetCurrentProcess().Id)
                        ]);
                    })
                    .WithTracing(o => o.AddOtlpExporter())
                    .WithMetrics(o => o.AddOtlpExporter())
                    .UseAzureMonitor();
            }

            return default;
        }
    }
}
