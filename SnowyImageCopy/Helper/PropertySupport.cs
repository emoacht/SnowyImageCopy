using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SnowyImageCopy.Helper
{
	public class PropertySupport
	{
		/// <summary>
		/// Get property name from a specified property expression.
		/// </summary>
		/// <typeparam name="T">Object type containing the property specified in a property expression</typeparam>
		/// <param name="propertyExpression">Property expression</param>
		/// <returns>Property name</returns>
		public static string GetPropertyName<T>(Expression<Func<T>> propertyExpression)
		{
			if (propertyExpression == null)
				throw new ArgumentNullException("propertyExpression");

			var memberExpression = propertyExpression.Body as MemberExpression;
			if (memberExpression == null)
				throw new ArgumentException("The expression is not a MemberExpression.");

			return memberExpression.Member.Name;
		}
	}
}