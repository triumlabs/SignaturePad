using System;

namespace Xamarin.Controls
{
	[Flags]
	public enum InkFeatures : uint
	{
		None = 0,

		TiltOrientationX = 1 << 1,

		TiltOrientationY = 1 << 2,

		Pressure = 1 << 3,
	}
}
