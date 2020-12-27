// 
// BaseTransformTool.cs
//  
// Author:
//       Volodymyr <${AuthorEmail}>
// 
// Copyright (c) 2012 Volodymyr
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using Cairo;
using Gtk;
using Pinta.Core;

namespace Pinta.Tools
{
	public abstract class BaseTransformTool : BaseTool
	{
        #region Members
        private readonly int rotate_steps = 32;
        private readonly Matrix transform = new Matrix();
		private Rectangle source_rect;
		private PointD original_point;
		private bool is_dragging = false;
		private bool is_rotating = false;
		private bool is_scaling = false;
		private bool using_mouse = false;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="Pinta.Tools.BaseTransformTool"/> class.
        /// </summary>
        public BaseTransformTool ()
		{
		}
		#endregion

		#region Implemenation

		protected abstract Cairo.Rectangle GetSourceRectangle();

		protected virtual void OnStartTransform()
		{
			source_rect = GetSourceRectangle();
			transform.InitIdentity();
		}

		protected virtual void OnUpdateTransform(Matrix transform)
		{
		}

		protected virtual void OnFinishTransform(Matrix transform)
		{
			is_dragging = false;
			is_rotating = false;
			is_scaling = false;
			using_mouse = false;
		}

		protected override void OnMouseDown (Gtk.DrawingArea canvas,
		                                     Gtk.ButtonPressEventArgs args,
		                                     Cairo.PointD point)
		{
			Document doc = PintaCore.Workspace.ActiveDocument;

			if(IsActive)
				return;

			original_point = point;
			if (!doc.Workspace.PointInCanvas(point))
				return;

			if (args.Event.Button == GtkExtensions.MouseRightButton)
				is_rotating = true;
			else if (args.Event.State.IsControlPressed ())
				is_scaling = true;
			else
				is_dragging = true;

			using_mouse = true;
			OnStartTransform();
		}

		protected override void OnMouseMove (object o,
		                                     Gtk.MotionNotifyEventArgs args,
		                                     Cairo.PointD point)
		{
			if (!IsActive || !using_mouse)
				return;

			bool constrain = args.Event.State.IsShiftPressed ();

			PointD center = source_rect.GetCenter();

			// The cursor position can be a subpixel value. Round to an integer
			// so that we only translate by entire pixels.
			// (Otherwise, blurring / anti-aliasing may be introduced)
			double dx = Math.Floor(point.X - original_point.X);
			double dy = Math.Floor(point.Y - original_point.Y);

			double cx1 = original_point.X - center.X;
			double cy1 = original_point.Y - center.Y;

			double cx2 = point.X - center.X;
			double cy2 = point.Y - center.Y;

			double angle = Math.Atan2(cy1, cx1) - Math.Atan2(cy2, cx2);

			transform.InitIdentity ();

			if (is_scaling)
			{
				double sx = (cx1 + dx) / cx1;
				double sy = (cy1 + dy) / cy1;
				if (constrain)
				{
					var max_scale = Math.Max (Math.Abs (sx), Math.Abs (sy));
					sx = max_scale * Math.Sign (sx);
					sy = max_scale * Math.Sign (sy);
				}

				transform.Translate(center.X, center.Y);
				transform.Scale (sx, sy);
				transform.Translate(-center.X, -center.Y);
			}
			else if (is_rotating)
			{
				if (constrain)
					angle = Utility.GetNearestStepAngle (angle, rotate_steps);

				transform.Translate(center.X, center.Y);
				transform.Rotate(-angle);
				transform.Translate(-center.X, -center.Y);
			}
			else
			{
				transform.Translate(dx, dy);
			}

			OnUpdateTransform (transform);
		}

		protected override void OnMouseUp (Gtk.DrawingArea canvas, Gtk.ButtonReleaseEventArgs args, Cairo.PointD point)
		{
			if (!IsActive || !using_mouse)
				return;

			OnFinishTransform(transform);
		}

        protected override void OnKeyDown (DrawingArea canvas, KeyPressEventArgs args)
        {
            // Don't handle the arrow keys while already interacting via the mouse.
            if (using_mouse) {
                base.OnKeyDown (canvas, args);
                return;
            }

            double dx = 0.0;
            double dy = 0.0;

            switch (args.Event.Key) {
            case Gdk.Key.Left:
                dx = -1;
                break;
            case Gdk.Key.Right:
                dx = 1;
                break;
            case Gdk.Key.Up:
                dy = -1;
                break;
            case Gdk.Key.Down:
                dy = 1;
                break;
            default:
                // Otherwise, let the key be handled elsewhere.
                base.OnKeyDown (canvas, args);
                return;
            }

            if (!IsActive) {
                is_dragging = true;
                OnStartTransform ();
            }

            transform.Translate (dx, dy);
            OnUpdateTransform (transform);

            args.RetVal = true;
        }

        protected override void OnKeyUp (DrawingArea canvas, KeyReleaseEventArgs args)
        {
            if (IsActive && !using_mouse) {
                OnFinishTransform (transform);
            }

            base.OnKeyUp (canvas, args);
        }

        private bool IsActive { get { return is_dragging || is_rotating || is_scaling; } }
        #endregion
    }
}

