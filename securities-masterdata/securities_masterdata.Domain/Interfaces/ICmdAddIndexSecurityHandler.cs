using securities_masterdata.Domain.Models.Command;

namespace securities_masterdata.Domain.Interfaces;

public interface ICmdAddIndexSecurityHandler
{
    Task HandleAddIndexSecurity(AddIndexSecurityCmd cmd);
}