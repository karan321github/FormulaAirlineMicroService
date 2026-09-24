using formulaAirline.Api.Model;
using formulaAirline.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace formulaAirline.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IPaymentService paymentService, ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPayment(int id)
        {
            try
            {
                var payment = await _paymentService.GetPaymentByIdAsync(id);
                if (payment == null)
                    return NotFound($"Payment with ID {id} not found");

                return Ok(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving payment: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("booking/{bookingId}")]
        public async Task<IActionResult> GetPaymentsByBooking(int bookingId)
        {
            try
            {
                var payments = await _paymentService.GetPaymentsByBookingIdAsync(bookingId);
                return Ok(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving payments: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPayments()
        {
            try
            {
                var payments = await _paymentService.GetAllPaymentsAsync();
                return Ok(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving payments: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] Payment payment)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var createdPayment = await _paymentService.CreatePaymentAsync(payment);
                return CreatedAtAction(nameof(GetPayment), new { id = createdPayment.Id }, createdPayment);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating payment: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePayment(int id, [FromBody] Payment payment)
        {
            try
            {
                if (id != payment.Id)
                    return BadRequest("ID mismatch");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                await _paymentService.UpdatePaymentAsync(payment);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating payment: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePayment(int id)
        {
            try
            {
                await _paymentService.DeletePaymentAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting payment: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("process")]
        public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _paymentService.ProcessPaymentAsync(
                    request.BookingId,
                    request.Amount,
                    request.PaymentMethod);

                if (!result)
                    return BadRequest("Payment processing failed");

                return Ok(new { success = true, message = "Payment processed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing payment: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("booking/{bookingId}/total")]
        public async Task<IActionResult> GetTotalPaymentAmount(int bookingId)
        {
            try
            {
                var total = await _paymentService.GetTotalAmountByBookingAsync(bookingId);
                return Ok(new { bookingId, totalAmount = total });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error calculating total amount: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
    }

    public class ProcessPaymentRequest
    {
        public int BookingId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "";
    }
}
