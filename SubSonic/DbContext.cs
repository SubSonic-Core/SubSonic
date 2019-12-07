﻿using SubSonic.Infrastructure;
using SubSonic.Infrastructure.Providers;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("SubSonic.Tests", AllInternalsVisible = true)]

namespace SubSonic
{
    public class DbContext
        : IDisposable, IInfrastructure<IServiceProvider>
    {
        protected DbContext()
        {
            OnDbConfiguring(new DbContextOptionsBuilder(this, Options = new DbContextOptions()));
            OnDbModeling(new DbModelBuilder(Model = new DbModel(this)));
        }

        public DbContextOptions Options { get; }

        public DbModel Model { get; }

        public DbSet<TEntity> Set<TEntity>()
            where TEntity : class
        {
            return new DbSet<TEntity>(new SubSonicDbSetProvider<TEntity>(this, new Infrastructure.Logging.SubSonicLogger<DbSet<TEntity>>(null)));
        }

        protected virtual void OnDbConfiguring(DbContextOptionsBuilder builder)
        {

        }

        protected virtual void OnDbModeling(DbModelBuilder builder)
        {

        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~DbContext()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

        public IServiceProvider Instance { get; internal set; }
    }
}
