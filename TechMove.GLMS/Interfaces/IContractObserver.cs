using TechMove.GLMS.Models;

namespace TechMove.GLMS.Interfaces;

public interface IContractObserver
{
    void Update(Contract contract, string oldStatus);
}
