namespace Lwt.Test.Utilities;

using System.Threading.Tasks;
using Lwt.Exceptions;
using Lwt.Models;
using Lwt.Repositories;
using Lwt.Utilities;
using Moq;
using Xunit;

/// <summary>
/// test user text getter.
/// </summary>
public class UserTextGetterTest
{
    private readonly UserTextGetter userTextGetter;
    private readonly Mock<ISqlTextRepository> textRepositoryMock;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserTextGetterTest"/> class.
    /// </summary>
    public UserTextGetterTest()
    {
        this.textRepositoryMock = new Mock<ISqlTextRepository>();
        this.userTextGetter = new UserTextGetter(this.textRepositoryMock.Object);
    }
}