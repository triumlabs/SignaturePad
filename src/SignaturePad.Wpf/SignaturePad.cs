using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Xamarin.Controls
{
	[TemplatePart (Name = PartSignaturePadCanvas, Type = typeof (InkCanvas))]
	public partial class SignaturePad : Control
	{
		public delegate void OnSignaturePadBorderTouch();

		public delegate void OnSignatureRecordingStarted();

		private const string PartSignaturePadCanvas = "SignaturePadCanvas";

		#region DependencyProperties

		public static readonly DependencyProperty StrokeColorProperty = DependencyProperty.Register (
				nameof (StrokeColor),
				typeof (Color),
				typeof (SignaturePad),
				new PropertyMetadata (Colors.DarkBlue));
		public Color StrokeColor
		{
			get { return (Color)GetValue (StrokeColorProperty); }
			set { SetValue (StrokeColorProperty, value); }
		}

		public static readonly DependencyProperty StrokeWidthProperty = DependencyProperty.Register (
				nameof (StrokeWidth),
				typeof (double),
				typeof (SignaturePad),
				new PropertyMetadata (1.0));
		public double StrokeWidth
		{
			get { return (double)GetValue (StrokeWidthProperty); }
			set { SetValue (StrokeWidthProperty, value); }
		}

		#endregion

		private readonly List<InkStroke> lStroke = new List<InkStroke> ();
		private InkStroke strokeStylus;
		private InkStroke strokeMouse;
		private InkCanvas canvasSignaturePad;
		public OnSignaturePadBorderTouch BorderTouch { get; set; }
		public OnSignatureRecordingStarted RecordingStarted { get; set; }
		private bool borderTouch = false;

		public SignaturePad ()
		{
			DefaultStyleKey = typeof (SignaturePad);
		}

		public Signature GetSignature()
		{
			if (lStroke.Count == 0)
			{
				return null;
			}

			var features = Signature.SignatureFeatures.None;
			var points = lStroke.SelectMany (stroke => stroke.Points).ToList ();
			if (points.Min (point => point.Pressure) != points.Max (point => point.Pressure))
			{
				features |= Signature.SignatureFeatures.Pressure;
			}
			if (points.Min (point => point.TiltOrieantation?.X ?? 0) != points.Max (point => point.TiltOrieantation?.X ?? 0))
			{
				features |= Signature.SignatureFeatures.TiltOrientationX;
			}
			if (points.Min (point => point.TiltOrieantation?.Y ?? 0) != points.Max (point => point.TiltOrieantation?.Y ?? 0))
			{
				features |= Signature.SignatureFeatures.TiltOrientationY;
			}

			var signature = new Signature (
				features,
				new Signature.SignatureFrame ((float)Width, (float)Height),
				lStroke
					?.Select (inkStroke =>
						{
							var tsOrigin = inkStroke.Points.Count > 0 ? inkStroke.Points[0].Timestamp : TimeSpan.Zero;
							var stroke = new Signature.SignatureStroke (
								(Signature.SignatureStrokeSource)inkStroke.Source,
								inkStroke.Points
									.Select (inkPoint => new Signature.SignaturePoint (
										new Signature.SignaturePointPosition ((float)inkPoint.Position.X, (float)inkPoint.Position.Y),
										inkPoint.Pressure,
										inkPoint.TiltOrieantation != null ? new Signature.SignatureTiltOrieantation (inkPoint.TiltOrieantation.X, inkPoint.TiltOrieantation.Y) : null,
										inkPoint.Timestamp - tsOrigin)),
								inkStroke.Timestamp);
							return stroke;
						}),
				lStroke[0].Timestamp);

			return signature;
		}

		public void Clear()
		{
			canvasSignaturePad.Strokes.Clear ();
			canvasSignaturePad.UpdateLayout ();

			lStroke.Clear ();
		}

		#region Control members

		public override void OnApplyTemplate ()
		{
			canvasSignaturePad = GetTemplateChild (PartSignaturePadCanvas) as InkCanvas;
			canvasSignaturePad.DefaultDrawingAttributes.Color = StrokeColor;
			canvasSignaturePad.DefaultDrawingAttributes.Width = StrokeWidth;

			canvasSignaturePad.StylusDown += HandleEventSignaturePadCanvasStylusDown;
			canvasSignaturePad.StylusMove += HandleEventSignaturePadCanvasStylusMove;
			canvasSignaturePad.StylusUp += HandleEventSignaturePadCanvasStylusUp;

			canvasSignaturePad.PreviewMouseDown += HandleEventSignaturePadCanvasPreviewMouseDown;
			canvasSignaturePad.PreviewMouseMove += HandleEventSignaturePadCanvasPreviewMouseMove;
			canvasSignaturePad.PreviewMouseUp += HandleEventSignaturePadCanvasPreviewMouseUp;
			canvasSignaturePad.MouseDown += (sender, e) => { ReviewAllMouseEvent (); };
			//canvasSignaturePad.MouseMove += (sender, e) => { ReviewAllMouseEvent (); };
			canvasSignaturePad.MouseUp += (sender, e) => { ReviewAllMouseEvent (); };
		}

		#endregion

		#region Event handlers

		private void HandleEventSignaturePadCanvasStylusDown (object sender, StylusEventArgs e)
		{
			ProcessStylusEvent (e);
		}

		private void HandleEventSignaturePadCanvasStylusMove (object sender, StylusEventArgs e)
		{
			ProcessStylusEvent (e);
		}

		private void HandleEventSignaturePadCanvasStylusUp (object sender, StylusEventArgs e)
		{
			ProcessStylusEvent (e);
		}

		private void ProcessStylusEvent (StylusEventArgs e, [CallerMemberName] string callerName = null)
		{
			DebugMessage ($"{callerName}");

			if (strokeMouse != null) return;

			if (callerName == nameof (HandleEventSignaturePadCanvasStylusDown))
			{
				if (lStroke?.Count == 0)
				{
					RecordingStarted?.Invoke ();
				}

				strokeStylus = new InkStroke (InkSource.Stylus);
				foreach (var sp in e.GetStylusPoints (canvasSignaturePad))
				{
					var ip = new InkPoint (sp.ToPoint (), sp.PressureFactor, null, GetCurrentTimestamp ());
					DebugInkPoint (callerName, strokeStylus.Points.Count, ip);

					if (ip.Position.X < 0 || ip.Position.X > Width || ip.Position.Y < 0 || ip.Position.Y > Height)
					{
						BorderTouch?.Invoke();
						return;
					}

					strokeStylus.Points.Add (ip);
				}
			}
			else if (strokeStylus != null)
			{
				var tsNow = GetCurrentTimestamp ();
				var sps = e.GetStylusPoints (canvasSignaturePad);
				if (sps.Count > 1)
				{
					var tsLast = strokeStylus.Points.Last ().Timestamp;

					for (var idxSp = 0; idxSp < sps.Count; idxSp++)
					{
						var sp = sps[idxSp];
						var tsPointApprox = TimeSpan.FromTicks ((tsNow.Ticks - tsLast.Ticks) * (idxSp + 1) / sps.Count + tsLast.Ticks);
						var ip = new InkPoint (sp.ToPoint (), sp.PressureFactor, null, tsPointApprox);
						DebugInkPoint (callerName, strokeStylus.Points.Count, ip);

						if (ip.Position.X < 0 || ip.Position.X > Width || ip.Position.Y < 0 || ip.Position.Y > Height)
						{
							BorderTouch?.Invoke();
							return;
						}

						strokeStylus.Points.Add (ip);
					}

				}
				else if (sps.Count > 0)
				{
					var ip = new InkPoint (sps[0].ToPoint (), sps[0].PressureFactor, null, tsNow);
					DebugInkPoint (callerName, strokeStylus.Points.Count, ip);

					if (ip.Position.X < 0 || ip.Position.X > Width || ip.Position.Y < 0 || ip.Position.Y > Height)
					{
						BorderTouch?.Invoke();
						return;
					}

					strokeStylus.Points.Add (ip);
				}

				if (callerName == nameof (HandleEventSignaturePadCanvasStylusUp))
				{
					DebugMessage ($"StrokeCaptured: #{lStroke.Count} Src = {strokeStylus.Source}, Count = {strokeStylus.Points.Count} @ {strokeStylus.Timestamp}");
					lStroke.Add (strokeStylus);
					strokeStylus = null;
				}
			}
		}

		private void HandleEventSignaturePadCanvasPreviewMouseDown (object sender, MouseEventArgs e)
		{
			ProcessMouseEvent (e);
		}

		private void HandleEventSignaturePadCanvasPreviewMouseMove (object sender, MouseEventArgs e)
		{
			ProcessMouseEvent (e);
		}

		private void HandleEventSignaturePadCanvasPreviewMouseUp (object sender, MouseEventArgs e)
		{
			ProcessMouseEvent (e);
		}

		private void ProcessMouseEvent(MouseEventArgs e, [CallerMemberName] string callerName = null)
		{
			DebugMessage ($"{callerName}");

			if (strokeStylus != null) return;

			strokeMouse = callerName == nameof(HandleEventSignaturePadCanvasPreviewMouseDown) ? new InkStroke (InkSource.Mouse) : strokeMouse;
			if (strokeMouse == null) return;

			var point = e.GetPosition (canvasSignaturePad);
			var ip = new InkPoint (point, 0f, null, GetCurrentTimestamp ());
			DebugInkPoint (callerName, strokeMouse.Points.Count, ip);

			if (ip.Position.X < 0 || ip.Position.X > Width || ip.Position.Y < 0 || ip.Position.Y > Height)
			{
				borderTouch = true;
				return;
			}

			if (lStroke?.Count == 0 && strokeMouse?.Points.Count == 0)
			{
				RecordingStarted?.Invoke();
			}

			strokeMouse.Points.Add (ip);

			if (callerName == nameof(HandleEventSignaturePadCanvasPreviewMouseUp))
			{
				DebugMessage ($"StrokeCaptured: #{lStroke.Count} Src = {strokeMouse.Source}, Count = {strokeMouse.Points.Count} @ {strokeMouse.Timestamp}");
				lStroke.Add (strokeMouse);
				strokeMouse = null;
			}
		}

		private void ReviewAllMouseEvent ()
		{
			if (borderTouch)
			{
				BorderTouch?.Invoke ();
				borderTouch = false;
			}
		}

		#endregion

		private static TimeSpan GetCurrentTimestamp()
		{
			return TimeSpan.FromTicks(Stopwatch.GetTimestamp());
		}

		private static void DebugInkPoint(string source, int index, InkPoint point)
		{
			DebugMessage($"{source} => #{index}: {point.Position.X} x {point.Position.Y} x {point.Pressure} @ {point.Timestamp.Ticks}");
		}

		private static void DebugMessage(string message)
		{
			Debug.WriteLine ($"[{DateTime.Now:HH:mm:ss.ffffff}] {message}");
		}
	}
}
