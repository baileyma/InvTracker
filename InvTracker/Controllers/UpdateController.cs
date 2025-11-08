using InvTracker.Models;
using InvTracker.Services;
using InvTracker.Utils;
using Microsoft.AspNetCore.Mvc;

namespace InvTracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UpdateController : ControllerBase
    {
        private readonly IPaymentsRepository _paymentsRepo;
        private readonly IBalancesRepository _balancesRepo;

        public UpdateController(IPaymentsRepository paymentsRepo, IBalancesRepository balancesRepo)
        {
            _paymentsRepo = paymentsRepo;
            _balancesRepo = balancesRepo;
        }

        [HttpPost("add-payment")]
        public async Task<ActionResult> PostPayment(PaymentToAdd payment)
        {
            await _paymentsRepo.PostPayment(payment);

            return Created();
        }

        [HttpPut("update-balance")]
        public async Task<ActionResult> UpsertBalance([FromBody] BalanceToAdd balance)
        {
            await _balancesRepo.PostBalance(balance);

            return NoContent();
        }

        [HttpDelete("delete-payment/{paymentId}")]
        public async Task<ActionResult> DeletePayment([FromRoute] int paymentId)
        {
            await _paymentsRepo.DeletePayment(paymentId);

            return NoContent();
        }


        // learn result methods
        // ok, created/createdataction, notfound, badrequest, nocontent (for put)

        //[HttpPost("add-account")]
        //public async Task<ActionResult<Account>> AddAccount([FromBody] string accountName)
        //{
        //    var newAccount = await _repo.AddAccountAsync(accountName);
        //    await _repo.AddBalancesForAllYears(newAccount.Id);

        //    return CreatedAtAction(nameof(AccountsController.GetAccounts), new { id = newAccount.Id }, newAccount);
        //}

        //[HttpDelete("delete/{id}")]
        //public async Task<ActionResult> DeleteAccount(int accountId)
        //{
        //    await _repo.DeleteAccountById(accountId);

        //    return NoContent();
        //}
    }
}
