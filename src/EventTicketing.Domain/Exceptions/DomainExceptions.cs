namespace EventTicketing.Domain.Exceptions;

public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
}

public class InvalidTicketTransitionException : DomainException
{
    public InvalidTicketTransitionException(string message) : base(message) { }
}

public class SeatUnavailableException : DomainException
{
    public SeatUnavailableException(string message) : base(message) { }
}

public class CapacityExceededException : DomainException
{
    public CapacityExceededException(string message) : base(message) { }
}

public class BusinessRuleException : DomainException
{
    public BusinessRuleException(string message) : base(message) { }
}

public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string message) : base(message) { }
}

public class InvalidCredentialsException : DomainException
{
    public InvalidCredentialsException(string message) : base(message) { }
}

public class ForbiddenException : DomainException
{
    public ForbiddenException(string message) : base(message) { }
}
