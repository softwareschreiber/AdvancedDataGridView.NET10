using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zuby
{
	public class ColumnFilterSelection
	{
		public List<Object> SelectedValues { get; set; }
		public String DataPropertyName { get; set; }
		public Type DataType { get; set; }
	}
}
