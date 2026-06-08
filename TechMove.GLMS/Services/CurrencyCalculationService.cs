namespace TechMove.GLMS.Services;

public class CurrencyCalculationService
{
    public decimal ConvertUsdToZar(decimal? usdAmount, decimal rate)
    {
        if (usdAmount is null)
            throw new ArgumentNullException(nameof(usdAmount), "Amount cannot be null.");
        if (usdAmount < 0)
            throw new ArgumentException("Amount cannot be negative.", nameof(usdAmount));
        if (rate < 0)
            throw new ArgumentException("Exchange rate cannot be negative.", nameof(rate));
        return usdAmount.Value * rate;
    }
}
