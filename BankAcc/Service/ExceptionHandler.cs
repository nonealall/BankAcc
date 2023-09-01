using System;

using System;

public class NotFoundException : Exception
{
    public NotFoundException()
    {
    }

    public NotFoundException(string message)
        : base(message)
    {
    }

    public NotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

public class InvalidOperationException : Exception
{
    public InvalidOperationException()
    {
    }

    public InvalidOperationException(string message)
        : base(message)
    {
    }

    public InvalidOperationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
using System;


public class InsufficientFundsException : Exception
{
    public InsufficientFundsException()
    {
    }

    public InsufficientFundsException(string message)
        : base(message)
    {
    }

    public InsufficientFundsException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

public class RepositoryException : Exception
{
    public RepositoryException()
    {
    }

    public RepositoryException(string message)
        : base(message)
    {
    }

    public RepositoryException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
    
}
