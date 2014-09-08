using System;
using System.Collections.Generic;
using System.Linq;

namespace TreeGraphControl
{
	public class DisplayGrid
	{
		private List<Cell> cells = new List<Cell>();

		public virtual Cell Reserve(int col, int line, ITreeNode node)
		{
			if (IsCellTaken(col, line))
				throw new InvalidOperationException("Cell already taken");

			Cell cell = new Cell(col, line, node, this);
			cells.Add(cell);
			return cell;
		}

		public virtual bool IsCellTaken(int col, int line)
		{
			return cells.Exists(cell => cell.Column == col && cell.Line == line);
		}

		public virtual int MaxColumn {
			get { return cells.Count > 0 ? cells.Max(cell => cell.Column) : 0; }
		}

		public virtual int MaxLine {
			get { return cells.Count > 0 ? cells.Max(cell => cell.Line) : 0; }
		}

		public class Cell
		{
			public virtual int Column {
				get;
				private set;
			}

			public virtual int Line {
				get;
				private set;
			}

			public virtual ITreeNode Content {
				get;
				private set;
			}

			public virtual DisplayGrid Grid {
				get;
				private set;
			}

			public Cell(int col, int line, ITreeNode content, DisplayGrid grid)
			{
				Column = col;
				Line = line;
				Content = content;
				Grid = grid;
			}
		}
	}
}