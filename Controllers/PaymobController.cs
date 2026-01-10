using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using paymob.paymob;

namespace paymob.Controllers
{
    [ApiController]
    [Route("api/paymob")]
    public class PaymobController : ControllerBase
    {
        private readonly IPaymobService _paymobService;

        public PaymobController(IPaymobService paymobService)
        {
            _paymobService = paymobService;
        }

        [HttpGet("test")]
        public async Task<IActionResult> TestPayment(decimal amount = 100, string method = "card")
        {
            try
            {
                var url = await _paymobService.ProcessPaymentAsync(amount, method);
                return Ok(new { payment_url = url });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
