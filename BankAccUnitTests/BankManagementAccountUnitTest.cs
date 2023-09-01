
namespace BankManagementAccountUnitTest
{
    using AutoFixture;
using Xunit;
using Moq;
    using BankAcc.Dtos;

    public class BankAccountManagementServiceTest
    {
        private IBankManagementAccountService _bankAccountService;
        private Mock<IBankAccountRepository> _mockBankRepository;
        private Fixture _fixture;

        public BankAccountManagementServiceTest()
        {
            // fixture for creating test data
            _fixture = new Fixture();
            _fixture.Behaviors.ToList()
    .OfType<ThrowingRecursionBehavior>()
    .ToList()
    .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());


            // mock user repo dependency
            _mockBankRepository = new Mock<IBankAccountRepository>();

            // service under test
            _bankAccountService = new BankManagementAccountService(_mockBankRepository.Object);        
        }

        [Fact]
        public async Task GetInfoFromAllUserReturnsValidResults()
        {
            // Arrange
        List<BankAccountDto> expectedUserList = new List<BankAccountDto>
            {
                new BankAccountDto{
                    AccountNumber = 1,
                    TotalMoney = 2000,
                    Type = AccountType.Gold,
                    DeletedDateTime = null
                },
                new BankAccountDto{
                    AccountNumber = 2,
                    TotalMoney = 9000,
                    Type = AccountType.Gold,
                    DeletedDateTime = null
                },
                new BankAccountDto{
                    AccountNumber = 3,
                    TotalMoney = 100,
                    Type = AccountType.Basic,
                    DeletedDateTime = null
                }
            };
            _mockBankRepository.Setup(x => x.GetAllBankAccounts()).ReturnsAsync(expectedUserList);

            // Act
            var user = await _bankAccountService.ShowGoldAndBasicMembersAsync();

            // Assert
            Assert.NotNull(user);
            Assert.True(user.Count() == 3);
            Assert.Equal(expectedUserList, user);
    }

    [Fact]
        public async Task GetDeletedAccountsReturnsValidResults()
        {
            // Arrange
        List<BankAccountDto> expectedUserList = new List<BankAccountDto>
            {
                new BankAccountDto{
                    AccountNumber = 1,
                    TotalMoney = 2000,
                    Type = AccountType.Gold,
                    DeletedDateTime = null
                },
                new BankAccountDto{
                    AccountNumber = 2,
                    TotalMoney = 9000,
                    Type = AccountType.Gold,
                    DeletedDateTime = null
                },
                new BankAccountDto{
                    AccountNumber = 3,
                    TotalMoney = 100,
                    Type = AccountType.Basic,
                    DeletedDateTime = null
                }
            };
            _mockBankRepository.Setup(x => x.GetAllDeletedBankAccounts()).ReturnsAsync(expectedUserList);

            // Act
            var user = await _bankAccountService.GetDeletedBankAccountsAsync();

            // Assert
            Assert.NotNull(user);
            Assert.True(user.Count() == 3);
            Assert.Equal(expectedUserList, user);
        }

        [Fact]
        public async Task GetBankTotalMoneyAsyncReturnsValidResults()
        {
            // Arrange
        List<BankAccountDto> expectedUserList = new List<BankAccountDto>
            {
                new BankAccountDto{
                    AccountNumber = 1,
                    TotalMoney = 2000,
                    Type = AccountType.Gold,
                    DeletedDateTime = null
                },
                new BankAccountDto{
                    AccountNumber = 2,
                    TotalMoney = 9000,
                    Type = AccountType.Gold,
                    DeletedDateTime = null
                },
                new BankAccountDto{
                    AccountNumber = 3,
                    TotalMoney = 100,
                    Type = AccountType.Basic,
                    DeletedDateTime = null
                }
            };
            double expectedTotal = 11100;
            _mockBankRepository.Setup(x => x.GetAllBankAccounts()).ReturnsAsync(expectedUserList);

            // Act
            double user = await _bankAccountService.BankTotalMoneyAsync();

            // Assert
            Assert.NotNull(user);
            Assert.Equal(expectedTotal, user);
        }
        [Fact]
        public async Task GetNumBankAccAsyncReturnsValidResults()
        {
            // Arrange
        List<BankAccountDto> expectedUserList = new List<BankAccountDto>
            {
                new BankAccountDto{
                    AccountNumber = 1,
                    TotalMoney = 2000,
                    Type = AccountType.Gold,
                    DeletedDateTime = null
                },
                new BankAccountDto{
                    AccountNumber = 2,
                    TotalMoney = 9000,
                    Type = AccountType.Gold,
                    DeletedDateTime = null
                },
                new BankAccountDto{
                    AccountNumber = 3,
                    TotalMoney = 100,
                    Type = AccountType.Basic,
                    DeletedDateTime = null
                }
            };
            double expectedTotal = 3;
            _mockBankRepository.Setup(x => x.GetAllBankAccounts()).ReturnsAsync(expectedUserList);

            // Act
            double user = await _bankAccountService.NumBankAccAsync();

            // Assert
            Assert.NotNull(user);
            Assert.Equal(expectedTotal, user);
        }

