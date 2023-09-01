using AutoMapper;
using BankAcc.Dtos;
using BankAccount.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class BankAccountRepository : IBankAccountRepository
{
    private readonly BankContext _context;
    private readonly IMapper _mapper;

    public BankAccountRepository(BankContext context, IMapper mapper)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<BankAccountDto?> GetBankAccount(int id)
    {
        try
        {
            var bankAccount = await _context.Bankaccounts
                .Include(a => a.Transactions)
                .FirstOrDefaultAsync(a => a.AccountNumber == id && a.DeletedDateTime == null);

            return _mapper.Map<BankAccountDto>(bankAccount);
        }
        catch (Exception ex)
        {
            // Log the exception or handle it as per your application's error handling strategy
            throw new RepositoryException("Error occurred while fetching the bank account.", ex);
        }
    }

    public async Task<List<BankAccountDto>> GetAllBankAccounts()
    {
        try
        {
            var bankAccounts = await _context.Bankaccounts
                .Include(a => a.Transactions)
                .Where(m => m.DeletedDateTime == null)
                .Select(bankAccount => _mapper.Map<BankAccountDto>(bankAccount))
                .ToListAsync();

            return bankAccounts;
        }
        catch (Exception ex)
        {
            // Log the exception or handle it as per your application's error handling strategy
            throw new RepositoryException("Error occurred while fetching all bank accounts.", ex);
        }
    }

    public async Task<List<BankAccountDto>> GetAllDeletedBankAccounts()
    {
        try
        {
            var bankAccounts = await _context.Bankaccounts
                .Include(a => a.Transactions)
                .Where(m => m.DeletedDateTime != null)
                .Select(bankAccount => _mapper.Map<BankAccountDto>(bankAccount))
                .ToListAsync();

            return bankAccounts;
        }
        catch (Exception ex)
        {
            // Log the exception or handle it as per your application's error handling strategy
            throw new RepositoryException("Error occurred while fetching all deleted bank accounts.", ex);
        }
    }

    public async Task<BankAccountDto> InsertBankAccountAsync(BankAccountDto bankAccountDto, int id)
    {
        try
        {
            var bankAccount = _mapper.Map<BankAccounts>(bankAccountDto);
            bankAccount.AccountNumber = id;

            _context.Bankaccounts.Add(bankAccount);
            await _context.SaveChangesAsync();

            return _mapper.Map<BankAccountDto>(bankAccount);
        }
        catch (Exception ex)
        {
            // Log the exception or handle it as per your application's error handling strategy
            throw new RepositoryException("Error occurred while inserting the bank account.", ex);
        }
    }

    public async Task<BankAccountDto> UpdateBankAccountAsync(BankAccountDto bankAccountDto, int id)
    {
        try
        {
            var bankAccount = await _context.Bankaccounts
                .Include(a => a.Transactions)
                .FirstOrDefaultAsync(a => a.AccountNumber == id && a.DeletedDateTime == null);

            if (bankAccount != null)
            {
                bankAccount.AccountNumber = bankAccountDto.AccountNumber;
                bankAccount.TotalMoney = bankAccountDto.TotalMoney;
                bankAccount.Type = bankAccountDto.Type;
                bankAccount.DeletedDateTime = bankAccountDto.DeletedDateTime;
                bankAccount.Transactions.Clear();

                if (bankAccountDto.Transactions != null)
                {
                    foreach (var transactionDto in bankAccountDto.Transactions)
                    {
                        var transactionEntity = _mapper.Map<TransactionEntity>(transactionDto);
                        transactionEntity.AccountNumber = id;
                        bankAccount.Transactions.Add(transactionEntity);
                    }
                }
                await _context.SaveChangesAsync();
            }

            return _mapper.Map<BankAccountDto>(bankAccount);
        }
        catch (Exception ex)
        {
            // Log the exception or handle it as per your application's error handling strategy
            throw new RepositoryException("Error occurred while updating the bank account.", ex);
        }
    }

    public async Task DeleteBankAccountAsync(int id)
    {
        try
        {
            var bankAccount = await _context.Bankaccounts
                .Include(a => a.Transactions)
                .FirstOrDefaultAsync(a => a.AccountNumber == id);

            if (bankAccount != null)
            {
                bankAccount.DeletedDateTime = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            // Log the exception or handle it as per your application's error handling strategy
            throw new RepositoryException("Error occurred while deleting the bank account.", ex);
        }
    }

    public async Task<List<TransactionDto>> GetTransactionReportForAllUsers()
    {
        try
        {
            var transactions = await (from acc in _context.Bankaccounts
                                      join tran in _context.Transactions on acc.AccountNumber equals tran.AccountNumber
                                      where acc.DeletedDateTime == null
                                      orderby tran.TransactionDate // Order by the transaction date
                                      select tran).ToListAsync();

            var transactionDtos = _mapper.Map<List<TransactionDto>>(transactions);
            return transactionDtos;
        }
        catch (Exception ex)
        {
            // Log the exception or handle it as per your application's error handling strategy
            throw new RepositoryException("Error occurred while fetching the transaction report.", ex);
        }
    }
}
