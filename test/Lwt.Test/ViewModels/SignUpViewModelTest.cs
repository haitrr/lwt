using Lwt.ViewModels.User;
using Xunit;
using System.ComponentModel.DataAnnotations;

namespace Lwt.Test.ViewModels
{
    public class SignUpViewModelTest
    {
        [Theory]
        [InlineData("username", "password")]
        [InlineData("t", "p")]
        [InlineData("thisIsAReallyLongUserName", "password")]
        [InlineData("thisIsAReallyLongUserName", "thisIsAReallyLongPassword")]
        public void ShouldValid(string userName, string password)
        {
            // arrange
            var viewModel = new SignUpViewModel()
            {
                Password = userName,
                UserName = password
            };
            var validationContext = new ValidationContext(viewModel);

            //act
            bool actual = Validator.TryValidateObject(viewModel, validationContext, null);

            // assert
            Assert.True(actual);
        }


        [Theory]
        [InlineData("", "aPassword")]
        [InlineData("aUser", "")]
        [InlineData("thisIsAUser", null)]
        [InlineData(null, null)]
        [InlineData(null, "p@ssW0rd")]
        public void ShouldNotValid(string userName, string password)
        {
            // arrange
            var viewModel = new SignUpViewModel()
            {
                Password = userName,
                UserName = password
            };
            var validationContext = new ValidationContext(viewModel);

            //act
            bool actual = Validator.TryValidateObject(viewModel, validationContext, null);

            // assert
            Assert.False(actual);
        }

    }
}
