using securities_masterdata.DataAccess.Interfaces;
using securities_masterdata.Domain.Interfaces;
using securities_masterdata.Domain.Models.Command;

namespace securities_masterdata.Domain.Handlers.Commands;

public class CmdAddIndexSecurityHandler : ICmdAddIndexSecurityHandler
{
    private readonly IIndexRepository _indexRepository;

    public CmdAddIndexSecurityHandler(IIndexRepository indexRepository)
    {
        _indexRepository = indexRepository;
    }

    public async Task HandleAddIndexSecurity(AddIndexSecurityCmd cmd)
    {
        var index = await _indexRepository.GetIndexById(cmd.IndexId);
        if (index == null)
        {
            return;
        }

        foreach (var cmdSecurities in cmd.IndexSecurities)
        {
            var security = index?.IndexSecurities.FirstOrDefault(i => i.SecurityId == cmdSecurities.SecurityId);
            if (security == null)
            {
                index.IndexSecurities.Add(new()
                    { SecurityId = cmdSecurities.SecurityId, IndexId = cmd.IndexId, Weight = cmdSecurities.Weight });
            }
            else
            {
                security.Weight = cmdSecurities.Weight;
            }

        }
        
        await _indexRepository.SaveIndex(index);
    }
}