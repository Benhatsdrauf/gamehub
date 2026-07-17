namespace GameHub.Domain.Core;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
}