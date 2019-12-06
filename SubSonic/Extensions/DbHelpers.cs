﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SubSonic
{
    public static partial class Extensions
    {
        public static string[] GetForeignKeyName(this PropertyInfo propertyInfo)
        {
            string[] result = propertyInfo
                                .GetCustomAttributes<ForeignKeyAttribute>()
                                .Select(attribute => attribute.Name)
                                .ToArray();

            return result.Length == 0 ? new[] { $"{propertyInfo.Name}ID" } : result;
        }

        public static string[] GetPrimaryKeyName<TEntity>()
            where TEntity : class
        {
            string[] result = typeof(TEntity)
                                .GetProperties()
                                .Where(property => property.GetCustomAttribute<KeyAttribute>().IsNotNull())
                                .Select(property => property.Name)
                                .ToArray();

            return result.Length == 0 ? new[] { $"ID" } : result;
        }

        public static TType GetValue<TType>(this PropertyInfo source, object value, object[] index = null)
        {
            return (TType)source.GetValue(value, index);
        }

    }
}