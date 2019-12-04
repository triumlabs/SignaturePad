using System.Diagnostics;

namespace Xamarin.Controls
{
	[DebuggerDisplay ("{DebuggerDisplay,nq}")]
	public sealed class InkTiltOrieantation
	{
		private string DebuggerDisplay => $"{X} x {Y}";

		public float X { get; private set; }

		public float Y { get; private set; }

		public InkTiltOrieantation (float x, float y)
		{
			X = x;
			Y = y;
		}
	}
}
