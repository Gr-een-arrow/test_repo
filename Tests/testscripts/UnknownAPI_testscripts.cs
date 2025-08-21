using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

[TestFixture]
public class AccountSignInTests
{
 [Test]
 public async Task Handler_ShouldReturnUserNotFound_WhenMemberNotInDirectory()
 {
 // Arrange
 var request = new AccountSignInCommand { Email = "testuser", Password = "password", OTP = "123456" };

 // Mock the Initiate method to return an error indicating user not found
 var errorResponse = @"{
 "error": "user_not_found",
 "error_description": "User not found in directory",
 "error_codes": [90001],
 "timestamp": "2025-04-24 10:15:00Z",
 "trace_id": "0000aaaa-11bb-cccc-dd22-eeeeee333333",
 "correlation_id": "aaaa0000-bb11-2222-33cc-444444dddddd"
 }";

 _mockMediator.Setup(m => m.Send(It.IsAny<VerifyMemberStatusCommand>(), new CancellationToken()))
 .ReturnsAsync(new Domain.MemberInfo { MemberStatus = "ACTIVE" });

 _mockEntraService
 .Setup(m => m.Initiate(It.IsAny<List<ChallengeType>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
 .ReturnsAsync(errorResponse);

 // Act
 var result = await _handler.Handle(request, CancellationToken.None);

 // Assert
 Assert.NotNull(result);
 Assert.AreEqual(Status.BusinessException, result.Status);
 Assert.AreEqual("90001", result.Errors.First().Code);
 }
}