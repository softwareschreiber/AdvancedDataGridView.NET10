using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zuby
{
	public enum FilterExpression
	{
		nothing,
		equals,
		does_not_equal,
		earlier_than,
		later_than,
		between,
		greater_than,
		greater_than_or_equal_to,
		less_than,
		less_than_or_equal_to,
		begins_with,
		does_not_begin_with,
		ends_with,
		does_not_end_with,
		contains,
		does_not_contain
	}
	public class CustomFilter
	{
		public FilterExpression FExpression { get; set; }
		public String Text { get; set; }
		public String DataPropertyName { get; set; }
		public Type DataType { get; set; }
		public Object Value1 { get; set; }
		public Object Value2 { get; set; }

		public CustomFilter(string t, FilterExpression e)
		{
			FExpression = e;
			Text = t;
		}

		public override string ToString()
		{
			return Text;
		}
	}
}
