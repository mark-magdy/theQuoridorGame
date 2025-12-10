using System;
using System.Collections.Generic;

namespace QuoridorBackend.Api.Middleware.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message) { }
}

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message) { }
}

public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message) { }
}

public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }
    
    public ValidationException(IDictionary<string, string[]> errors) 
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }
}