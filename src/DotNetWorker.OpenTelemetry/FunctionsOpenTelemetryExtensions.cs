// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Microsoft.Azure.Functions.Worker
{
    using System;

    using global::Azure.Monitor.OpenTelemetry.AspNetCore;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    using OpenTelemetry;

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
                var otBuilder = services.AddOpenTelemetry().UseAzureMonitor();

                // Lets the host know that the worker is sending logs to App Insights. The host will now ignore these.
                services.Configure<WorkerOptions>(workerOptions => workerOptions.Capabilities["WorkerApplicationInsightsLoggingEnabled"] = bool.TrueString);

                return otBuilder;
            }

            return default;
        }
    }
}
