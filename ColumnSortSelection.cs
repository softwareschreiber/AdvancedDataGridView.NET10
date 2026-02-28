using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zuby
{
	public class ColumnSortSelection
	{
		public String DataPropertyName { get; set; }
		public Zuby.SortType SortTyp { get; set; }
		public Type DataType { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ColumnSortSelection"/> class.
		/// </summary>
		/// <param name="dpn">The DPN.</param>
		/// <param name="t">The t.</param>
		public ColumnSortSelection(String dpn, Zuby.SortType t, Type typ)
		{
			DataPropertyName = dpn;
			SortTyp          = t;
			DataType         = typ;
		}
	}
}
