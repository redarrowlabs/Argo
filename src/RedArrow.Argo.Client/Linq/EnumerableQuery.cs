using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace RedArrow.Argo.Client.Linq
{
    public class EnumerableQuery<TRtln> : IQueryable<TRtln>
    {
        private IEnumerable<TRtln> Contents { get; }

        public EnumerableQuery(IEnumerable<TRtln> contents)
        {
            Contents = contents;
        }

        public IEnumerator<TRtln> GetEnumerator()
        {
            return Contents.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Type ElementType => typeof(TRtln);

        public Expression Expression { get; }

        public IQueryProvider Provider { get; }
    }
}
