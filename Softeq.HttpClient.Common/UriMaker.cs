// Developed for LilBytes by Softeq Development Corporation
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace Softeq.HttpClient.Common
{
    public class UriMaker
    {
        private const string NameProperty = "Name";
        private const string EmitDefaultValueNameAttribute = "EmitDefaultValue";
        private const string Point = ".";
        private const string Ampersand = "&";
        private const string Assignment = "=";
        private const string Slash = "/";
        private const string Query = "?";

        public Uri BuildUp<T>(Expression<Predicate<T>> queryParams, string baseUrl, params string[] pathFragments)
        {
            var uriStr = new StringBuilder();

            uriStr.Append(Combine(baseUrl, pathFragments));

            if (queryParams != null)
            {
                uriStr.Append($"{Query}{BuildQuery(queryParams.Body)}");
            }

            return new Uri(uriStr.ToString());
        }

        public Uri BuildUp(object queryParams, string baseUrl, params string[] pathFragments)
        {
            var uriStr = new StringBuilder();

            uriStr.Append(Combine(baseUrl, pathFragments));

            if (queryParams != null)
            {
                uriStr.Append($"{Query}{BuildQuery(queryParams)}");
            }

            return new Uri(uriStr.ToString());
        }

        public Uri BuildUp(object queryParams, string baseUrl)
        {
            var uriStr = new StringBuilder(baseUrl);

            if (queryParams != null)
            {
                uriStr.Append($"{Query}{BuildQuery(queryParams)}");
            }

            return new Uri(uriStr.ToString());
        }

        public Uri Combine(string baseUrl, params string[] pathFragments)
        {
            if (pathFragments.Length == 0)
            {
                throw new ArgumentException("Uri fragments should be defined", nameof(pathFragments));
            }

            var uriStrBuilder = new StringBuilder();

            uriStrBuilder.Append(baseUrl.EnsureNotEndsWith(Slash));

            pathFragments.Aggregate(
                uriStrBuilder,
                (url, fragment) =>
                    url.Append(fragment.EnsureStartsWith(Slash).EnsureNotEndsWith(Slash)));

            return new Uri(uriStrBuilder.ToString());
        }

        private string BuildQuery(object queryParams)
        {
            var properties = queryParams.GetType().GetRuntimeProperties();

            var queryParamsMap = new Dictionary<string, string>();

            foreach (var prop in properties)
            {
                if (prop.CustomAttributes.Any(a => a.AttributeType == typeof(IgnoreDataMemberAttribute)))
                {
                    continue;
                }

                var attr = prop.CustomAttributes.FirstOrDefault(a => a.AttributeType == typeof(DataMemberAttribute));

                var emitDefaultValueAttribute =
                    attr?.NamedArguments.FirstOrDefault(x => x.MemberName.Equals(EmitDefaultValueNameAttribute));

                var propertyValue = prop.GetValue(queryParams);

                if (emitDefaultValueAttribute?.TypedValue.Value as bool? == false &&
                    propertyValue?.ToString() == GetDefaultValue(prop.PropertyType)?.ToString())
                {
                    continue;
                }

                var paramName = prop.Name;

                var namedProperty = attr?.NamedArguments.FirstOrDefault(x => x.MemberName == NameProperty);

                if (namedProperty != null)
                {
                    paramName = namedProperty.Value.TypedValue.Value.ToString();
                }

                var value = CreateQueryValue(propertyValue, paramName);

                if (value != null)
                {
                    queryParamsMap.Add(paramName, value);
                }
            }

            return string.Join(
                Ampersand,
                queryParamsMap.Select(param => $"{param.Key}{Assignment}{param.Value}"));
        }

        public object GetDefaultValue(Type t)
        {
            return t.GetTypeInfo().IsValueType ? Activator.CreateInstance(t) : null;
        }

        #region Expression Tree visitor

        private string CallRightVisitMethod(Expression exp)
        {
            switch (exp)
            {
                case BinaryExpression expression:
                    return Visit(expression);
                case MemberExpression memberExpression:
                    return Visit(memberExpression);
                case ConstantExpression constantExpression:
                    return Visit(constantExpression);
                case ParameterExpression parameterExpression:
                    return Visit(parameterExpression);
                default:
                    throw new NotSupportedException($"{exp.GetType().Name} is not supported");
            }
        }

        private string BuildQuery(Expression exp)
        {
            return CallRightVisitMethod(exp);
        }

        private string Visit(BinaryExpression exp)
        {
            return
                $"{VisitSubExpression(exp.Left)}{ExpressionNodeToString(exp.NodeType)}{VisitSubExpression(exp.Right)}";
        }

        private string ExpressionNodeToString(ExpressionType nodeType)
        {
            switch (nodeType)
            {
                case ExpressionType.AndAlso:
                    return Ampersand;
                case ExpressionType.Equal:
                    return Assignment;
                default:
                    throw new NotSupportedException($"Expression NodeType '{nodeType}' is not supported");
            }
        }

        private string Visit(ConstantExpression exp)
        {
            return CreateQueryValue(exp.Value);
        }

        private string CreateQueryValue(object value, string key = null)
        {
            if (value == null)
            {
                return null;
            }

            var resultValue = value;

            switch (value)
            {
                case double d:
                    resultValue = d.ToString(CultureInfo.InvariantCulture);
                    break;
                case bool b:
                    resultValue = b.ToString().ToLower();
                    break;
                case IEnumerable<object> collection:
                {
                    var separator = string.Join(key, Ampersand, Assignment);

                    foreach (var item in collection)
                    {
                        var itemEncode = WebUtility.UrlEncode(item.ToString());
                        if (resultValue.Equals(collection))
                        {
                            resultValue = itemEncode;

                            continue;
                        }

                        resultValue = string.Join(separator, resultValue, itemEncode);
                    }

                    return resultValue.ToString();
                }
            }

            return WebUtility.UrlEncode(resultValue.ToString());
        }

        private string Visit(ParameterExpression exp)
        {
            return exp.Name;
        }

        private string Visit(MemberExpression exp)
        {
            if (!(exp.Expression is ConstantExpression))
            {
                return GetDataMemberAttributeFullPath(exp);
            }

            var propertyInfo = exp.Member as PropertyInfo;

            if (propertyInfo != null)
            {
                return CreateQueryValue(Convert.ToString(propertyInfo.GetValue(((ConstantExpression) exp.Expression).Value)));
            }

            var fieldInfo = exp.Member as FieldInfo;

            if (fieldInfo != null)
            {
                return CreateQueryValue(Convert.ToString(fieldInfo.GetValue(((ConstantExpression) exp.Expression).Value)));
            }

            return string.Empty;
        }

        private string VisitSubExpression(Expression exp)
        {
            return CallRightVisitMethod(exp);
        }

        private static string GetDataMemberAttributeFullPath(MemberExpression memberExpr)
        {
            if (memberExpr == null)
            {
                return string.Empty;
            }

            var stack = new Stack<string>();

            while (memberExpr != null)
            {
                var attr = memberExpr.Member.CustomAttributes.FirstOrDefault(a => a.AttributeType == typeof(DataMemberAttribute));

                if (attr != null && attr.NamedArguments?.Count != 0)
                {
                    var name = attr.NamedArguments.First().TypedValue.Value.ToString();
                    stack.Push(name);
                }
                else
                {
                    stack.Push(memberExpr.Member.Name);
                }

                memberExpr = memberExpr.Expression as MemberExpression;
            }

            return string.Join(Point, stack.ToArray());
        }

        #endregion
    }
}