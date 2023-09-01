using BankAccount.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankAccount.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BankAccountController : ControllerBase
    {
        private readonly IBankAccountService _bankAccountService;
        private readonly IBankManagementAccountService _bankManagementAccountService;
        private readonly ILogger<BankAccountController> _logger;

        public BankAccountController(
            IBankAccountService bankAccountService,
            IBankManagementAccountService bankManagementAccountService,
            ILogger<BankAccountController> logger)
        {
            _bankAccountService = bankAccountService;
            _bankManagementAccountService = bankManagementAccountService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] BankAccountDto bankAccount)
        {
            try
            {
                var newBankAccount = await _bankManagementAccountService.NewBankAccountAsync(bankAccount);
                return Ok(newBankAccount);
            }
            catch (Exception ex) when (ex.Message == "bank account already exists")
            {
                _logger.LogError(ex, "Failed to create a new bank account: Bank account already exists");
                return BadRequest("Bank account already exists");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a new bank account");
                return StatusCode(500, "An error occurred while creating a new bank account");
            }
        }

        [HttpPut("{id}/save")]
        public async Task<IActionResult> SaveMoneyAsync(int id, [FromBody] double amount)
        {
            try
            {
                var bankAccount = await _bankAccountService.Deposit(id, amount);
                return Ok(bankAccount);
            }
            catch (Exception ex) when (ex.Message == "bank account not found")
            {
                _logger.LogError(ex, "Failed to deposit money: Bank account not found");
                return BadRequest("Bank account not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while depositing money");
                return StatusCode(500, "An error occurred while depositing money");
            }
        }

        [HttpPut("{id}/withdraw")]
        public async Task<IActionResult> WithdrawMoneyAsync(int id, [FromBody] double amount)
        {
            try
            {
                var bankAccount = await _bankAccountService.Withdraw(id, amount);
                return Ok(bankAccount);
            }
            catch (Exception ex) when (ex.Message == "bank account not found")
            {
                _logger.LogError(ex, "Failed to withdraw money: Bank account not found");
                return BadRequest("Bank account not found");
            }
            catch (Exception ex) when (ex.Message == "not enough money in the account")
            {
                _logger.LogError(ex, "Failed to withdraw money: Not enough money in the account");
                return BadRequest("Not enough money in the account");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while withdrawing money");
                return StatusCode(500, "An error occurred while withdrawing money");
            }
        }

        [HttpGet("{id}/Info")]
        public async Task<IActionResult> GetUserInfoAsync(int id)
        {
            try
            {
                var bankAccount = await _bankAccountService.GetBankAccountAsync(id);
                return Ok(bankAccount);
            }
            catch (Exception ex) when (ex.Message == "bank account not found")
            {
                _logger.LogError(ex, "Failed to get bank account information: Bank account not found");
                return BadRequest("Bank account not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting bank account information");
                return StatusCode(500, "An error occurred while getting bank account information");
            }
        }

        [HttpGet("AllInfo")]
        public async Task<IActionResult> GetUserAllInfoAsync()
        {
            try
            {
                var bankAccount = await _bankManagementAccountService.ShowGoldAndBasicMembersAsync();
                return Ok(bankAccount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving user information");
                return StatusCode(500, "An error occurred while retrieving user information");
            }
        }

        [HttpGet("GetDeletedAcc")]
        public async Task<IActionResult> GetDeletedAccAsync()
        {
            try
            {
                var bankAccount = await _bankManagementAccountService.GetDeletedBankAccountsAsync();
                return Ok(bankAccount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving deleted bank accounts");
                return StatusCode(500, "An error occurred while retrieving deleted bank accounts");
            }
        }

        [HttpGet("TotalBankMoney")]
        public async Task<IActionResult> GetTotalMoneyAsync()
        {
            try
            {
                var bankAccount = await _bankManagementAccountService.BankTotalMoneyAsync();
                return Ok(bankAccount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the total bank money");
                return StatusCode(500, "An error occurred while retrieving the total bank money");
            }
        }

        [HttpGet("NumAcc")]
        public async Task<IActionResult> GetNumAccAsync()
        {
            try
            {
                var bankAccount = await _bankManagementAccountService.NumBankAccAsync();
                return Ok(bankAccount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the number of bank accounts");
                return StatusCode(500, "An error occurred while retrieving the number of bank accounts");
            }
        }

        [HttpGet("AllTransactions")]
        public async Task<IActionResult> GetAllTransactions()
        {
            try
            {
                var transactions = await _bankManagementAccountService.GetAllTransactionsAsync();
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all transactions");
                return StatusCode(500, "An error occurred while retrieving all transactions");
            }
        }

        [HttpDelete("{id}/RemoveAcc")]
        public async Task<IActionResult> RemoveAccAsync(int id)
        {
            try
            {
                await _bankManagementAccountService.RemoveBankAccountAsync(id);
                return Ok();
            }
            catch (Exception ex) when (ex.Message == "bank account not found" || ex.Message == "bank account already deleted")
            {
                _logger.LogError(ex, "Failed to remove bank account: Bank account not found or was already deleted");
                return BadRequest("Bank account not found or was already deleted");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while removing the bank account");
                return StatusCode(500, "An error occurred while removing the bank account");
            }
        }
    }
}
