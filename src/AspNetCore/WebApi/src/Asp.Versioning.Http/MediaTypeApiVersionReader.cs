﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Microsoft.AspNetCore.Http;

/// <content>
/// Provides the implementation for ASP.NET Core.
/// </content>
[CLSCompliant( false )]
public partial class MediaTypeApiVersionReader
{
    /// <inheritdoc />
    public virtual IReadOnlyList<string> Read( HttpRequest request )
    {
        if ( request == null )
        {
            throw new ArgumentNullException( nameof( request ) );
        }

        var headers = request.GetTypedHeaders();
        var contentType = headers.ContentType;
        var version = contentType is null ? default : ReadContentTypeHeader( contentType );
        var accept = headers.Accept;

        if ( accept is null || ReadAcceptHeader( accept ) is not string otherVersion )
        {
            return version is null ? Array.Empty<string>() : new[] { version };
        }

        var comparer = StringComparer.OrdinalIgnoreCase;

        if ( version is null || comparer.Equals( version, otherVersion ) )
        {
            return new[] { otherVersion };
        }

        return comparer.Compare( version, otherVersion ) <= 0 ?
            new[] { version, otherVersion } :
            new[] { otherVersion, version };
    }
}