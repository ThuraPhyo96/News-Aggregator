using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using NewsAggregator.Application.Interfaces;
using NewsAggregator.Application.Services;
using NewsAggregator.FunctionalTests.TestDoubles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NewsAggregator.Domain.Interfaces;
using NewsAggregator.Infrastructure.Repositories;

namespace NewsAggregator.FunctionalTests
{
    public class CustomWebAppFactory : WebApplicationFactory<Program>
    {
        private readonly Action<IServiceCollection>? _configureTestServices;

        public CustomWebAppFactory(Action<IServiceCollection>? configureTestServices = null)
        {
            _configureTestServices = configureTestServices;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                _configureTestServices?.Invoke(services);
            });
        }
    }
}
