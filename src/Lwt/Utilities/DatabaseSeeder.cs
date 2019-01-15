namespace Lwt.Utilities
{
    using System;
    using System.Threading.Tasks;
    using Lwt.DbContexts;
    using Lwt.Interfaces;
    using Lwt.Interfaces.Services;
    using Lwt.Models;

    /// <inheritdoc />
    public class DatabaseSeeder : IDatabaseSeeder
    {
        private readonly IUserRepository userRepository;

        private readonly IdentityDbContext lwtDbContext;
        private readonly ITextService textService;
        private readonly ITermService termService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseSeeder"/> class.
        /// </summary>
        /// <param name="userRepository"> user repo.</param>
        /// <param name="lwtDbContext">the db context.</param>
        /// <param name="textService">the text service.</param>
        /// <param name="termService">the term service.</param>
        public DatabaseSeeder(
            IUserRepository userRepository,
            IdentityDbContext lwtDbContext,
            ITextService textService,
            ITermService termService)
        {
            this.userRepository = userRepository;
            this.lwtDbContext = lwtDbContext;
            this.textService = textService;
            this.termService = termService;
        }

        /// <inheritdoc />
        public async Task SeedData()
        {
            bool notSeeded = this.lwtDbContext.Database.EnsureCreated();

            if (!notSeeded)
            {
                return;
            }

            User hai = await this.userRepository.GetByUserNameAsync("hai");

            if (hai == null)
            {
                hai = new User { Id = new Guid("9E18BB68-66D2-4711-A27B-1A54AC2E8077"), UserName = "hai" };
                await this.userRepository.CreateAsync(hai, "q");
            }

            var text = new Text
            {
                Title = @"Why do people never get rich by working as an employee?",
                Content = @"First things first, what is “rich” and what is “poor”?

There are 2 types of people all around the world, if you divide them by financially: The richests and the remainings (poors).

If you are not one of the shareholders or owners of the FED, BoE or many other biggest central banks (Central banks, in other words the owners of central banks can print money), congratulations!!! You are in the group of the “remainings”. In other words, if you are trying to gain money by producing products and services (even illegal ways) and you are paying taxes (especially income tax), you are not rich, even Bill Gates and his rich fellows. It does not matter if you are an employee or an employer. You are just a “source of money” or “a component of a big tax farm”, that’s all.

Every individuals, using financial system in this world are working for the richests. There are very few of them (maybe 1 over 1 billion) and the financial system is completely in their hands. Every financial movement (using printed money, banking money or digital transactions) is making them richer more and more.

Furthermore; in short, I’ll try to explain a few ways how we are empowering the richests as a “remaining”.

1- Using credit cards: Using credit cards make poor us all, even not only the users, but also everybody in the group of “remainings”. Because there is no money at any stage of the shopping, just transactions of numbers. Whenever we use credit cards, we are producing virtual (digital) money. Paper money is already worthless, this type of transaction is completely unrequited transfer. Using credit cards increases inflation, decreasing the value of money and make us all poor.

2- Using credits: This is a very long, complicated issue and it is very tough to explain especially for me because of the language barrier. Instead, just look around and observe people who are using credits to buy house, cars, etc.

3- Banking money to a bank for interest or any other reason, moreover witholding money as “money”.

4- Buying fancy and unnecessary things, eating out frequently, expensive vacations. Expensing more money means expensing more money and paying more taxes… It’s a chain.

5- Buying things, those have relatively high tax ratio and need other expenses (for example: high cc cars generally more expensive than their low cc versions, you need to pay more taxes to buy and use one of them and they consupt more fuel; high fuel consumption means more+more taxes).

6- Also, paying for compulsory traffic insurance and automobile insurance, life insurance and any other types of incurance mean that the same.

I can add many other ways how we are adding power to their power but they are not the original subject of the question. Your daily routine make them more and more stronger.

If you think that in a macro plan (wars, international conflicts, terrorist organisations, inflations, deflations, economic crisis, depressions, etc.), you can see the power of the richests and you can see how they are using all humanity to empower them. It is very difficult to explain in a few Quora lines because the plan is 5,000 years old at least and it is continuing.

The long and the short of it; it is meaningless to talk about who is poor or who is rich. The exact question must be “what did we do wrong?”",
                Language = Language.English,
                CreatorId = hai.Id,
                Id = default,
            };
            await this.textService.CreateAsync(text);

            string[] knowedTerms = new[]
            {
                "the", "of", "to", "and", "a", "in", "is", "it", "you", "that", "he", "was", "for", "on", "are",
                "with", "as", "I", "his", "they", "be", "at", "one", "have", "this", "from", "or", "had", "by",
                "hot", "word", "but", "what", "some", "we", "can", "out", "other", "were", "all", "there", "when",
                "up", "use", "your", "how", "said", "an", "each", "she", "which", "do", "their", "time", "if",
                "will", "way", "about", "many", "then", "them", "write", "would", "like", "so", "these", "her",
                "long", "make", "thing", "see", "him", "two", "has", "look", "more", "day", "could", "go", "come",
                "did", "number", "sound", "no", "most", "people", "my", "over", "know", "water", "than", "call",
                "first", "who", "may", "down", "side", "been", "now", "find", "any", "new", "work", "part", "take",
                "get", "place", "made", "live", "where", "after", "back", "little", "only", "round", "man", "year",
                "came", "show", "every", "good", "me", "give", "our", "under", "name", "very", "through", "just",
                "form", "sentence", "great", "think", "say", "help", "low", "line", "differ", "turn", "cause",
                "much", "mean", "before", "move", "right", "boy", "old", "too", "same", "tell", "does", "set",
                "three", "want", "air", "well", "also", "play", "small", "end", "put", "home", "read", "hand",
                "port", "large", "spell", "add", "even", "land", "here", "must", "big", "high", "such", "follow",
                "act", "why", "ask", "men", "change", "went", "light", "kind", "off", "need", "house", "picture",
                "try", "us", "again", "animal", "point", "mother", "world", "near", "build", "self", "earth",
                "father",
            };

            foreach (string knownTerm in knowedTerms)
            {
                await this.termService.CreateAsync(new Term()
                {
                    Content = knownTerm.ToUpperInvariant(),
                    CreatorId = hai.Id,
                    Id = default,
                    Language = Language.English,
                    LearningLevel = TermLearningLevel.WellKnow,
                    Meaning = null,
                });
            }
        }
    }
}