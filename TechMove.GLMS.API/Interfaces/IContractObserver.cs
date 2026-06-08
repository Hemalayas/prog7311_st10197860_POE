using TechMove.GLMS.API.Models;

namespace TechMove.GLMS.API.Interfaces;

public interface IContractObserver
{
    void Update(Contract contract, string oldStatus);
}
