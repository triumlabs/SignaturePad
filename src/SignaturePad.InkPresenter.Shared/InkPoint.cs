using System;
using System.Diagnostics;

#if __ANDROID__
using NativePoint = System.Drawing.PointF;
#elif __IOS__
using NativePoint = CoreGraphics.CGPoint;
#elif WINDOWS_PHONE_APP
using NativePoint = Windows.Foundation.Point;
#elif WPF
using NativePoint = System.Windows.Point;
#endif

namespace Xamarin.Controls
{
	[DebuggerDisplay ("{DebuggerDisplay,nq}")]
	public sealed class InkPoint
	{
		private string DebuggerDisplay => $"{Position.X} x {Position.Y} X {Pressure} @ {Timestamp}";

		public NativePoint Position { get; private set; }

		public float Pressure { get; private set; }

		public InkTiltOrieantation TiltOrieantation { get; private set; }

		public TimeSpan Timestamp { get; private set; }

		public InkPoint (NativePoint position, float pressure, InkTiltOrieantation tiltOrieantation, TimeSpan timestamp)
		{
			Position = position;
			Pressure = pressure;
			TiltOrieantation = tiltOrieantation;
			Timestamp = timestamp;
		}
	}
}
