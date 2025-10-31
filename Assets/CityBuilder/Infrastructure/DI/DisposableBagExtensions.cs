using System;
using MessagePipe;

namespace CityBuilder.Infrastructure.DI
{
    public static class DisposableBagExtensions
    {
        public static void AddTo(this IDisposable disposable, DisposableBagBuilder builder)
        {
            builder.Add(disposable);
        }
    }
}