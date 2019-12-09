﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace SubSonic.Infrastructure
{
#if X86
    using Factories;
#endif

    public class DbContextOptionsBuilder
    {
        private readonly DbContext dbContext;
        private readonly DbContextOptions options;

        private bool isDirtyServiceProvider = false;

        public DbContextOptionsBuilder(DbContext dbContext, DbContextOptions options)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public IServiceProvider ServiceProvider => dbContext.Instance;

        public void EnableProxyGeneration()
        {
            options.EnableProxyGeneration = true;
        }

        public DbContextOptionsBuilder SetDefaultProviderFactory(string providerInvariantName)
        {
            if (string.IsNullOrEmpty(providerInvariantName))
            {
                throw new ArgumentException("", nameof(providerInvariantName));
            }

            if (!DbProviderFactories.GetProviderInvariantNames().Any(provider => provider.Equals(providerInvariantName, StringComparison.OrdinalIgnoreCase)))
            {
                throw new DbProviderFactoryNotRegisteredException(providerInvariantName);
            }

            options.ProviderInvariantName = providerInvariantName;

            return this;
        }

        public DbContextOptionsBuilder EnableMultipleActiveResultSets(bool enable)
        {
            options.UseMultipleActiveResultSets = enable;

            return this;
        }

        public DbContextOptionsBuilder RegisterProviderFactory(string providerInvariantName, DbProviderFactory factory)
        {
            if (string.IsNullOrEmpty(providerInvariantName))
            {
                throw new ArgumentException("", nameof(providerInvariantName));
            }

            if (factory is null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            DbProviderFactories.RegisterFactory(providerInvariantName, factory);

            return this;
        }

        public DbContextOptionsBuilder RegisterProviderFactory(string providerInvariantName, Type factoryType)
        {
            if (string.IsNullOrEmpty(providerInvariantName))
            {
                throw new ArgumentException("", nameof(providerInvariantName));
            }

            if (factoryType is null)
            {
                throw new ArgumentNullException(nameof(factoryType));
            }

            DbProviderFactories.RegisterFactory(providerInvariantName, factoryType);

            return this;
        }

        public DbContextOptionsBuilder RegisterProviderFactory(string providerInvariantName, string factoryTypeAssembyQualifiedName)
        {
            if (string.IsNullOrEmpty(providerInvariantName))
            {
                throw new ArgumentException("", nameof(providerInvariantName));
            }

            if (string.IsNullOrEmpty(factoryTypeAssembyQualifiedName))
            {
                throw new ArgumentException("", nameof(factoryTypeAssembyQualifiedName));
            }

            DbProviderFactories.RegisterFactory(providerInvariantName, factoryTypeAssembyQualifiedName);

            return this;
        }

        public void SetServiceProvider(IServiceProvider provider)
        {
            if (provider is null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            if (!isDirtyServiceProvider)
            {
                dbContext.Instance = provider;
            }
        }
    }
}
