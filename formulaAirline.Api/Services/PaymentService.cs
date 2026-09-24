using formulaAirline.Api.Model;
using formulaAirline.Api.Repository;

namespace formulaAirline.Api.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IRepository<Payment> _paymentRepository;
        private readonly IRepository<Booking> _bookingRepository;
        private readonly IMessageProducer _messageProducer;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IRepository<Payment> paymentRepository,
            IRepository<Booking> bookingRepository,
            IMessageProducer messageProducer,
            ILogger<PaymentService> logger)
        {
            _paymentRepository = paymentRepository;
            _bookingRepository = bookingRepository;
            _messageProducer = messageProducer;
            _logger = logger;
        }

        public async Task<Payment?> GetPaymentByIdAsync(int paymentId)
        {
            try
            {
                return await _paymentRepository.GetByIdAsync(paymentId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving payment with ID {paymentId}: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByBookingIdAsync(int bookingId)
        {
            try
            {
                return await _paymentRepository.FindAsync(p => p.BookingId == bookingId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving payments for booking {bookingId}: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<Payment>> GetAllPaymentsAsync()
        {
            try
            {
                return await _paymentRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving all payments: {ex.Message}");
                throw;
            }
        }

        public async Task<Payment> CreatePaymentAsync(Payment payment)
        {
            try
            {
                payment.PaymentDate = DateTime.UtcNow;
                payment.TransactionId = Guid.NewGuid().ToString();
                await _paymentRepository.AddAsync(payment);
                _logger.LogInformation($"Payment {payment.Id} created for booking {payment.BookingId}");
                return payment;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating payment: {ex.Message}");
                throw;
            }
        }

        public async Task UpdatePaymentAsync(Payment payment)
        {
            try
            {
                await _paymentRepository.UpdateAsync(payment);
                _logger.LogInformation($"Payment {payment.Id} updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating payment: {ex.Message}");
                throw;
            }
        }

        public async Task DeletePaymentAsync(int paymentId)
        {
            try
            {
                var payment = await _paymentRepository.GetByIdAsync(paymentId);
                if (payment != null)
                {
                    await _paymentRepository.DeleteAsync(payment);
                    _logger.LogInformation($"Payment {paymentId} deleted successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting payment: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> ProcessPaymentAsync(int bookingId, decimal amount, string paymentMethod)
        {
            try
            {
                // Verify booking exists
                var booking = await _bookingRepository.GetByIdAsync(bookingId);
                if (booking == null)
                {
                    _logger.LogError($"Booking {bookingId} not found");
                    return false;
                }

                // Create payment record
                var payment = new Payment
                {
                    BookingId = bookingId,
                    Amount = amount,
                    PaymentMethod = paymentMethod,
                    Status = "Completed",
                    PaymentDate = DateTime.UtcNow,
                    TransactionId = Guid.NewGuid().ToString()
                };

                // Save payment
                await CreatePaymentAsync(payment);

                // Send message through message producer for further processing
                _messageProducer.SendingMessages<Payment>(payment);

                _logger.LogInformation($"Payment processed successfully for booking {bookingId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing payment: {ex.Message}");
                return false;
            }
        }

        public async Task<decimal> GetTotalAmountByBookingAsync(int bookingId)
        {
            try
            {
                var payments = await GetPaymentsByBookingIdAsync(bookingId);
                return payments.Where(p => p.Status == "Completed").Sum(p => p.Amount);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error calculating total payment amount: {ex.Message}");
                throw;
            }
        }
    }
}
