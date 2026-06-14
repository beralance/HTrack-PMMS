namespace PMMS.Server.Common.Exceptions
{
    public abstract class DomainException(string message) : Exception(message);

    public class ForbiddenException(string message = "You do not have permission to access this resource.") 
        : DomainException(message);
    public class NotFoundException(string name, object key)
        : DomainException($"{name} with id ({key}) was not found.");
    public class AlreadyExistsException(string name, string value) 
        : DomainException($"{name} with value '{value}' already exists.");
    public class BusinessRuleException(string message) : DomainException(message);
}