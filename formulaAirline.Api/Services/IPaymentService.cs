using formulaAirline.Api.Model;

namespace formulaAirline.Api.Services
{
    public interface IPaymentService
    {
        Task<Payment?> GetPaymentByIdAsync(int paymentId);
        Task<IEnumerable<Payment>> GetPaymentsByBookingIdAsync(int bookingId);
        Task<IEnumerable<Payment>> GetAllPaymentsAsync();
        Task<Payment> CreatePaymentAsync(Payment payment);
        Task UpdatePaymentAsync(Payment payment);
        Task DeletePaymentAsync(int paymentId);
        Task<bool> ProcessPaymentAsync(int bookingId, decimal amount, string paymentMethod);
        Task<decimal> GetTotalAmountByBookingAsync(int bookingId);
    }
}
