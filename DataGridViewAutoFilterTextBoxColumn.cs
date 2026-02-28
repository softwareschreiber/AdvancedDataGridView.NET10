using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DataGridViewAutoFilter
{
	public class DataGridViewAutoFilterTextBoxColumn : DataGridViewTextBoxColumn
	{
		private string _TableColumnName;

		/// <summary>
		/// Initializes a new instance of the DataGridViewAutoFilterTextBoxColumn class.
		/// </summary>
		public DataGridViewAutoFilterTextBoxColumn()
			: base()
		{
		}


        ///// <summary>
        ///// Gets or sets the real name of the table column.
        ///// </summary>
        ///// <value>The name of the table column.</value>
        //[DefaultValue(""), Description("Sets the real name of the table column.")]
        [DefaultValue("Name")]
        public System.String TableColumnName
		{
			get { return _TableColumnName; /* ((DataGridViewAutoFilterColumnHeaderCell)HeaderCell).TableColumnName;*/ }
			set
			{
				this._TableColumnName = value;
			}
		}
        [DefaultValue("Name")]
        public new string DataPropertyName
		{
			get
			{
				if (!String.IsNullOrEmpty(TableColumnName))
				{
					return TableColumnName;
				}
				else
				{
					return base.DataPropertyName;
				}
			}
			set
			{
				base.DataPropertyName = value;
			}
		}
        [DefaultValue(true)]
        public bool FilteringEnabled { get; set; }
	}
}
