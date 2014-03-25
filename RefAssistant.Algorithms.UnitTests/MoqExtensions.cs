using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Moq
{
    internal static class MoqExtensions
    {
        public static Mock<T> SetupUnionWith<T, TProperty>(this Mock<T> @this, Expression<Func<T, IEnumerable<TProperty>>> expression, TProperty addedItem)
            where T : class
        {
            var existigItemsFunc = expression.Compile();
            var existingItems = (existigItemsFunc(@this.Object) ?? Enumerable.Empty<TProperty>());

            @this.SetupGet<IEnumerable<TProperty>>(expression)
                .Returns(existingItems.Concat(Enumerable.Repeat(addedItem, 1)).ToArray());

            return @this;
        }

        public static Mock<T> SetupUnionWith<T, TProperty>(this Mock<T> @this, Expression<Func<T, IEnumerable<TProperty>>> expression, IEnumerable<TProperty> addedItems)
            where T : class
        {
            var existigItemsFunc = expression.Compile();
            var existingItems = (existigItemsFunc(@this.Object) ?? Enumerable.Empty<TProperty>());

            @this.SetupGet<IEnumerable<TProperty>>(expression)
                .Returns(existingItems.Concat(addedItems).ToArray());

            return @this;
        }
    }
}
