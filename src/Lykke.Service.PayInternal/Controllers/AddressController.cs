using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayInternal.Controllers
{
    [Route("api/[controller]")]
    public class AddressController : Controller
    {
        private readonly BitcoinApiClient _client;

        public AddressController(BitcoinApiClient client)
        {
            _client = client;
        }

        [HttpPost]
        [Route("SetBalance")]
        [SwaggerOperation("SetBalance")]
        public void SetBalance([FromBody] BalanceRequest request)
        {
            _client.SetBalance(request.Address, request.Balance);
        }

        [HttpPost]
        [Route("ResetBalance")]
        [SwaggerOperation("ResetBalance")]
        public void ResetBalance([FromBody] BalanceRequest request)
        {
            _client.ResetBalance();
        }

        public class BalanceRequest
        {
            public string Address { get; set; }
            public decimal Balance { get; set; }
        }
    }
}
