using System.Diagnostics;

namespace Xamarin.Controls
{
	[DebuggerDisplay ("{X} x {Y}")]
	public sealed class InkTiltOrieantation
	{
		public float X { get; private set; }

		public float Y { get; private set; }

		public InkTiltOrieantation (float x, float y)
		{
			X = x;
			Y = y;
		}
	}
}
