﻿using System;
using System.Linq.Expressions;

namespace SubSonic.Linq.Expressions
{
    public abstract class DbExpression : Expression
    {
        protected DbExpression(DbExpressionType eType, Type type)
            : base()
        {
            NodeType = (ExpressionType)eType;
            Type = type;
        }

        public virtual DbExpression Expression { get; }

        public override ExpressionType NodeType { get; }

        public override Type Type { get; }

        public override string ToString()
        {
            return DbExpressionWriter.WriteToString(this);
        }
    }
}
