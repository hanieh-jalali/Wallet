using Microsoft.AspNetCore.Mvc;
using NBitcoin;
using System.Linq;
using System.Threading.Tasks;
using Wallet.Domain.Contract.Repositories;

namespace Wallet.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AddressController : ControllerBase
    {
        private readonly IAddressRepository _addressRepository;

        public AddressController(IAddressRepository addressRepository)
        {
            _addressRepository = addressRepository;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddAddress(string walletName)
        {
            await _addressRepository.AddAddressAsync(walletName);
            return Ok(new { message = "Address added successfully" });
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAllAddresses(string walletName)
        {
            var addresses = await _addressRepository.GetAllAddressesAsync(walletName);

            if (addresses == null || !addresses.Any())
            {
                return NotFound(new { message = "No addresses found for this wallet." });
            }

            return Ok(addresses);
        }

        [HttpGet("get")]
        public async Task<IActionResult> GetAddress(string walletName, int index)
        {
            try
            {
                var address = await _addressRepository.GetAddressAsync(walletName, index);
                return Ok(new { address });
            }
            catch (IndexOutOfRangeException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
