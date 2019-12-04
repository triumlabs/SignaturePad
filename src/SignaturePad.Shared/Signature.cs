using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

#if XFORMS
namespace SignaturePad.Forms
#else
namespace Xamarin.Controls
#endif
{
	/// <summary>
	/// 
	/// </summary>
	/// <remarks>DebuggerDiaply: https://blogs.msdn.microsoft.com/jaredpar/2011/03/18/debuggerdisplay-attribute-best-practices/</remarks>
	[DebuggerDisplay ("{DebuggerDisplay,nq}")]
	public sealed class Signature
	{
		private string DebuggerDisplay => $"Strokes = {Strokes.Count} @ {Timestamp}";
		private static readonly IReadOnlyList<IReadOnlyList<SignaturePoint>> EmptyPoints = new ReadOnlyCollection<IReadOnlyList<SignaturePoint>> (new SignaturePoint[0][]);

		public SignatureFeatures Features { get; private set; }

		public SignatureFrame Frame { get; private set; }

		public IReadOnlyList<SignatureStroke> Strokes { get; private set; }

		public DateTime Timestamp { get; private set; }

		public IList<SignaturePoint> GetPoints()
		{
			return Strokes.SelectMany (stroke => stroke.Points).ToList ();
		}

		public Signature (SignatureFeatures features, SignatureFrame frame, IEnumerable<SignatureStroke> strokes, DateTime timestamp)
		{
			Features = features;
			Frame = frame;
			Strokes = new ReadOnlyCollection<SignatureStroke> (strokes.ToList ());
			Timestamp = timestamp;
		}

		[Flags]
		public enum SignatureFeatures
		{
			None,

			TiltOrientationX = 1 << 1,

			TiltOrientationY = 1 << 2,

			Pressure = 1 << 3,
		}

		[DebuggerDisplay ("{DebuggerDisplay,nq}")]
		public sealed class SignatureFrame
		{
			private string DebuggerDisplay => $"{Width} x {Height}";

			public float Width { get; private set; }

			public float Height { get; private set; }

			public SignatureFrame (float width, float height)
			{
				Width = width;
				Height = height;
			}
		}

		[DebuggerDisplay ("{DebuggerDisplay,nq}")]
		public sealed class SignatureStroke
		{
			private string DebuggerDisplay => $"Count = {Points.Count} @ {Timestamp}";

			public SignatureStrokeSource Source { get; private set; }

			public IReadOnlyList<SignaturePoint> Points { get; private set; }

			public DateTime Timestamp { get; private set; }

			public SignatureStroke (SignatureStrokeSource source, IEnumerable<SignaturePoint> points, DateTime timestamp)
			{
				Source = source;
				Points = new ReadOnlyCollection<SignaturePoint> (points.ToList());
				Timestamp = timestamp;
			}
		}

		public enum SignatureStrokeSource
		{
			Undefined = 0,
			Mouse,
			Touch,
			Stylus,
		}

		[DebuggerDisplay ("{DebuggerDisplay,nq}")]
		public sealed class SignaturePoint
		{
			private string DebuggerDisplay => $"{Position.X} x {Position.Y} x {Pressure} @ {Timestamp.Ticks}";

			public SignaturePointPosition Position { get; private set; }

			public float Pressure { get; private set; }

			public SignatureTiltOrieantation TiltOrieantation { get; private set; }

			public TimeSpan Timestamp { get; private set; }

			public SignaturePoint (SignaturePointPosition position, float pressure, SignatureTiltOrieantation tiltOrieantation, TimeSpan timestamp)
			{
				Position = position;
				Pressure = pressure;
				TiltOrieantation = tiltOrieantation;
				Timestamp = timestamp;
			}
		}

		[DebuggerDisplay ("{DebuggerDisplay,nq}")]
		public sealed class SignaturePointPosition
		{
			private string DebuggerDisplay => $"{X} x {Y}";

			public float X { get; private set; }

			public float Y { get; private set; }

			public SignaturePointPosition (float x, float y)
			{
				X = x;
				Y = y;
			}
		}

		[DebuggerDisplay ("{DebuggerDisplay,nq}")]
		public sealed class SignatureTiltOrieantation
		{
			private string DebuggerDisplay => $"{X} x {Y}";

			public float X { get; private set; }

			public float Y { get; private set; }

			public SignatureTiltOrieantation (float x, float y)
			{
				X = x;
				Y = y;
			}
		}
	}
}
