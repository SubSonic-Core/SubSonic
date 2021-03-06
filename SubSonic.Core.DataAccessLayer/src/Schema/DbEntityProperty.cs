﻿using System;
using System.Collections.Generic;

namespace SubSonic.Schema
{
    using Linq;
    using Linq.Expressions;
    using Linq.Expressions.Alias;
    using System.Data;

    public class DbEntityProperty
        : DbObject
        , IDbEntityProperty
    {
        private readonly IDbEntityModel dbEntityModel;

        public DbEntityProperty(IDbEntityModel dbEntityModel, string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
            {
                throw new ArgumentException("", nameof(columnName));
            }

            this.dbEntityModel = dbEntityModel ?? throw new ArgumentNullException(nameof(dbEntityModel));
            this.Name = columnName;
        }

        public IDbEntityModel EntityModel => dbEntityModel;

        public string PropertyName { get; internal set; }

        public Type PropertyType { get; internal set; }

        public bool IsPrimaryKey { get; internal set; }

        public IEnumerable<string> ForeignKeys { get; internal set; }

        public int Size { get; internal set; }
        public int Scale { get; internal set; }
        public int Precision { get; internal set; }
        public bool IsRequired { get; internal set; }
        public bool IsNullable { get; internal set; }
        public bool IsReadOnly { get; internal set; }
        public bool IsComputed { get; internal set; }
        public bool IsAutoIncrement { get; internal set; }
        public int Order { get; set; }
        public DbType DbType { get; set; }

        public DbEntityPropertyType EntityPropertyType
        {
            get
            {
                DbEntityPropertyType result;
                if (PropertyType.GetUnderlyingType().IsValueType || PropertyType.IsAssignableFrom(typeof(string)))
                {
                    if (PropertyName.IsNotNullOrEmpty())
                    {
                        result = DbEntityPropertyType.Value;
                    }
                    else
                    {
                        result = DbEntityPropertyType.DAL;
                    }
                }
                else if (PropertyType.IsClass)
                {
                    result = DbEntityPropertyType.Navigation;
                }
                else if (PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>) || PropertyType.GetInterface(typeof(IEnumerable<>).Name).IsNotNull())
                {
                    result = DbEntityPropertyType.Collection;
                }
                else
                {
                    throw new NotSupportedException($"Property Type \"{PropertyType.GetTypeName()}\", is not supported.");
                }

                return result;
            }
        }

        public DbColumnExpression Expression { get; private set; }

        internal void SetExpression(TableAlias alias)
        {
            Expression = new DbColumnExpression(PropertyType, alias, Name);
        }

        public override string ToString()
        {
            return PropertyName ?? Name;
        }
    }
}