        [Theory]
        [InlineData(100, AccountType.Basic)]
        [InlineData(10000, AccountType.Gold)]
        public async Task GetFireNotificationToBankManagementAsyncReturnsValidResults(double tAmount, AccountType type)
        {
            // Arrange
            var userFixture = _fixture.Create<BankAccountDto>();
            userFixture.TotalMoney = tAmount;
            userFixture.Type = type;
            var id = userFixture.AccountNumber;
            _mockBankRepository.Setup(x => x.GetBankAccount(id)).ReturnsAsync(userFixture);
            _mockBankRepository.Setup(x => x.UpdateBankAccountAsync(userFixture, id)).ReturnsAsync(userFixture);

            // Act
            await _bankAccountService.FireNotificationToBankManagementAsync(id, tAmount);

            // Assert
            Assert.Equal(type, userFixture.Type);
            _mockBankRepository.Verify(mock => mock.UpdateBankAccountAsync(userFixture, id), Times.Once);
        }


        [Fact]
        public async Task RemoveBankAccountAsyncReturnsValidResults()
        {
            // Arrange
            BankAccountDto removedUser = new BankAccountDto
            {
                    AccountNumber = 1,
                    TotalMoney = 2000,
                    Type = AccountType.Gold,
                    DeletedDateTime = null
            };
            _mockBankRepository.Setup(x => x.GetBankAccount(removedUser.AccountNumber)).ReturnsAsync(removedUser);
            _mockBankRepository.Setup(x => x.DeleteBankAccountAsync(removedUser.AccountNumber)).Returns(Task.CompletedTask);
            
            // Act
            await _bankAccountService.RemoveBankAccountAsync(removedUser.AccountNumber);

            // Assert
             _mockBankRepository.Verify(mock => mock.DeleteBankAccountAsync(removedUser.AccountNumber), Times.Once);
        }

        [Fact]
        public async Task NewBankAccountAsyncReturnsValidResults()
        {
            // Arrange
            var userFixture = _fixture.Create<BankAccountDto>();
            var id = userFixture.AccountNumber;
            BankAccountDto? returnValue = null;
            _mockBankRepository.Setup(x => x.GetBankAccount(id)).ReturnsAsync(returnValue);
            _mockBankRepository.Setup(x => x.InsertBankAccountAsync(userFixture, id)).ReturnsAsync(userFixture);
            // Act
            var user = await _bankAccountService.NewBankAccountAsync(userFixture);

            // Assert
            Assert.NotNull(user);
            Assert.Equal(userFixture.AccountNumber, user.AccountNumber);
            Assert.Equal(userFixture.TotalMoney, user.TotalMoney);
            Assert.Equal(userFixture.Type, user.Type);
            Assert.Equal(userFixture.DeletedDateTime, user.DeletedDateTime);
        }

        [Fact]
        public async Task NewBankAccountAsync_ThrowsException_WhenBankAccExists()
        {
            var userFixture = _fixture.Create<BankAccountDto>();
            var id = userFixture.AccountNumber;
            _mockBankRepository.Setup(x => x.GetBankAccount(id)).ReturnsAsync(userFixture);
            _mockBankRepository.Setup(x => x.InsertBankAccountAsync(userFixture, id)).ReturnsAsync(userFixture);
            // Act
            await Assert.ThrowsAsync<Exception>(() => _bankAccountService.NewBankAccountAsync(userFixture ));
        }


