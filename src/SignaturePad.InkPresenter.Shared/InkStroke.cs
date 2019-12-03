using System.Collections.Generic;

#if __ANDROID__
using NativeColor = Android.Graphics.Color;
using NativePath = Android.Graphics.Path;
#elif __IOS__
using NativeColor = UIKit.UIColor;
using NativePath = UIKit.UIBezierPath;
#elif WINDOWS_PHONE_APP
using NativeColor = Windows.UI.Color;
using NativePath = Windows.UI.Xaml.Media.PathGeometry;
#endif

namespace Xamarin.Controls
{
	internal class InkStroke
	{
		private NativeColor color;
		private float width;
		private IList<InkPoint> inkPoints;

		public InkStroke (NativePath path, IList<InkPoint> points, NativeColor color, float width)
		{
			Path = path;
			inkPoints = points;
			Color = color;
			Width = width;
		}

		public NativePath Path { get; private set; }

		public IList<InkPoint> GetPoints ()
		{
			return inkPoints;
		}

		public NativeColor Color
		{
			get { return color; }
			set
			{
				color = value;
				IsDirty = true;
			}
		}

		public float Width
		{
			get { return width; }
			set
			{
				width = value;
				IsDirty = true;
			}
		}

		internal bool IsDirty { get; set; }
	}
}
