using Microsoft.Extensions.DependencyInjection;
using Xunit;
using BankAccount.Controllers;
using BankAccount.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BankAcc.Dtos;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Castle.Core.Configuration;
using Microsoft.Extensions.Options;
using AutoFixture;
using Microsoft.Extensions.Logging;

namespace BankAccountIntTest
{
    [CollectionDefinition("IntegrationTestCollection", DisableParallelization = true)]
    public class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture>
    {
    }

    [Collection("IntegrationTestCollection")]
    public class IntegrationTestFixture 
    {
        public IServiceProvider ServiceProvider { get; }

        protected readonly BankContext _context;

        private IMapper _mapper;
        private ILogger<BankAccountController> _logger;

        public IntegrationTestFixture()
        {

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            _context = new BankContext(configuration);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            var services = new ServiceCollection();

            services.AddSingleton<BankContext>(_context);
            services.AddScoped<IBankManagementAccountService, BankManagementAccountService>();
            services.AddScoped<IBankAccountService, BankAccountService>();
            services.AddScoped<IBankAccountRepository, BankAccountRepository>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<BankAccounts, BankAccountDto>();
                cfg.CreateMap<BankAccountDto, BankAccounts>();
                cfg.CreateMap<TransactionEntity, TransactionDto>();
                cfg.CreateMap<TransactionDto, TransactionEntity>();
            });
            _mapper = config.CreateMapper();
            services.AddSingleton(_mapper);

            services.AddScoped<BankAccountController>();

            services.AddLogging(builder =>
            {
                builder.AddConsole();
            });