        [Theory]
        [InlineData (100)]
        [InlineData (10000)]
        public async Task FireNotificationToBankManagementAsync_ThrowsException_WhenBankAccDoesNotExists(double tAmount)
        {
            var userFixture = _fixture.Create<BankAccountDto>();
            BankAccountDto? returnValue = null;
            var id = userFixture.AccountNumber;
            _mockBankRepository.Setup(x => x.GetBankAccount(id)).ReturnsAsync(returnValue);
            _mockBankRepository.Setup(x => x.InsertBankAccountAsync(userFixture, id)).ReturnsAsync(userFixture);
            await Assert.ThrowsAsync<Exception>(() => _bankAccountService.FireNotificationToBankManagementAsync(id, tAmount));
        }
        [Fact]
        public async Task RemoveBankAccountAsync_ThrowsException_WhenBankAccDoesNotExists()
        {
            // Arrange
            var userFixture = _fixture.Create<BankAccountDto>();
            BankAccountDto? returnValue = null;
            var id = userFixture.AccountNumber;
            _mockBankRepository.Setup(x => x.GetBankAccount(id)).ReturnsAsync(returnValue);
            _mockBankRepository.Setup(x => x.DeleteBankAccountAsync(id)).Returns(Task.CompletedTask);
            await Assert.ThrowsAsync<Exception>(() => _bankAccountService.RemoveBankAccountAsync(id));
        }

        [Fact]
        public async Task RemoveBankAccountAsync_ThrowsException_WhenDeletedDateTimeNotNull()
        {
            // Arrange
            BankAccountDto removedUser = new BankAccountDto
            {
                AccountNumber = 1,
                TotalMoney = 2000,
                Type = AccountType.Gold,
                DeletedDateTime = DateTime.Now
        };
            _mockBankRepository.Setup(x => x.GetBankAccount(removedUser.AccountNumber)).ReturnsAsync(removedUser);
            _mockBankRepository.Setup(x => x.DeleteBankAccountAsync(removedUser.AccountNumber)).Returns(Task.CompletedTask);
            await Assert.ThrowsAsync<Exception>(() => _bankAccountService.RemoveBankAccountAsync(removedUser.AccountNumber));
        }
        public class BankManagementAccountServiceTest
        {
            private IBankManagementAccountService _bankAccountService;
            private Mock<IBankAccountRepository> _mockBankRepository;
            private Fixture _fixture;

            public BankManagementAccountServiceTest()
            {
                _fixture = new Fixture();

                _mockBankRepository = new Mock<IBankAccountRepository>();

                _bankAccountService = new BankManagementAccountService(_mockBankRepository.Object);
            }

            [Fact]
            public async Task GetAllTransactionsAsync_ReturnsValidResults()
            {
                // Arrange
                List<TransactionDto> expectedTransactionList = new List<TransactionDto>
        {
            new TransactionDto
            {
                TransactionId = 1,
                Amount = 100,
                Type = TransactionType.Deposit,
                AccountNumber = 1,
                BankAccount = new BankAccounts()
            },
            new TransactionDto
            {
                TransactionId = 2,
                Amount = 200,
                Type = TransactionType.Withdraw,
                AccountNumber = 2,
                BankAccount = new BankAccounts()
            },
            new TransactionDto
            {
                TransactionId = 3,
                Amount = 300,
                Type = TransactionType.Deposit,
                AccountNumber = 1,
                BankAccount = new BankAccounts()
            }
        };
                _mockBankRepository.Setup(x => x.GetTransactionReportForAllUsers()).ReturnsAsync(expectedTransactionList);

                // Act
                var transactions = await _bankAccountService.GetAllTransactionsAsync();

                // Assert
                Assert.NotNull(transactions);
                Assert.True(transactions.Count() == 3);
                Assert.Equal(expectedTransactionList, transactions);
            }
        }

    }
}