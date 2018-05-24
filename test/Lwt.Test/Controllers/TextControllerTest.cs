using System.Threading.Tasks;
using Lwt.Controllers;
using Lwt.DbContexts;
using Lwt.Interfaces;
using Lwt.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Lwt.Test.Controllers
{
    public class TextControllerTest
    {
        private readonly TextController _textController;
        private readonly Mock<ITextService> _textService;

        public TextControllerTest()
        {
            _textService = new Mock<ITextService>();
            _textController = new TextController(_textService.Object);
        }

        #region Constructor

        [Fact]
        public void Constructor_ShouldWork()
        {
            // arrange
            ServiceProvider efServiceProvider = new ServiceCollection().AddEntityFrameworkInMemoryDatabase().BuildServiceProvider();

            var services = new ServiceCollection();

            services.AddDbContext<LwtDbContext>(b => b.UseInMemoryDatabase("Lwt").UseInternalServiceProvider(efServiceProvider));
            var configuration = new Mock<IConfiguration>();
            var startup = new Startup(configuration.Object);
            startup.ConfigureServices(services);
            services.AddTransient<TextController>();
            ServiceProvider serviceProvider = services.BuildServiceProvider();

            // act
            var instance = serviceProvider.GetService<TextController>();

            // assert
            Assert.NotNull(instance);
        }

        #endregion

        #region Create

        [Fact]
        public async Task CreateAsync_ShouldCallService()
        {
            // arrange
            _textService.Reset();

            // act
            await _textController.CreateAsync();

            // assert
            _textService.Verify(s => s.CreateAsync(), Times.Once);

        }

        [Fact]
        public void CreateAsync_ShouldReturnOk_IfSuccess()
        {
            // arrange


            //

        }

        #endregion


    }
}
