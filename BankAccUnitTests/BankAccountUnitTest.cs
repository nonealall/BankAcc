namespace BankAccountUnitTest
{
    using AutoFixture;
using Xunit;
using Moq;
    using AutoFixture.AutoMoq;

    public class BankAccountServiceTest
    {
        private IBankAccountService _bankAccountService;
        private Mock<IBankAccountRepository> _mockBankRepository;
        private Mock<IBankManagementAccountService> _mockBankManagementAccountService;
        private Fixture _fixture;

        public BankAccountServiceTest()
        {
            // fixture for creating test data
            _fixture = new Fixture();
            _fixture.Customize(new AutoMoqCustomization());
            _fixture.Behaviors.ToList()
    .OfType<ThrowingRecursionBehavior>()
    .ToList()
    .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());


            // mock user repo dependency
            _mockBankRepository = new Mock<IBankAccountRepository>();
            _mockBankManagementAccountService = new Mock<IBankManagementAccountService>();

            // service under test
            _bankAccountService = new BankAccountService(_mockBankRepository.Object, _mockBankManagementAccountService.Object);
        }

        [Fact]
        public async void GetInfoFromUserWhenValidIdProvided()
        {
            // Arrange
            var userFixture = _fixture.Create<BankAccountDto>();
            var id = userFixture.AccountNumber;
            _mockBankRepository.Setup(x => x.GetBankAccount(id)).ReturnsAsync(userFixture);

            // Act
            BankAccountDto user = await _bankAccountService.GetBankAccountAsync(id);

            // Assert
            Assert.NotNull(user);
            Assert.Equal(userFixture.AccountNumber, user.AccountNumber);
            Assert.Equal(userFixture.TotalMoney, user.TotalMoney);
            Assert.Equal(userFixture.Type, user.Type);
            Assert.Equal(userFixture.DeletedDateTime, user.DeletedDateTime);
            Assert.Equal(userFixture.Transactions, user.Transactions);
            _mockBankRepository.Verify(mock => mock.GetBankAccount(id), Times.Once);

        }

        [Fact]
        public async Task GetInfoFromUserWhenInvalidId_ThrowsException()
        {
            BankAccountDto? userFixture = null;
            _mockBankRepository.Setup(x => x.GetBankAccount(It.IsAny<int>())).ReturnsAsync(userFixture);
            await Assert.ThrowsAsync<Exception>(() => _bankAccountService.GetBankAccountAsync(100));
        }

        [Theory]
        [InlineData (100, AccountType.Basic)]
        [InlineData (999, AccountType.Basic)]
        [InlineData (3000, AccountType.Gold)]
        public async Task GetDepositFromUserWhenValidIDProvided(double amount, AccountType type)
        {
            // Arrange
            BankAccountDto userFixture = _fixture.Create<BankAccountDto>();
            int id = userFixture.AccountNumber;
            double expected = userFixture.TotalMoney+amount;
            _mockBankRepository.Setup(x => x.GetBankAccount(id)).ReturnsAsync(userFixture);  
            // _mockBankRepository.Setup(x => x.UpdateBankAccountAsync(userFixture, id)).ReturnsAsync(userFixture); 
            _mockBankManagementAccountService.Setup(x => x.FireNotificationToBankManagementAsync(id, expected)).Returns(Task.CompletedTask);
            // Act    
            double balanceAmount = await _bankAccountService.Deposit(id, amount);
            // Assert
            Assert.Equal(expected, balanceAmount);
            _mockBankManagementAccountService.Verify(mock => mock.FireNotificationToBankManagementAsync(id, balanceAmount), Times.Once);
        }

        [Theory]
        [InlineData(100, AccountType.Basic, 10)]
        [InlineData(80, AccountType.Basic, 10)]
        [InlineData(2000, AccountType.Gold, 1999)]
        [InlineData(2000, AccountType.Gold, 100)]
        public async Task GetWithdrawFromUserWhenValidAmountProvidedForGold(double amount, AccountType type, double withdrawAmount)
        {
            // Arrange
            var userFixture = _fixture.Create<BankAccountDto>();
            userFixture.Type = type;
            userFixture.TotalMoney = amount;
            double GetTaxRate(BankAccountDto account) => account.Type == AccountType.Gold ? 0 : 0.1;
            var id = userFixture.AccountNumber;
            double expected = userFixture.TotalMoney - withdrawAmount - (withdrawAmount * GetTaxRate(userFixture));

            _mockBankRepository.Setup(x => x.GetBankAccount(id)).ReturnsAsync(userFixture);
            _mockBankManagementAccountService.Setup(x => x.FireNotificationToBankManagementAsync(id, expected)).Returns(Task.CompletedTask);


            // Act    
            double remainingBalance = await _bankAccountService.Withdraw(id, withdrawAmount);

            // Assert
            Assert.Equal(expected, remainingBalance);
            _mockBankManagementAccountService.Verify(mock => mock.FireNotificationToBankManagementAsync(id, remainingBalance), Times.Once);
        }

        [Theory]
        [InlineData ( 100, AccountType.Basic, 95)]
        [InlineData (80, AccountType.Basic, 79)]
        [InlineData (3000, AccountType.Gold, 3001)]
        public async Task GetWithdrawFromUserWhenInvalidId_ThrowsException(double amount, AccountType type, double withdrawAmount)
        {
            var userFixture = _fixture.Create<BankAccountDto>();
            _mockBankRepository.Setup(x => x.GetBankAccount(It.IsAny<int>())).ReturnsAsync(userFixture);
            userFixture.TotalMoney = amount;
            await Assert.ThrowsAsync<Exception>(() => _bankAccountService.Withdraw(10, withdrawAmount ));
        }
    }
}