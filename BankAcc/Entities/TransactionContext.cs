using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace BankAccount.Entities
{
    public class TransactionEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } // Primary key for TransactionEntity

        public double Amount { get; set; }
        public TransactionType Type { get; set; }

        public int AccountNumber { get; set; } // Foreign key to BankAccounts

        [ForeignKey("AccountNumber")]
        public BankAccounts BankAccount { get; set; } // Navigation property

        public TransactionEntity()
        {
            BankAccount = new BankAccounts();
        }
    }
}