using System;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;


namespace Kalow.Apps.Common.Reflection
{
    public static class DynamicCaller
    {
        public static string Evaluate(object o, string function, string member)
        {
            MethodInfo method = typeof(string).GetMethod(function);
            PropertyInfo property = o.GetType().GetProperty(member);
            string originalValue = property.GetValue(o).ToString();
            return method.Invoke(originalValue, Array.Empty<object>()).ToString();
        }

        public static T EvalueExpression<T>(this object o, string expression)
        {
            var p = Expression.Parameter(o.GetType(), "o");
            var exp = DynamicExpressionParser.ParseLambda(new[] { p }, typeof(string), expression);
            return (T)exp.Compile().DynamicInvoke(o);
        }
    }
}
