using BankAcc.Dtos;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class BankAccounts
{
    [Key]
    public int AccountNumber { get; set; }
    public double TotalMoney { get; set; }
    public AccountType Type { get; set; }

    public DateTime? DeletedDateTime { get; set; }
    public List<TransactionDto> Transactions { get; set; }

    public BankAccounts()
    {
        Type = AccountType.Basic;
        Transactions = new List<TransactionDto>();
    }
}