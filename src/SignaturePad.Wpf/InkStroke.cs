using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Xamarin.Controls
{
	[DebuggerDisplay ("{DebuggerDisplay,nq}")]
	internal class InkStroke
	{
		private string DebuggerDisplay => $"Count = {Points.Count}, Src = {Source}";

		public InkSource Source { get; private set; }
		public IList<InkPoint> Points { get; private set; }
		public DateTime Timestamp { get; private set; }

		public InkStroke (InkSource source)
		{
			Source = source;
			Points = new List<InkPoint> ();
			Timestamp = DateTime.UtcNow;
		}
	}
}
