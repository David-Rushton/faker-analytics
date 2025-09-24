using System;
using Spectre.Console.Cli;

namespace Dr.FakerAnalytics.Cli.Infrastructure;

public sealed class TypeResolver(IServiceProvider provider) : ITypeResolver, IDisposable
{
    public object? Resolve(Type? type) =>
        type is null
        ? throw new ArgumentNullException(nameof(type))
        : provider.GetService(type);


    public void Dispose()
    {
        if (provider is IDisposable disposable)
            disposable.Dispose();
    }
}
