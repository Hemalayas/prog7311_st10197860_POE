using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TechMove.GLMS.API.Interfaces;

namespace TechMove.GLMS.API.Models;

public enum ContractStatus
{
    Draft,
    Active,
    Expired,
    OnHold
}

public class Contract
{
    private readonly List<IContractObserver> _observers = new();

    public int Id { get; set; }

    [Required]
    public int ClientId { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    public ContractStatus Status { get; set; } = ContractStatus.Draft;

    [StringLength(200)]
    public string? ServiceLevel { get; set; }

    public string? SignedAgreementPath { get; set; }

    [ForeignKey(nameof(ClientId))]
    public Client? Client { get; set; }

    public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();

    public void Attach(IContractObserver observer) => _observers.Add(observer);

    public void Detach(IContractObserver observer) => _observers.Remove(observer);

    public void SetStatus(ContractStatus newStatus)
    {
        var oldStatus = Status.ToString();
        Notify(oldStatus);
        Status = newStatus;
    }

    private void Notify(string oldStatus)
    {
        foreach (var observer in _observers)
            observer.Update(this, oldStatus);
    }
}
