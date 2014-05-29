﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ValueUtils {
    public static class FieldwiseEquality {
        /// <summary>
        /// Checks whether two objects of the same type are field-wise equal.  Type resolution is done 
        /// statically, which allows fast code (similar to hand-rolled performance).
        /// However, warning: if the objects are of compile-time type BaseType, but at runtime turn out
        /// to be SubClass, then only the fields of BaseType will be checked.
        /// 
        /// This is simply a type-inference friendly wrapper around FieldwiseEquality&lt;&gt;.Instance
        /// </summary>
        /// <typeparam name="T">The type of the objects to compare.</typeparam>
        public static bool AreEqual<T>(T a, T b) { return FieldwiseEquality<T>.Instance(a, b); }

    }
    public static class FieldwiseEquality<T> {
        /// <summary>
        /// Checks whether two objects of the same type are field-wise equal.  Type resolution is specified 
        /// statically, which allows fast code (similar to hand-rolled performance).
        /// However, warning: if the objects are of compile-time type BaseType, but at runtime turn out
        /// to be SubClass, then only the fields of BaseType will be checked.
        /// </summary>
        public static readonly Func<T, T, bool> Instance = Create();

        static Func<T, T, bool> Create() {
            //Get all fields including inherited fields
            var fields = typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            var aExpr = Expression.Parameter(typeof(T), "a");
            var bExpr = Expression.Parameter(typeof(T), "b");
            var objectEqualsMethod = ((Func<object, object, bool>)object.Equals).Method;

            Expression eqExpr = Expression.Constant(true);

            foreach (var fieldInfo in fields) {
                var aFieldExpr = Expression.Field(aExpr, fieldInfo);
                var bFieldExpr = Expression.Field(bExpr, fieldInfo);
                var fieldsEqualExpr =
                    Expression.Call(objectEqualsMethod,
                        Expression.Convert(aFieldExpr, typeof(object)),
                        Expression.Convert(bFieldExpr, typeof(object))
                    );
                eqExpr = Expression.AndAlso(eqExpr, fieldsEqualExpr);
            }
            var funcExpr = Expression.Lambda<Func<T, T, bool>>(eqExpr, aExpr, bExpr);

            return funcExpr.Compile();
        }
    }
}
