using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Conectify.Database;
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
using Conectify.Services.Automatization.Models.Database;

namespace Conectify.Services.Automatization.Test.Services;

public class RuleServiceTests
{
    private readonly ConectifyDb dbContext;

    public RuleServiceTests()
    {
        var contextOptions = new DbContextOptionsBuilder<ConectifyDb>()
            .UseInMemoryDatabase(databaseName: "Test-" + Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        dbContext = new ConectifyDb(contextOptions);
    }
}
