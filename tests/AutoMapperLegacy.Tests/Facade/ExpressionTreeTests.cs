// #14: this project uses 3.x's ResolutionContext members, so it is on C# 14, and C# 14 changes what array.Contains is in
// an expression tree (webForCore#138). These tests pin that down, and the two workarounds the README gives.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace LegacyApp
{
    public class ExpressionTreeTests
    {
        private class Row
        {
            public int GroupId { get; set; }
        }

        [Fact]
        public void CSharp14MakesArrayContainsASpanCall()
        {
            int[] ids = { 1, 2 };
            Expression<Func<Row, bool>> query = r => ids.Contains(r.GroupId);

            // MemoryExtensions.Contains(op_Implicit(ids), r.GroupId): what EF 6, LINQ to SQL and visitors such as Umbraco
            // 7's ExpressionVisitorBase ("No logic supported for op_Implicit") don't know.
            var call = (MethodCallExpression)query.Body;
            Assert.Equal(typeof(MemoryExtensions), call.Method.DeclaringType);
            Assert.Equal("op_Implicit", ((MethodCallExpression)call.Arguments[0]).Method.Name);
        }

        [Fact]
        public void CastingToIEnumerableKeepsEnumerableContains()
        {
            int[] ids = { 1, 2 };
            Expression<Func<Row, bool>> query = r => ((IEnumerable<int>)ids).Contains(r.GroupId);

            var call = (MethodCallExpression)query.Body;
            Assert.Equal(typeof(Enumerable), call.Method.DeclaringType);
            Assert.True(query.Compile()(new Row { GroupId = 2 }));
        }

        [Fact]
        public void AVisitorCanMapTheSpanCallBack()
        {
            int[] ids = { 1, 2 };
            Expression<Func<Row, bool>> query = r => ids.Contains(r.GroupId);

            var mapped = (Expression<Func<Row, bool>>)new SpanContainsToEnumerable().Visit(query);

            var call = (MethodCallExpression)mapped.Body;
            Assert.Equal(typeof(Enumerable), call.Method.DeclaringType);
            Assert.Equal(new[] { true, false }, new[] { 2, 3 }.Select(id => mapped.Compile()(new Row { GroupId = id })));
        }

        // The README's visitor: MemoryExtensions.Contains(op_Implicit(array), value) becomes Enumerable.Contains(array,
        // value), as the smoke test does at the top of Umbraco 7's ExpressionVisitorBase.VisitMethodCall.
        private sealed class SpanContainsToEnumerable : ExpressionVisitor
        {
            protected override Expression VisitMethodCall(MethodCallExpression m)
            {
                // As in the README, line for line.
                if (m.Method.DeclaringType == typeof(MemoryExtensions) && m.Method.Name == "Contains" && m.Arguments.Count == 2
                    && m.Arguments[0] is MethodCallExpression { Method.Name: "op_Implicit" } span && span.Arguments[0].Type.IsArray)
                {
                    var contains = typeof(Enumerable).GetMethods().Single(x => x.Name == "Contains" && x.GetParameters().Length == 2)
                        .MakeGenericMethod(span.Arguments[0].Type.GetElementType());
                    return Visit(Expression.Call(contains, span.Arguments[0], m.Arguments[1]));
                }
                return base.VisitMethodCall(m);
            }
        }
    }
}
