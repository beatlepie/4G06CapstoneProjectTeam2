using Auth;
using NUnit.Framework;

public class AuthConnectorTest
{
    [Test]
    public void AuthenticationTest()
    {
        Assert.IsNotNull(AuthConnector.Instance.CurrentUser);
        Assert.IsNotNull(AuthConnector.Instance.CurrentUser.Email);
        Assert.IsNotNull(AuthConnector.Instance.IsEmailVerified);
        Assert.IsNotNull(AuthConnector.Instance.Perms);
    }
}