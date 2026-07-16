using portfolio.DataAccess.Models.Views;
using portfolio.Domain.Models;
using Riok.Mapperly.Abstractions;

namespace portfolio.Domain.Mappers;

[Mapper(UseDeepCloning = true)]
public partial class DtoMapper
{
    public partial SessionDto MapToSessionDto(SessionCountView session);
}