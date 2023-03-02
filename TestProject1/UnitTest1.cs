using System;
using System.Threading.Tasks;
using WebApplication2.Controllers;
using Xunit;

namespace TestProject1
{
    public class ProfileControllerTest
    {
        private readonly InMemoryProfileStore _inMemoryProfileStore = new InMemoryProfileStore();

        private ProfileController _profileController;

        public ProfileControllerTest()
        {
            _profileController = new ProfileController(_inMemoryProfileStore);
        }

        [Fact]
        public async Task AddProfile()
        {
            var profile = new Profile
            {
                Username = "foobar",
                FirstName = "Foo",
                LastName = "Bar"
            };

            await _profileController.AddProfile(profile);
            var storedProfile = await _inMemoryProfileStore.GetProfile(profile.Username);
            Assert.Equal(profile, storedProfile);
        }
    }
}