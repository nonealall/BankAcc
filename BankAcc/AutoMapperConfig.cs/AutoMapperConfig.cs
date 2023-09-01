using AutoMapper;
using BankAcc.Dtos;
using BankAccount.Entities;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

public class AutoMapperConfig : Profile
{
    public AutoMapperConfig()
    {
        CreateMap<BankAccounts, BankAccountDto>()
            .ForMember(dest => dest.Transactions, opt => opt.MapFrom(src => src.Transactions));

        CreateMap<TransactionEntity, TransactionDto>();

        CreateMap<BankAccountDto, BankAccounts>()
            .ForMember(dest => dest.Transactions, opt => opt.MapFrom(src => src.Transactions));

        CreateMap<TransactionDto, TransactionEntity>();
    }

}