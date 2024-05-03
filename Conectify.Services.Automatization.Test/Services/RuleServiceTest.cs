using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Conectify.Database;
using Conectify.Database.Models.Automatization;
using Conectify.Services.Automatization.Models.ApiModels;
using Conectify.Services.Automatization.Services;
using Conectify.Services.Library;
using Microsoft.EntityFrameworkCore;
using FakeItEasy;
using AutoMapper.QueryableExtensions;
using Conectify.Shared.Library.Models;
using System.Data.Entity;
using System.Net.WebSockets;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace Conectify.Services.Tests.Automatization.Services
{
    public class RuleServiceTests
    {
        private ConectifyDb dbContext;

        public RuleServiceTests()
        {
            var contextOptions = new DbContextOptionsBuilder<ConectifyDb>()
                .UseInMemoryDatabase(databaseName: "Test-" + Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            dbContext = new ConectifyDb(contextOptions);
        }

        [Fact]
        public async Task ShouldAddNewRule_CorrectRule_ReturnRuleId()
        {
            var ac = A.Fake<IAutomatizationCache>();
            var id = Guid.NewGuid();
            A.CallTo(() => ac.AddNewRule(A<Rule>.Ignored, A<CancellationToken>.Ignored)).Returns(id);

            var createRuleApiModel = new CreateRuleApiModel()
            {
                BehaviourId = Guid.NewGuid(),
                Parameters = "",
                X = 100,
                Y = 100,
            };

            var ruleService = new RuleService(ac, A.Fake<IMapper>(), dbContext, A.Fake<IConnectorService>(), A.Fake<IDeviceData>(), A.Fake<ITimingService>() );

            var result = await ruleService.AddNewRule(createRuleApiModel, CancellationToken.None);

            Assert.Equal(result, id);
        }

        [Theory]
        [InlineData( "{}", "d274c7f0-211e-413a-8689-f2543dbfc818" )]
        public async Task ShouldAddNewRule_SpecificRules_AddExtraParams(string parameters, string behaviourId)
        {
            var ac = A.Fake<IAutomatizationCache>();
            var id = Guid.NewGuid();
            A.CallTo(() => ac.AddNewRule(A<Rule>.Ignored, A<CancellationToken>.Ignored)).Returns(id);

            var createRuleApiModel = new CreateRuleApiModel()
            {
                BehaviourId = Guid.Parse(behaviourId),
                Parameters = parameters,
                X = 100,
                Y = 100,
            };

            var ruleService = new RuleService(ac, A.Fake<IMapper>(), dbContext, A.Fake<IConnectorService>(), A.Fake<IDeviceData>(), A.Fake<ITimingService>());

            var result = await ruleService.AddNewRule(createRuleApiModel, CancellationToken.None);

            Assert.Equal(result, id);
        }
    }


}
