using System;

namespace HoloCure.ModLoader.Utils
{
    public sealed class Disposable<T> : IDisposable
    {
        private readonly T Value;
        private readonly Action<T> Dispose;

        public Disposable(T value, Action<T> dispose)
        {
            Value = value;
            Dispose = dispose;
        }

        void IDisposable.Dispose()
        {
            Dispose(Value);
        }
    }
}