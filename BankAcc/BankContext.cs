namespace BankAccount.Entities
{
    public class BankContext : DbContext
    {
        protected readonly IConfiguration Configuration;

        public BankContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(Configuration.GetConnectionString("PostgreSQL"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BankAccounts>()
                .HasMany(b => b.Transactions)
                .WithOne(t => t.BankAccount)
                .HasForeignKey(t => t.AccountNumber);

            base.OnModelCreating(modelBuilder);
        }
        public DbSet<BankAccounts> Bankaccounts { get; set; } = null!;
        public DbSet<TransactionEntity> Transactions { get; set; }
    }
}