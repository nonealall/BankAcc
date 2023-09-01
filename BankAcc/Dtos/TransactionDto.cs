using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankAcc.Dtos
{
    public class TransactionDto
    {
        [Key]
        public int TransactionId { get; set; } // Primary key for TransactionDto

        public double Amount { get; set; }
        public TransactionType Type { get; set; }

        public int AccountNumber { get; set; } // Foreign key to BankAccount

        [ForeignKey("AccountNumber")]
        public BankAccounts BankAccount { get; set; } // Navigation property

        public TransactionDto()
        {
        }

        public TransactionDto(double amount, TransactionType type, int accountNumber)
        {
            Amount = amount;
            Type = type;
            AccountNumber = accountNumber;
        }
    }
}