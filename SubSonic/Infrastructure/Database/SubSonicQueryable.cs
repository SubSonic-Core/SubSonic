﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SubSonic.Infrastructure
{
    using Builders;
    using Linq.Expressions;
    using Logging;
    using Schema;
    using SubSonic.Interfaces;

    public sealed class SubSonicCollection<TElement>
        : SubSonicCollection
        , ISubSonicCollection<TElement>
    {
        public SubSonicCollection()
            : base(typeof(TElement))
        {

        }

        public SubSonicCollection(IQueryProvider provider, Expression expression)
            : base(typeof(TElement), provider, expression)
        {

        }

        public SubSonicCollection(IQueryProvider provider, Expression expression, IEnumerable<TElement> enumerable)
            : base(typeof(TElement), provider, expression, enumerable)
        {

        }

        IAsyncSubSonicQueryProvider IAsyncSubSonicQueryable<TElement>.AsyncProvider
        {
            get
            {
                if (Provider is IAsyncSubSonicQueryProvider provider)
                {
                    return provider;
                }

                return null;
            }
        }

        #region ICollection<> Implementation
        public void Clear()
        {
            if (TableData is ICollection<TElement> data)
            {
                data.Clear();
            }
            else
            {
                throw new NotSupportedException();
            }
        }
        public void Add(TElement element)
        {
            if (TableData is ICollection<TElement> data)
            {
                if (!IsReadOnly)
                {
                    data.Add(element);
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public void AddRange(IEnumerable<TElement> elements)
        {
            if (!(elements is null))
            {
                foreach (TElement element in elements)
                {
                    Add(element);
                }
            }
        }

        public bool Remove(TElement element)
        {
            if (TableData is ICollection<TElement> data)
            {
                return data.Remove(element);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public bool Contains(TElement element)
        {
            if (TableData is ICollection<TElement> data)
            {
                return data.Contains(element);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public void CopyTo(TElement[] elements, int startAt)
        {
            if (TableData is ICollection<TElement> data)
            {
                data.Select(x => x).ToArray().CopyTo(elements, startAt);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        IEnumerator<TElement> IEnumerable<TElement>.GetEnumerator()
        {
            if (TableData is ICollection<TElement> data)
            {
                if (!IsLoaded)
                {
                    Load();
                }

                return data.GetEnumerator();
            }
            else
            {
                throw new NotSupportedException();
            }
        }
        #endregion

        IAsyncEnumerator<TElement> IAsyncEnumerable<TElement>.GetAsyncEnumerator(System.Threading.CancellationToken cancellationToken)
        {
            throw Error.NotImplemented();
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1010:Collections should implement generic interface", Justification = "Generic Class that inherits from this one addresses the generic interface")]
    public class SubSonicCollection
        : ISubSonicCollection
    {
        protected bool IsLoaded { get; set; }

        public SubSonicCollection(Type elementType)
        {
            ElementType = elementType ?? throw new ArgumentNullException(nameof(elementType));
            TableData = (IEnumerable)Activator.CreateInstance(typeof(HashSet<>).MakeGenericType(elementType));

            if (DbContext.DbModel.TryGetEntityModel(elementType, out IDbEntityModel model))
            {
                Model = model;
                Expression = DbExpression.DbSelect(this, GetType(), Model.Table);
                Provider = new DbSqlQueryBuilder(ElementType, DbContext.ServiceProvider.GetService<ISubSonicLogger>());
            }
            else
            {
                Expression = Expression.Constant(this);
            }
        }
        public SubSonicCollection(Type elementType, IQueryProvider provider, Expression expression)
            : this(elementType)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            Provider = provider ?? new DbSqlQueryBuilder(ElementType, DbContext.ServiceProvider.GetService<ISubSonicLogger>());
        }

        public SubSonicCollection(Type elementType, IQueryProvider provider, Expression expression, IEnumerable elements)
            : this(elementType, provider, expression)
        {
            TableData = (IEnumerable)Activator.CreateInstance(typeof(HashSet<>).MakeGenericType(elementType), elements);
            IsLoaded = true;
        }

        protected IDbEntityModel Model { get; }

        protected IEnumerable TableData { get; private set; }

        public Type ElementType { get; }

        public Expression Expression { get; }

        public IQueryProvider Provider { get; }

        public virtual IQueryable Load()
        {
            if (Provider.Execute(Expression) is IEnumerable elements)
            {
                TableData = (IEnumerable)Activator.CreateInstance(typeof(HashSet<>).MakeGenericType(ElementType), elements);

                IsLoaded = true;
            }

            return this;
        }

        #region ICollection<> Implementation
        public int Count => (int)TableData.GetType().GetProperty(nameof(Count)).GetValue(TableData);
        public bool IsReadOnly => false;
        public IEnumerator GetEnumerator()
        {
            if (!IsLoaded)
            {
                Load();
            }

            return TableData.GetEnumerator();
        }
        #endregion

    }
}
