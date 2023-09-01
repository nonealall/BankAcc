using System;
using System.Collections.Generic;
using AutoMapper;
using BankAcc.Dtos;
using BankAccount.Entities;

public class BankAccountDto
{
    public int AccountNumber { get; set; }
    public double TotalMoney { get; set; }
    public AccountType Type { get; set; }
    public DateTime? DeletedDateTime { get; set; }
    public List<TransactionDto>? Transactions { get; set; }

    public BankAccountDto()
    {
        Transactions = new List<TransactionDto>();
    }

    public BankAccountDto(BankAccounts accountEntity)
    {
        AccountNumber = accountEntity.AccountNumber;
        TotalMoney = accountEntity.TotalMoney;
        Type = accountEntity.Type;
        DeletedDateTime = accountEntity.DeletedDateTime;
        Transactions = new List<TransactionDto>();
    }

    public static void ConfigureMapping(Profile profile)
    {
        profile.CreateMap<BankAccounts, BankAccountDto>()
            .ForMember(dest => dest.Transactions, opt => opt.Ignore());

        profile.CreateMap<BankAccountDto, BankAccounts>()
            .ForMember(dest => dest.Transactions, opt => opt.Ignore());
    }
}