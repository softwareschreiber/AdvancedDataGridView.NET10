using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Forms;
using System.ComponentModel;

namespace Zuby
{
	public class LambdaFilterBuilder<T>
	{
		private Expression<Func<T, bool>> _lambda;

		public System.Linq.Expressions.Expression<Func<T, bool>> Lambda
		{
			get { return _lambda; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LambdaFilterBuilder&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="selection">The selection.</param>
		public LambdaFilterBuilder(ColumnFilterSelection selection)
		{
			if (selection != null && selection.SelectedValues != null && selection.SelectedValues.Count > 0)
			{
				var parameter = Expression.Parameter(typeof(T), "list");
				var property = Expression.Property(parameter, selection.DataPropertyName);

				if (selection.DataType == typeof(String))
				{
					var valueList             = Expression.Constant(selection.SelectedValues);
					var type                  = valueList.Type;
					MethodInfo containsMethod = type.GetMethod("Contains", new[] { typeof(String) });
					var call                  = Expression.Call(valueList, containsMethod, property);
					_lambda                   = Expression.Lambda<Func<T, bool>>(call, parameter);
				}
				else if (selection.DataType == typeof(System.Boolean))
				{
					//var valueList = Expression.Constant(selection.SelectedValues);
					List<bool> valuelist = new List<bool>();
					selection.SelectedValues.ForEach((p) =>
					{
						if (p is String)
						{
							string ps = (string)p;
							if (ps.ToLower() == "true")
							{
								valuelist.Add(true);
							}
							else
								valuelist.Add(false);
						}
						else
							valuelist.Add(false);
					});
					var type = valuelist.GetType();
					MethodInfo containsMethod = type.GetMethod("Contains", new[] { typeof(System.Boolean) });
					var valueList = Expression.Constant(valuelist);
					var call = Expression.Call(valueList, containsMethod, property);
					_lambda = Expression.Lambda<Func<T, bool>>(call, parameter);
				}
				else if (selection.DataType == typeof(DateTime))
				{
					var prop                             = Expression.Call(property, typeof(DateTime).GetProperty("Date").GetGetMethod());
					List<DateTime> list                  = new List<DateTime>();
					selection.SelectedValues.ForEach((p) => list.Add(((DateTime)p).Date));
					var valueList                        = Expression.Constant(list);
					var type                             = valueList.Type;
					MethodInfo containsMethod            = type.GetMethod("Contains", new[] { typeof(DateTime) });
					var call                             = Expression.Call(valueList, containsMethod, property);
					_lambda                              = Expression.Lambda<Func<T, bool>>(call, parameter);
				}
				else if (selection.DataType == typeof(DateTime?))
				{
					List<DateTime?> li                 = new List<DateTime?>();
					selection.SelectedValues.ForEach(p => li.Add((DateTime?)p));	// get all values as nullable DateTime
					List<String> lii                   = li.ConvertAll(p => p.HasValue ? p.Value.ToShortDateString() : p.ToString());	// transform all to String
					var valueList                      = Expression.Constant(lii);
					var type                           = valueList.Type;
					// Build Expression tree for if(p.HasValue) p.Value.ToShortDateString() else p.ToString()
					///////////////////////////////////////////////////////////////////////////////////////////////////////////////
					Expression left           = Expression.Property(property, typeof(DateTime?), "HasValue");
					Expression right          = Expression.Constant(true, typeof(bool));
					Expression hasValue       = Expression.Equal(left, right);
					Expression iftrueValue    = Expression.Property(property, "Value");
					LabelTarget returnTarget  = Expression.Label(typeof(String));
					Expression iftrue         = Expression.Return(returnTarget, Expression.Call(iftrueValue, typeof(DateTime).GetMethod("ToShortDateString", System.Type.EmptyTypes)));
					Expression iffalse        = Expression.Return(returnTarget, Expression.Call(property, typeof(DateTime?).GetMethod("ToString", System.Type.EmptyTypes)));
					Expression iff            = Expression.IfThenElse(hasValue, iftrue, iffalse);
					MethodInfo containsMethod = type.GetMethod("Contains", new[] { typeof(String) });
					var call                  = Expression.Call(valueList, containsMethod, Expression.Block(iff, Expression.Label(returnTarget, Expression.Constant(String.Empty))));
					_lambda                   = Expression.Lambda<Func<T, bool>>(call, parameter);
				}
				else
				{
					var valueList             = Expression.Constant(selection.SelectedValues.ConvertAll(new Converter<object, String>(Convert.ToString)));
					var type                  = valueList.Type;
					var prop                  = Expression.Call(property, typeof(Object).GetMethod("ToString"));
					MethodInfo containsMethod = type.GetMethod("Contains", new[] { typeof(String) });
					var call                  = Expression.Call(valueList, containsMethod, prop);
					_lambda                   = Expression.Lambda<Func<T, bool>>(call, parameter);
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LambdaFilterBuilder&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="customFilter">The custom filter.</param>
		public LambdaFilterBuilder(CustomFilter customFilter)
		{
			var parameter = Expression.Parameter(typeof(T), "list");
			var property = Expression.Property(parameter, customFilter.DataPropertyName);
			Expression right = Expression.Constant(customFilter.Value1, customFilter.DataType);

			switch(customFilter.FExpression)
			{
				case FilterExpression.equals :
					Expression e = Expression.Equal(property, right);
					_lambda      = Expression.Lambda<Func<T, bool>>(e, parameter);
					break;
				case FilterExpression.does_not_equal:
					Expression e1 = Expression.NotEqual(property, right);
					_lambda       = Expression.Lambda<Func<T, bool>>(e1, parameter);
					break;
				case FilterExpression.earlier_than:
				case FilterExpression.less_than:
					Expression e2 = Expression.LessThan(property, right);
					_lambda       = Expression.Lambda<Func<T, bool>>(e2, parameter);
					break;
				case FilterExpression.later_than:
				case FilterExpression.greater_than:
					Expression e3 = Expression.GreaterThan(property, right);
					_lambda       = Expression.Lambda<Func<T, bool>>(e3, parameter);
					break;
				case FilterExpression.less_than_or_equal_to:
					Expression e4 = Expression.LessThanOrEqual(property, right);
					_lambda       = Expression.Lambda<Func<T, bool>>(e4, parameter);
					break;
				case FilterExpression.greater_than_or_equal_to:
					Expression e5 = Expression.GreaterThanOrEqual(property, right);
					_lambda       = Expression.Lambda<Func<T, bool>>(e5, parameter);
					break;
				case FilterExpression.begins_with:
					String src                  = (String)customFilter.Value1;
					Expression val              = Expression.Constant(src);
					MethodInfo startsWithMethod = src.GetType().GetMethod("StartsWith", new[] { typeof(String) });
					var call                    = Expression.Call(property, startsWithMethod, val);
					_lambda                     = Expression.Lambda<Func<T, bool>>(call, parameter);
					break;
				case FilterExpression.does_not_begin_with:
					String src1                  = (String)customFilter.Value1;
					Expression val1              = Expression.Constant(src1);
					MethodInfo startsWithMethod1 = src1.GetType().GetMethod("StartsWith", new[] { typeof(String) });
					var call1                    = Expression.Call(property, startsWithMethod1, val1);
					var call2                    = Expression.Not(call1);
					_lambda                      = Expression.Lambda<Func<T, bool>>(call2, parameter);
					break;
				case FilterExpression.ends_with:
					String src3               = (String)customFilter.Value1;
					Expression val3           = Expression.Constant(src3);
					MethodInfo endsWithMethod = src3.GetType().GetMethod("EndsWith", new[] { typeof(String) });
					var call3                 = Expression.Call(property, endsWithMethod, val3);
					_lambda                   = Expression.Lambda<Func<T, bool>>(call3, parameter);
					break;
				case FilterExpression.does_not_end_with:
					String src4                = (String)customFilter.Value1;
					Expression val4            = Expression.Constant(src4);
					MethodInfo endsWithMethod1 = src4.GetType().GetMethod("EndsWith", new[] { typeof(String) });
					var call4                  = Expression.Call(property, endsWithMethod1, val4);
					Expression call5           = Expression.Not(call4);
					_lambda                    = Expression.Lambda<Func<T, bool>>(call5, parameter);
					break;
				case FilterExpression.between:
					Expression l1  = Expression.Constant(customFilter.Value1, customFilter.DataType);
					Expression r1  = Expression.Constant(customFilter.Value2, customFilter.DataType);
					Expression ge  = Expression.GreaterThanOrEqual(property, l1);
					Expression le  = Expression.LessThanOrEqual(property, r1);
					Expression and = Expression.And(ge, le);
					_lambda        = Expression.Lambda<Func<T, bool>>(and, parameter);
					break;
				case FilterExpression.contains:
					String src6        = (String)customFilter.Value1;
					Expression val6    = Expression.Constant(src6);
					MethodInfo indexOf = typeof(MyStringExtension).GetMethod("IndexOfLower", new[] { typeof(String), typeof(String) });
                    var call6          = Expression.Call(indexOf, new List<Expression>() { property, val6 } );
					Expression one     = Expression.Constant((int)0);
					Expression eq      = Expression.GreaterThanOrEqual(call6, one);
					_lambda            = Expression.Lambda<Func<T, bool>>(eq, parameter);
					break;
				case FilterExpression.does_not_contain:
					String src7         = (String)customFilter.Value1;
					Expression val7     = Expression.Constant(src7);
					MethodInfo indexOf7 = typeof(MyStringExtension).GetMethod("IndexOfLower", new[] { typeof(String), typeof(String) });
					var call7           = Expression.Call(indexOf7, new List<Expression>() { property, val7 });
					Expression one7     = Expression.Constant((int)-1);
					Expression eq7      = Expression.Equal(call7, one7);
					_lambda             = Expression.Lambda<Func<T, bool>>(eq7, parameter);
					break;
				default:
					break;
			}

		}

		#region DataGridView helper methods

		/// <summary>
		/// Sorts the list.
		/// </summary>
		/// <param name="dgv">The DGV.</param>
		/// <param name="liste">The liste.</param>
		/// <param name="bs">The bindingsource.</param>
		/// <param name="possibleStandardFilter">The possible standard filter.</param>
		public static void SortChanged(Zuby.ADGV.AdvancedDataGridView dgv, IList<T> liste, BindingSource bs, Zuby.CustomFilter possibleStandardFilter = null)
		{
			FilterChanged(dgv, liste, bs, possibleStandardFilter);
		}

		/// <summary>
		/// Filters the data.
		/// </summary>
		/// <param name="dgv">The DGV.</param>
		/// <param name="list">The list.</param>
		/// <param name="bs">The binding source.</param>
		/// <param name="possibleStandardFilter">The possible standard custom filter.</param>
		public static void FilterChanged(Zuby.ADGV.AdvancedDataGridView dgv, IList<T> list, BindingSource bs, Zuby.CustomFilter possibleStandardFilter = null)
		{
			bool datachanged = false;
			IQueryable<T> _querableData = list.AsQueryable();
			if (dgv.FilterSelektions.Count > 0)
			{
				for (int i = 0; i < dgv.FilterSelektions.Count; ++i)
				{
					Zuby.LambdaFilterBuilder<T> lBuilder = new Zuby.LambdaFilterBuilder<T>(dgv.FilterSelektions[i]);
					_querableData = _querableData.Where(lBuilder.Lambda);
					datachanged = true;
				}
			}
			if (dgv.CustomFilterList.Count > 0)
			{
				for (int i=0; i < dgv.CustomFilterList.Count; ++i)
				{
					Zuby.LambdaFilterBuilder<T> lBuilder = new Zuby.LambdaFilterBuilder<T>(dgv.CustomFilterList[i]);
					if (lBuilder.Lambda != null)
					{
						_querableData = _querableData.Where(lBuilder.Lambda);
						datachanged = true;
					}
				}
			}
			if (possibleStandardFilter != null)
			{
				Zuby.LambdaFilterBuilder<T> lBuilder = new Zuby.LambdaFilterBuilder<T>(possibleStandardFilter);
				if (lBuilder.Lambda != null)
				{
					_querableData = _querableData.Where(lBuilder.Lambda);
					datachanged = true;
				}

			}
			if (dgv.SortSelektion.Count > 0)
			{
				for (int i = 0; i < dgv.SortSelektion.Count; ++i)
				{
					Zuby.ColumnSortSelection s = dgv.SortSelektion[i];
					_querableData = Zuby.SortLambdaBuilder.CallOrderBy(_querableData, s.DataPropertyName, s.SortTyp);
					datachanged = true;
				}
			}
			if (datachanged)
			{
				try
				{
					if (list is BindingList<T>)
					{
						bs.DataSource = new BindingList<T>(_querableData.ToList());
					}
					else
						bs.DataSource = _querableData.ToList();
				}
				catch{}
			}
			else
			{
				bs.DataSource = list; // take the whole list
			}
			bs.ResetBindings(false);
		}
	}

		#endregion

}
