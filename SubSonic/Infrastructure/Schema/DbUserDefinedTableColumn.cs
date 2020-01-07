﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Reflection;
using System.Text;

namespace SubSonic.Infrastructure.Schema
{
    public class DbUserDefinedTableColumn
    {
        public DbUserDefinedTableColumn(PropertyInfo info)
        {
            if (info is null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            DbUserDefinedTableTypeColumnAttribute TableTypeColumn = null;

            Property = info;
            Name = TableTypeColumn.IsNotNull(x => x.Name) ?? Property.Name;
            Order = TableTypeColumn.IsNotNull(x => x.Order, -1);
            IsNullable = TableTypeColumn.IsNotNull(x => x.IsNullable, Property.PropertyType.IsNullableType());
            DbType = TableTypeColumn.IsNotNull(x => x.DbType, Property.PropertyType.GetDbType());
            IsPrimaryKey = !(info.GetCustomAttribute<KeyAttribute>() is null);
        }

        public DbUserDefinedTableColumn(PropertyInfo info, IDbEntityProperty property, bool disableKeys = false)
        {
            if (property is null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            if (info is null)
            {
                PropertyType = property.PropertyType;
            }
            else
            {
                Property = info;
            }

            Name = property.Name;
            Order = property.Order;
            IsNullable = property.IsNullable;
            DbType = property.DbType;

            if (!disableKeys)
            {
                IsPrimaryKey = property.IsPrimaryKey;
            }
        }

        public PropertyInfo Property { get; }

        private Type _propertyType;

        public Type PropertyType
        {
            get => Property?.PropertyType ?? _propertyType;
            set => _propertyType = value;
        }

        public string Name { get; }
        public int Order { get; set; }
        public bool IsNullable { get; }
        public DbType DbType { get; }
        public bool IsPrimaryKey { get; }
    }
}