            ServiceProvider = services.BuildServiceProvider();
            ResetDatabaseAsync().Wait();
        }

        public async Task ResetDatabaseAsync()
        {
            _context.RemoveRange(_context.Bankaccounts);
            _context.RemoveRange(_context.Transactions);
            await _context.SaveChangesAsync();
            await SeedTestData();
            await _context.SaveChangesAsync();
        }

        private async Task SeedTestData()
        {
            var users = new List<BankAccounts>
            {
                new BankAccounts { AccountNumber = 1, TotalMoney = 2000, Type = AccountType.Gold, DeletedDateTime = null},
                new BankAccounts { AccountNumber = 2, TotalMoney = 900, Type = AccountType.Basic, DeletedDateTime = null },
                new BankAccounts { AccountNumber = 3, TotalMoney = 300000, Type = AccountType.Gold, DeletedDateTime = null },
                new BankAccounts { AccountNumber = 4, TotalMoney = 1000, Type = AccountType.Basic, DeletedDateTime = null },
                new BankAccounts { AccountNumber = 6, TotalMoney = 500, Type = AccountType.Basic, DeletedDateTime = DateTime.UtcNow }
            };

            await _context.Bankaccounts.AddRangeAsync(users); 
            await _context.SaveChangesAsync();

            var transactions = new List<TransactionEntity>
            {
                new TransactionEntity { TransactionIdentification = Guid.NewGuid(), Amount = 1000, Type = TransactionType.Deposit, AccountNumber = 1, TransactionDate = DateTime.UtcNow},
                new TransactionEntity { TransactionIdentification = Guid.NewGuid(), Amount = 500, Type = TransactionType.Withdraw, AccountNumber = 1, TransactionDate = DateTime.UtcNow }
            };

            var transactionEntity = _mapper.Map<List<TransactionEntity>>(transactions);

            await _context.Transactions.AddRangeAsync(transactionEntity);

            await _context.SaveChangesAsync();
        }
    
    }

    public class IntegrationTestsBase<TController> : IClassFixture<IntegrationTestFixture> where TController : class
    {
        protected readonly TController _controller;

        protected readonly IMapper _mapper;

        protected readonly ILogger<BankAccountController> _logger;

        public IntegrationTestsBase(IntegrationTestFixture fixture)
        {
            _controller = fixture.ServiceProvider.GetRequiredService<TController>();
            _mapper = fixture.ServiceProvider.GetRequiredService<IMapper>();
            _logger = fixture.ServiceProvider.GetRequiredService<ILogger<BankAccountController>>();
        }
    }

    public class IntegrationTests : IntegrationTestsBase<BankAccountController>
    {
        private IntegrationTestFixture Fixture;
        public IntegrationTests(IntegrationTestFixture fixture) : base(fixture)
        {
            Fixture = fixture;
        }

        [Fact]
        public async Task CreateUser_SuccessfullyCreatesUser()
        {
            await Fixture.ResetDatabaseAsync();
            // Arrange
            var newUser = new BankAccountDto
            {
                AccountNumber = 5,
                TotalMoney = 1000,
                Type = AccountType.Basic,
                DeletedDateTime = null
            };

            // Act
            var actionResult = await _controller.Post(_mapper.Map<BankAccountDto>(newUser));
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            var createdUser = Assert.IsType<BankAccountDto>(_mapper.Map<BankAccountDto>(okResult.Value));

            // Assert
            Assert.NotNull(actionResult);
            Assert.Equal(newUser.AccountNumber, createdUser.AccountNumber);
            Assert.Equal(newUser.TotalMoney, createdUser.TotalMoney);
            Assert.Equal(newUser.Type, createdUser.Type);
            Assert.Equal(newUser.DeletedDateTime, createdUser.DeletedDateTime);
            var getUserActionResult = await _controller.GetUserInfoAsync(createdUser.AccountNumber);
            var getUserOkResult = Assert.IsType<OkObjectResult>(getUserActionResult);
            var retrievedUser = Assert.IsType<BankAccountDto>(_mapper.Map<BankAccountDto>(getUserOkResult.Value));
            Assert.Equal(createdUser.AccountNumber, retrievedUser.AccountNumber);
            Assert.Equal(createdUser.TotalMoney, retrievedUser.TotalMoney);
            Assert.Equal(createdUser.Type, retrievedUser.Type);
            Assert.Equal(createdUser.DeletedDateTime, retrievedUser.DeletedDateTime);
        }

        [Fact]
        public async Task CreateUser_BankAccountAlreadyExists_ReturnsBadRequest()
        {
            await Fixture.ResetDatabaseAsync();
            // Arrange
            var existingUser = new BankAccountDto
            {
                AccountNumber = 1,
                TotalMoney = 2000,
                Type = AccountType.Gold,
                DeletedDateTime = null
            };

            // Act
            var actionResult = await _controller.Post(_mapper.Map<BankAccountDto>(existingUser));
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult);
            var errorMessage = Assert.IsType<string>(badRequestResult.Value);

            // Assert
            Assert.Equal("Bank account already exists", errorMessage);
        }

        [Fact]
        public async Task SaveMoney_SuccessfullySavesMoney()
        {
            await Fixture.ResetDatabaseAsync();
            // Arrange
            int accountId = 1;
            double amount = 1000;
            double expectedTotalMoney = 3000;

            // Act
            var actionResult = await _controller.SaveMoneyasync(accountId, amount);
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            var totalMoney = Assert.IsType<double>(okResult.Value);

            // Assert
            Assert.Equal(expectedTotalMoney, totalMoney);
        }

        [Fact]
        public async Task SaveMoney_BankAccountNotFound_ReturnsBadRequest()
        {
            await Fixture.ResetDatabaseAsync();
            // Arrange
            int accountId = 100;
            double amount = 1000;

            // Act
            var actionResult = await _controller.SaveMoneyasync(accountId, amount);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult);
            var errorMessage = Assert.IsType<string>(badRequestResult.Value);

            // Assert
            Assert.Equal("An exception has occurred: Bank account not found", errorMessage);
        }

        [Fact]
        public async Task WithdrawMoney_SuccessfullyWithdrawsMoney()
        {
            await Fixture.ResetDatabaseAsync();
            // Arrange
            int accountId = 1;
            double amount = 500;
            double expectedTotalMoney = 1500;

            // Act
            var actionResult = await _controller.WithdrawMoneyasync(accountId, amount);
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            var totalMoney = Assert.IsType<double>(okResult.Value);

            // Assert
            Assert.Equal(expectedTotalMoney, totalMoney);
        }

        [Fact]
        public async Task WithdrawMoney_NotEnoughMoney_ReturnsBadRequest()
        {
            await Fixture.ResetDatabaseAsync();
            // Arrange
            int accountId = 2;
            double amount = 1000;

            // Act
            var actionResult = await _controller.WithdrawMoneyasync(accountId, amount);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult);
            var errorMessage = Assert.IsType<string>(badRequestResult.Value);

            // Assert
            Assert.Equal("An exception has occurred: Bank account not found or Not enough money in the account", errorMessage);
        }

        [Fact]
        public async Task WithdrawMoney_BankAccountNotFound_ReturnsBadRequest()
        {
            await Fixture.ResetDatabaseAsync();
            // Arrange
            int accountId = 100;
            double amount = 1000;

            // Act
            var actionResult = await _controller.WithdrawMoneyasync(accountId, amount);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult);
            var errorMessage = Assert.IsType<string>(badRequestResult.Value);

            // Assert
            Assert.Equal("An exception has occurred: Bank account not found or Not enough money in the account", errorMessage);
        }

        [Fact]
        public async Task GetUserInfoAsync_ReturnsBankAccount()
        {
            await Fixture.ResetDatabaseAsync();
            // Arrange
            int accountId = 1;
            var expectedBankAccount = new BankAccountDto
            {
                AccountNumber = 1,
                TotalMoney = 2000,
                Type = AccountType.Gold,
                DeletedDateTime = null
            };

            // Act
            var actionResult = await _controller.GetUserInfoAsync(accountId);
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            var bankAccount = Assert.IsType<BankAccountDto>(_mapper.Map<BankAccountDto>(okResult.Value));

            // Assert
            Assert.Equal(expectedBankAccount.AccountNumber, bankAccount.AccountNumber);
            Assert.Equal(expectedBankAccount.TotalMoney, bankAccount.TotalMoney);
            Assert.Equal(expectedBankAccount.Type, bankAccount.Type);
            Assert.Equal(expectedBankAccount.DeletedDateTime, bankAccount.DeletedDateTime);
        }

        [Fact]
        public async Task GetUserInfoAsync_BankAccountNotFound_ReturnsBadRequest()
        {
            await Fixture.ResetDatabaseAsync();
            // Arrange
            int accountId = 100;

            // Act
            var actionResult = await _controller.GetUserInfoAsync(accountId);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult);
            var errorMessage = Assert.IsType<string>(badRequestResult.Value);

            // Assert
            Assert.Equal("An exception has occurred: Bank account not found", errorMessage);
        }

        [Fact]
        public async Task GetUserAllInfoAsync_ReturnsListOfBankAccounts()
        {
            await Fixture.ResetDatabaseAsync();
            // Arrange
            var expectedBankAccounts = new List<BankAccountDto>
            {
                new BankAccountDto
                {
                    AccountNumber = 1,
                    TotalMoney = 2000,
                    Type = AccountType.Gold,
                    DeletedDateTime = null
                },
                new BankAccountDto
                {
                    AccountNumber = 2,
                    TotalMoney = 900,
                    Type = AccountType.Basic,
                    DeletedDateTime = null
                },
                new BankAccountDto
                {
                    AccountNumber = 3,
                    TotalMoney = 300000,
                    Type = AccountType.Gold,
                    DeletedDateTime = null
                },
                new BankAccountDto
                {
                    AccountNumber = 4,
                    TotalMoney = 1000,
                    Type = AccountType.Basic,
                    DeletedDateTime = null
                }
            };

            // Act
            var actionResult = await _controller.GetUserAllInfoAsync();
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            var bankAccounts = Assert.IsType<List<BankAccountDto>>(_mapper.Map<List<BankAccountDto>>(okResult.Value));

            // Assert
            Assert.Equal(expectedBankAccounts.Count, bankAccounts.Count);
            for (int i = 0; i < expectedBankAccounts.Count; i++)
            {
                Assert.Equal(expectedBankAccounts[i].AccountNumber, bankAccounts[i].AccountNumber);
                Assert.Equal(expectedBankAccounts[i].TotalMoney, bankAccounts[i].TotalMoney);
                Assert.Equal(expectedBankAccounts[i].Type, bankAccounts[i].Type);
                Assert.Equal(expectedBankAccounts[i].DeletedDateTime, bankAccounts[i].DeletedDateTime);
            }
        }

        [Fact]
        public async Task GetDeletedAccAsync_ReturnsListOfDeletedBankAccounts()
        {
            await Fixture.ResetDatabaseAsync();
            // Arrange
            var expectedBankAccounts = new List<BankAccountDto>
            {
                new BankAccountDto
                {
                    AccountNumber = 6,
                    TotalMoney = 500,
                    Type = AccountType.Basic,
                    DeletedDateTime = DateTime.UtcNow
                }
            };

            // Act
            var actionResult = await _controller.GetDeletedAccAsync();
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            var bankAccounts = Assert.IsType<List<BankAccountDto>>(_mapper.Map<List<BankAccountDto>>(okResult.Value));

            // Assert
            Assert.Equal(expectedBankAccounts.Count, bankAccounts.Count);
            for (int i = 0; i < expectedBankAccounts.Count; i++)
            {
                Assert.Equal(expectedBankAccounts[i].AccountNumber, bankAccounts[i].AccountNumber);
                Assert.Equal(expectedBankAccounts[i].TotalMoney, bankAccounts[i].TotalMoney);
                Assert.Equal(expectedBankAccounts[i].Type, bankAccounts[i].Type);
                Assert.Equal(expectedBankAccounts[i].DeletedDateTime, bankAccounts[i].DeletedDateTime);
            }
        }

        [Fact]
        public async Task GetTotalMoneyAsync_ReturnsTotalMoney()
        {
            await Fixture.ResetDatabaseAsync();
            // Arrange
            double expectedTotalMoney = 303900;

            // Act
            var actionResult = await _controller.GetTotalMoneyAsync();
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            var totalMoney = Assert.IsType<double>(okResult.Value);

            // Assert
            Assert.Equal(expectedTotalMoney, totalMoney);
        }

        [Fact]
        public async Task GetNumAccAsync_ReturnsNumberOfBankAccounts()
        {
            await Fixture.ResetDatabaseAsync();
            // Arrange
            int expectedNumBankAccounts = 4;

            // Act
            var actionResult = await _controller.GetNumAccAsync();
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            var numBankAccounts = Assert.IsType<int>(okResult.Value);

            // Assert
            Assert.Equal(expectedNumBankAccounts, numBankAccounts);
        }

        [Fact]
        public async Task GetAllTransactions_ReturnsListOfTransactions()
        {
            await Fixture.ResetDatabaseAsync();

            // Arrange
            var expectedTransactions = new List<TransactionDto>
            {
                new TransactionDto
                {
                    TransactionIdentification = Guid.NewGuid(),
                    TransactionDate = DateTime.UtcNow.Date,
                    Amount = 1000,
                    Type = TransactionType.Deposit,
                    AccountNumber = 1
                },
                new TransactionDto
                {
                    TransactionIdentification = Guid.NewGuid(),
                    TransactionDate = DateTime.UtcNow.Date,
                    Amount = 500,
                    Type = TransactionType.Withdraw,
                    AccountNumber = 1
                }
            };

            // Act
            var actionResult = await _controller.GetAllTransactions();
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            var transactions = Assert.IsType<List<TransactionDto>>(_mapper.Map<List<TransactionDto>>(okResult.Value));

            // Assert
            Assert.Equal(expectedTransactions.Count, transactions.Count);
            for (int i = 0; i < expectedTransactions.Count; i++)
            {
                Assert.Equal(expectedTransactions[i].TransactionDate, transactions[i].TransactionDate);
                Assert.Equal(expectedTransactions[i].Amount, transactions[i].Amount);
                Assert.Equal(expectedTransactions[i].Type, transactions[i].Type);
                Assert.Equal(expectedTransactions[i].AccountNumber, transactions[i].AccountNumber);
            }
        }

        [Fact]
        public async Task RemoveAccAsync_RemovesBankAccount()
        {
            await Fixture.ResetDatabaseAsync();
            // Arrange
            int accountId = 2;

            // Act
            var actionResult = await _controller.RemoveAccAsync(accountId);
            var okResult = Assert.IsType<OkResult>(actionResult);

            // Assert
            var getBankAccountActionResult = await _controller.GetUserInfoAsync(accountId);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(getBankAccountActionResult);
            var errorMessage = Assert.IsType<string>(badRequestResult.Value);
            Assert.Equal("An exception has occurred: Bank account not found", errorMessage);
        }

        [Fact]
        public async Task RemoveAccAsync_BankAccountNotFound_ReturnsBadRequest()
        {
            await Fixture.ResetDatabaseAsync();
            // Arrange
            int accountId = 100;

            // Act
            var actionResult = await _controller.RemoveAccAsync(accountId);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult);
            var errorMessage = Assert.IsType<string>(badRequestResult.Value);

            // Assert
            Assert.Equal("An exception has occurred: Bank account not found or was already deleted", errorMessage);
        }
    }
}
