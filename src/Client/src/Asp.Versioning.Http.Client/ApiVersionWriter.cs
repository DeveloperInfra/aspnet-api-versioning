﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Http;

/// <summary>
/// Provides utility functions for <see cref="IApiVersionWriter"/>.
/// </summary>
public static class ApiVersionWriter
{
    /// <summary>
    /// Returns a new API version writer that is a combination of the specified set.
    /// </summary>
    /// <param name="apiVersionWriter">The primary <see cref="IApiVersionWriter">API version writer</see>.</param>
    /// <param name="otherApiVersionwriters">An array of the other
    /// <see cref="IApiVersionWriter">API version writers</see> to combine.</param>
    /// <returns>A new, combined <see cref="IApiVersionWriter">API version writer</see>.</returns>
    public static IApiVersionWriter Combine(
        IApiVersionWriter apiVersionWriter,
        params IApiVersionWriter[] otherApiVersionwriters )
    {
        if ( apiVersionWriter == null )
        {
            throw new ArgumentNullException( nameof( apiVersionWriter ) );
        }

        int count;
        IApiVersionWriter[] apiVersionWriters;

        if ( otherApiVersionwriters is null || ( count = otherApiVersionwriters.Length ) == 0 )
        {
            apiVersionWriters = new[] { apiVersionWriter };
        }
        else
        {
            apiVersionWriters = new IApiVersionWriter[count + 1];
            apiVersionWriters[0] = apiVersionWriter;
            System.Array.Copy( otherApiVersionwriters, 0, apiVersionWriters, 1, count );
        }

        return new CombinedApiVersionWriter( apiVersionWriters );
    }

    /// <summary>
    /// Returns a new API version writer that is a combination of the specified set.
    /// </summary>
    /// <param name="apiVersionWriters">The <see cref="IEnumerable{T}">sequence</see> of
    /// <see cref="IApiVersionWriter">API version writers</see> to combine.</param>
    /// <returns>A new, combined <see cref="IApiVersionWriter">API version writer</see>.</returns>
    public static IApiVersionWriter Combine( IEnumerable<IApiVersionWriter> apiVersionWriters )
    {
        var writers = apiVersionWriters?.ToArray();

        if ( writers is null || writers.Length == 0 )
        {
            throw new ArgumentException( SR.ZeroApiVersionWriters, nameof( apiVersionWriters ) );
        }

        return new CombinedApiVersionWriter( writers );
    }

    private sealed class CombinedApiVersionWriter : IApiVersionWriter
    {
        private readonly IApiVersionWriter[] apiVersionWriters;

        public CombinedApiVersionWriter( IApiVersionWriter[] apiVersionWriters ) =>
            this.apiVersionWriters = apiVersionWriters;

        public void Write( HttpRequestMessage request, ApiVersion apiVersion )
        {
            for ( var i = 0; i < apiVersionWriters.Length; i++ )
            {
                apiVersionWriters[i].Write( request, apiVersion );
            }
        }
    }
}