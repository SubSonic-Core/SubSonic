﻿using System;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    using Structure;

    public abstract partial class DbExpression : Expression
    {
        protected DbExpression(DbExpressionType eType, Type type)
            : base()
        {
            NodeType = (ExpressionType)eType;
            Type = type;
        }

        public override ExpressionType NodeType { get; }

        public override Type Type { get; }

        public override string ToString()
        {
            return DbExpressionWriter.WriteToString(this);
        }
    }
}
