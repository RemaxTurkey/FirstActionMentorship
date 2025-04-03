using Xunit;
using Application.Services.Mail;
using System.Linq;

namespace Application.Tests.Services.Mail
{
    public class FAMAcceptanceNotificationTests
    {
        [Fact]
        public void GetBrokerEmailsSql_WithSingleEmployeeId_ShouldGenerateCorrectSql()
        {
            // Arrange
            int[] employeeIds = new[] { 1 };
            string expectedPart = "EmployeeId IN (1)";

            // Act
            var result = FAMAcceptanceNotification.GetBrokerEmailsSql(employeeIds);

            // Assert
            Assert.Contains(expectedPart, result);
        }

        [Fact]
        public void GetBrokerEmailsSql_WithMultipleEmployeeIds_ShouldGenerateCorrectSql()
        {
            // Arrange
            int[] employeeIds = new[] { 1, 2, 3 };
            string expectedPart = "EmployeeId IN (1,2,3)";

            // Act
            var result = FAMAcceptanceNotification.GetBrokerEmailsSql(employeeIds);

            // Assert
            Assert.Contains(expectedPart, result);
        }
    }
}