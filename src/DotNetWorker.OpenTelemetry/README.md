# Microsoft.Azure.Functions.Worker.OpenTelemetry

This package adds extension methods and services to configure Open Telemetry for use in Azure Functions .NET isolated applications.

## Getting Started

1. Add packages

``` CSharp
dotnet add package Microsoft.Azure.Functions.Worker.OpenTelemetry
```

2. Configure Open Telemetry

``` CSharp
services.ConfigureFunctionsOpenTelemetry();
```

## Logging

This package does **not** add OpenTelemetry services directly. This must be done directly. Instead, this package only configures isolated application and resource detector.

## In-Proc Comparison / Changes

With this package changing the worker to send telemetry directly to application insights, custom `ITelemetryInitializer` or `ITelemetryProcessor` will only apply to worker-originating telemetry. Telemetry which originates from the host process will **not** be ran through the same telemetry pipeline. This means when compared to an in-proc functions app, you may see some telemetry items missing customizations performed in initializers or processors. These telemetry items have originated from the host.

## Configuration

See [this document](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/observability-with-otel) on configuring Open Telemetry for dotnet applications.
