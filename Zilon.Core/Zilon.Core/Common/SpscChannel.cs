﻿using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Zilon.Core.Common
{
    public sealed class SpscChannel<T> : ISender<T>, IReceiver<T>, IDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly IProducerConsumerCollection<TaskCompletionSource<T>> _receivers;
        private readonly IProducerConsumerCollection<T> _values;

        public SpscChannel()
        {
            _semaphore = new SemaphoreSlim(1);
            _receivers = new ConcurrentQueue<TaskCompletionSource<T>>();
            _values = new ConcurrentQueue<T>();
        }

        public async Task SendAsync(T obj)
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);

            try
            {
                if (_receivers.TryTake(out var receiver))
                {
                    receiver.SetResult(obj);
                }
                else
                {
                    _values.TryAdd(obj);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<T> ReceiveAsync()
        {
            TaskCompletionSource<T> source;
            await _semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_values.TryTake(out var value))
                {
                    return value;
                }
                else
                {
                    source = new TaskCompletionSource<T>();
                    _receivers.TryAdd(source);
                }
            }
            finally
            {
                _semaphore.Release();
            }

            return await source.Task.ConfigureAwait(false);
        }

        public void Dispose()
        {
            _semaphore.Dispose();
        }
    }
}
