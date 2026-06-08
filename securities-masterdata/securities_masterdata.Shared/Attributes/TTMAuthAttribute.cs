using ttm_system.Shared.Enums;

namespace ttm_system.Shared.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class TTMAuthAttribute : Attribute
{
    public AuthLevel AuthLevel { get; }

    public TTMAuthAttribute(AuthLevel authLevel)
    {
        AuthLevel = authLevel;
    }
}