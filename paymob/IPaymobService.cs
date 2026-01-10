namespace paymob.paymob
{
    public interface IPaymobService
    {
         Task<string> ProcessPaymentAsync(decimal amount, string paymentMethod);
         string ComputeHmacSHA512(string data, string secret);
    }
}
