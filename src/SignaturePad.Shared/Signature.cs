using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

#if XFORMS
namespace SignaturePad.Forms
#else
namespace Xamarin.Controls
#endif
{
	public sealed class Signature
	{
		private static readonly IReadOnlyList<IReadOnlyList<SignaturePoint>> EmptyPoints = new ReadOnlyCollection<IReadOnlyList<SignaturePoint>> (new SignaturePoint[0][]);

		public SignatureFeatures Features { get; private set; }

		public SignatureFrame Frame { get; private set; }

		public IReadOnlyList<IReadOnlyList<SignaturePoint>> Points { get; private set; }

		public DateTime Timestamp { get; private set; }

		public Signature (SignatureFeatures features, SignatureFrame frame, IEnumerable<IEnumerable<SignaturePoint>> points, DateTime timestamp)
		{
			Features = features;
			Frame = frame;
			Points = new ReadOnlyCollection<IReadOnlyList<SignaturePoint>>(points
				?.Select (stroke => (IReadOnlyList<SignaturePoint>)new ReadOnlyCollection<SignaturePoint> (stroke.ToList ()))
				.ToList ());
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

		public sealed class SignatureFrame
		{
			public float Width { get; private set; }

			public float Height { get; private set; }

			public SignatureFrame (float width, float height)
			{
				Width = width;
				Height = height;
			}
		}

		public sealed class SignaturePoint
		{
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

		public sealed class SignaturePointPosition
		{
			public float X { get; private set; }

			public float Y { get; private set; }

			public SignaturePointPosition (float x, float y)
			{
				X = x;
				Y = y;
			}
		}

		public sealed class SignatureTiltOrieantation
		{
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
