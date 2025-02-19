﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DynamicData.Tests.Utilities;

internal class NoOpComparer<T> : IComparer<T>
{
    public int Compare(T x, T y) => throw new NotImplementedException();
}

internal class NoOpEqualityComparer<T> : IEqualityComparer<T>
{
    public bool Equals(T x, T y) => throw new NotImplementedException();
    public int GetHashCode([DisallowNull] T obj) => throw new NotImplementedException();
}


internal class InvertedComparer<T> : IComparer<T>
{
    private readonly IComparer<T> _original;

    public InvertedComparer(IComparer<T> original) => _original = original;

    public int Compare(T x, T y) => _original.Compare(x, y) * -1;
}


internal static class ComparerExtensions
{
    public static IComparer<T> Invert<T>(this IComparer<T> comparer) => new InvertedComparer<T>(comparer);
}
