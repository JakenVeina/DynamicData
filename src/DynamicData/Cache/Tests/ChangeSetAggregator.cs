// Copyright (c) 2011-2023 Roland Pheasant. All rights reserved.
// Roland Pheasant licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Disposables;
using System.Reactive.Linq;

using DynamicData.Diagnostics;

// ReSharper disable once CheckNamespace
namespace DynamicData.Tests;

/// <summary>
/// Aggregates all events and statistics for a change set to help assertions when testing.
/// </summary>
/// <typeparam name="TObject">The type of the object.</typeparam>
/// <typeparam name="TKey">The type of the key.</typeparam>
public class ChangeSetAggregator<TObject, TKey> : IDisposable
    where TObject : notnull
    where TKey : notnull
{
    private readonly IDisposable _disposer;

    private bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangeSetAggregator{TObject, TKey}"/> class.
    /// </summary>
    /// <param name="source">The source.</param>
    public ChangeSetAggregator(IObservable<IChangeSet<TObject, TKey>> source)
    {
        var published = source.Publish();

        Data = published.AsObservableCache();

        var results = published.Subscribe(updates => Messages.Add(updates), ex => Error = ex, () => Completed = true);
        var summariser = published.CollectUpdateStats().Subscribe(summary => Summary = summary, _ => { });
        var connected = published.Connect();

        _disposer = Disposable.Create(
            () =>
            {
                Data.Dispose();
                connected.Dispose();
                summariser.Dispose();
                results.Dispose();
            });
    }

    /// <summary>
    /// Gets the data.
    /// </summary>
    /// <value>
    /// The data.
    /// </value>
    public IObservableCache<TObject, TKey> Data { get; }

    /// <summary>
    /// Gets the error.
    /// </summary>
    /// <value>
    /// The error.
    /// </value>
    public Exception? Error { get; private set; }

    /// <summary>
    /// Gets a value indicating whether or not the ChangeSet fired OnCompleted.
    /// </summary>
    /// <value>
    /// Boolean Value.
    /// </value>
    public bool Completed { get; private set; }

    /// <summary>
    /// Gets the messages.
    /// </summary>
    /// <value>
    /// The messages.
    /// </value>
    public IList<IChangeSet<TObject, TKey>> Messages { get; } = new List<IChangeSet<TObject, TKey>>();

    /// <summary>
    /// Gets the summary.
    /// </summary>
    /// <value>
    /// The summary.
    /// </value>
    public ChangeSummary Summary { get; private set; } = ChangeSummary.Empty;

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes of managed and unmanaged responses.
    /// </summary>
    /// <param name="isDisposing">If being called by the Dispose method.</param>
    protected virtual void Dispose(bool isDisposing)
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;

        if (isDisposing)
        {
            _disposer.Dispose();
        }
    }
}
