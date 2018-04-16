using Lwt.ViewModels.User;
using Xunit;
using System.ComponentModel.DataAnnotations;

namespace Lwt.Test.ViewModels
{
    public class SignUpViewModelTest
    {
        [Theory]
        [InlineData("asjdl","asjdlk",true)]
        [InlineData("","asjdlk",false)]
        [InlineData("asjdl","",false)]
        [InlineData("asjdl",null,false)]
        [InlineData(null,null,false)]
        [InlineData(null,"hello",false)]
        public void ShouldValid(string userName,string password,bool isValid)
        {
            // arrange
            SignUpViewModel viewModel = new SignUpViewModel()
            {
                Password = userName,
                UserName = password
            };
            var validationContext = new ValidationContext(viewModel);

            // assert
            Assert.Equal(isValid,Validator.TryValidateObject(viewModel,validationContext,null));
        }

      
    }
}
