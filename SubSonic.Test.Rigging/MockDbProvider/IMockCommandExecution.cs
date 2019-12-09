﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SubSonic.Extensions.Test.MockDbProvider
{
    using Syntax;
    interface IMockCommandExecution
    {
        int ExecuteNonQuery(MockDbCommand cmd);
        object ExecuteScalar(MockDbCommand cmd);
        MockDbDataReaderCollection ExecuteDataReader(MockDbCommand cmd);
    }
}