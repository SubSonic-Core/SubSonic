﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure.SqlGenerator
{
    internal class SqlContext<TSqlFragment, TSqlMethods>
        : ISqlContext
        where TSqlFragment : class, ISqlFragment, new()
        where TSqlMethods : class, ISqlMethods, new()
    {
        private ISqlFragment sqlFragment;
        private ISqlMethods sqlMethods;
        ISqlFragment ISqlContext.Fragments => sqlFragment ?? (sqlFragment = new TSqlFragment());

        ISqlMethods ISqlContext.Methods => sqlMethods ?? (sqlMethods = new TSqlMethods());

        public SqlContext()
        {
        }
    }
}
