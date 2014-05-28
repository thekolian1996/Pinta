// 
// LineCurveTool.cs
//  
// Author:
//       Jonathan Pobst <monkey@jpobst.com>
// 
// Copyright (c) 2010 Jonathan Pobst
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
using Pinta.Core;
using Mono.Unix;
using System.Collections.Generic;
using System.Linq;

namespace Pinta.Tools
{
	public class LineCurveTool : ShapeTool
	{
        private EditEngine ee = new EditEngine();


		public override string Name
		{
			get { return Catalog.GetString("Line/Curve"); }
		}
		public override string Icon {
			get { return "Tools.Line.png"; }
		}
		public override string StatusBarText {
			get { return Catalog.GetString ("Left click to draw a line with primary color." +
					"\nLeft click on a line to add curve control points." +
					"\nLeft click on a control point and drag to move it." +
					"\nRight click on a control point and drag to change tension." +
					"\nHold Shift to snap to angles." +
					"\nUse arrow keys to move selected control point." +
					"\nPress Ctrl + left/right arrows to navigate through (select) control points by order." +
					"\nPress Delete to delete selected control point." +
					"\nPress Space to create a new point on the outermost side of the selected control point at the mouse position." +
					"\nHold Ctrl while pressing Space to create the point at the exact same position." +
					"\nHold Ctrl while left clicking on a control point to create a new line at the exact same position." +
					"\nHold Ctrl while clicking outside of the Image bounds to create a new line starting at the edge." +
					"\nPress Enter to finalize the curve.");
			}
		}
		public override Gdk.Cursor DefaultCursor {
			get { return new Gdk.Cursor (PintaCore.Chrome.Canvas.Display, PintaCore.Resources.GetIcon ("Cursor.Line.png"), 9, 18); }
		}
		protected override bool ShowStrokeComboBox {
			get { return false; }
		}
		public override int Priority {
			get { return 39; }
		}


		private DashPatternBox dashPBox = new DashPatternBox();


		private Gtk.SeparatorToolItem arrowSep;
		private ToolBarLabel arrowLabel;
		private Gtk.CheckButton showArrowOneBox, showArrowTwoBox;
		private bool showOtherArrowOptions;

		private ToolBarComboBox arrowSize;
		private ToolBarLabel arrowSizeLabel;
		private ToolBarButton arrowSizeMinus, arrowSizePlus;

		private ToolBarComboBox arrowAngleOffset;
		private ToolBarLabel arrowAngleOffsetLabel;
		private ToolBarButton arrowAngleOffsetMinus, arrowAngleOffsetPlus;

		private ToolBarComboBox arrowLengthOffset;
		private ToolBarLabel arrowLengthOffsetLabel;
		private ToolBarButton arrowLengthOffsetMinus, arrowLengthOffsetPlus;


		protected override void BuildToolBar(Gtk.Toolbar tb)
		{
			base.BuildToolBar(tb);


			Gtk.ComboBox dpbBox = dashPBox.SetupToolbar(tb);

			if (dpbBox != null)
			{
				dpbBox.Changed += (o, e) =>
				{
					ee.ActiveCurveEngine.DashPattern = dpbBox.ActiveText;

					//Update the line/curve.
					DrawCurves(false, false, false);
				};
			}


			#region Show Arrows

			//Arrow separator.

			if (arrowSep == null)
			{
				arrowSep = new Gtk.SeparatorToolItem();

				showOtherArrowOptions = false;
			}

			tb.AppendItem(arrowSep);


			if (arrowLabel == null)
			{
				arrowLabel = new ToolBarLabel(string.Format(" {0}: ", Catalog.GetString("Arrow")));
			}

			tb.AppendItem(arrowLabel);


			//Show arrow 1.

			showArrowOneBox = new Gtk.CheckButton("1");

			showArrowOneBox.Toggled += (o, e) =>
			{
				//Determine whether to change the visibility of Arrow options in the toolbar based on the updated Arrow showing/hiding.
				if (!showArrowOneBox.Active && !showArrowTwoBox.Active)
				{
					if (showOtherArrowOptions)
					{
						tb.Remove(arrowSizeLabel);
						tb.Remove(arrowSizeMinus);
						tb.Remove(arrowSize);
						tb.Remove(arrowSizePlus);
						tb.Remove(arrowAngleOffsetLabel);
						tb.Remove(arrowAngleOffsetMinus);
						tb.Remove(arrowAngleOffset);
						tb.Remove(arrowAngleOffsetPlus);
						tb.Remove(arrowLengthOffsetLabel);
						tb.Remove(arrowLengthOffsetMinus);
						tb.Remove(arrowLengthOffset);
						tb.Remove(arrowLengthOffsetPlus);

						showOtherArrowOptions = false;
					}
				}
				else
				{
					if (!showOtherArrowOptions)
					{
						tb.Add(arrowSizeLabel);
						tb.Add(arrowSizeMinus);
						tb.Add(arrowSize);
						tb.Add(arrowSizePlus);
						tb.Add(arrowAngleOffsetLabel);
						tb.Add(arrowAngleOffsetMinus);
						tb.Add(arrowAngleOffset);
						tb.Add(arrowAngleOffsetPlus);
						tb.Add(arrowLengthOffsetLabel);
						tb.Add(arrowLengthOffsetMinus);
						tb.Add(arrowLengthOffset);
						tb.Add(arrowLengthOffsetPlus);

						showOtherArrowOptions = true;
					}
				}

				CurveEngine selEngine = ee.SelectedCurveEngine;

				if (selEngine != null)
				{
					selEngine.Arrow1.Show = showArrowOneBox.Active;

					DrawCurves(false, false, false);
				}
			};

			tb.AddWidgetItem(showArrowOneBox);


			//Show arrow 2.

			showArrowTwoBox = new Gtk.CheckButton("2");

			showArrowTwoBox.Toggled += (o, e) =>
			{
				//Determine whether to change the visibility of Arrow options in the toolbar based on the updated Arrow showing/hiding.
				if (!showArrowOneBox.Active && !showArrowTwoBox.Active)
				{
					if (showOtherArrowOptions)
					{
						tb.Remove(arrowSizeLabel);
						tb.Remove(arrowSizeMinus);
						tb.Remove(arrowSize);
						tb.Remove(arrowSizePlus);
						tb.Remove(arrowAngleOffsetLabel);
						tb.Remove(arrowAngleOffsetMinus);
						tb.Remove(arrowAngleOffset);
						tb.Remove(arrowAngleOffsetPlus);
						tb.Remove(arrowLengthOffsetLabel);
						tb.Remove(arrowLengthOffsetMinus);
						tb.Remove(arrowLengthOffset);
						tb.Remove(arrowLengthOffsetPlus);

						showOtherArrowOptions = false;
					}
				}
				else
				{
					if (!showOtherArrowOptions)
					{
						tb.Add(arrowSizeLabel);
						tb.Add(arrowSizeMinus);
						tb.Add(arrowSize);
						tb.Add(arrowSizePlus);
						tb.Add(arrowAngleOffsetLabel);
						tb.Add(arrowAngleOffsetMinus);
						tb.Add(arrowAngleOffset);
						tb.Add(arrowAngleOffsetPlus);
						tb.Add(arrowLengthOffsetLabel);
						tb.Add(arrowLengthOffsetMinus);
						tb.Add(arrowLengthOffset);
						tb.Add(arrowLengthOffsetPlus);

						showOtherArrowOptions = true;
					}
				}

				CurveEngine selEngine = ee.SelectedCurveEngine;

				if (selEngine != null)
				{
					selEngine.Arrow2.Show = showArrowTwoBox.Active;

					DrawCurves(false, false, false);
				}
			};

			tb.AddWidgetItem(showArrowTwoBox);

			#endregion Show Arrows


			#region Arrow Size

			if (arrowSizeLabel == null)
			{
				arrowSizeLabel = new ToolBarLabel(string.Format(" {0}: ", Catalog.GetString("Size")));
			}

			if (arrowSizeMinus == null)
			{
				arrowSizeMinus = new ToolBarButton("Toolbar.MinusButton.png", "", Catalog.GetString("Decrease arrow size"));
				arrowSizeMinus.Clicked += new EventHandler(arrowSizeMinus_Clicked);
			}

			if (arrowSize == null)
			{
				arrowSize = new ToolBarComboBox(65, 7, true,
					"3", "4", "5", "6", "7", "8", "9", "10", "12", "15", "18",
					"20", "25", "30", "40", "50", "60", "70", "80", "90", "100");

				arrowSize.ComboBox.Changed += (o, e) =>
				{
					if (arrowSize.ComboBox.ActiveText.Length < 1)
					{
						//Ignore the change until the user enters something.
						return;
					}
					else
					{
						double newSize = 10d;

						if (arrowSize.ComboBox.ActiveText == "-")
						{
							//The user is trying to enter a negative value: change it to 1.
							newSize = 1d;
						}
						else
						{
							if (Double.TryParse(arrowSize.ComboBox.ActiveText, out newSize))
							{
								if (newSize < 1d)
								{
									//Less than 1: change it to 1.
									newSize = 1d;
								}
								else if (newSize > 100d)
								{
									//Greater than 100: change it to 100.
									newSize = 100d;
								}
							}
							else
							{
								//Not a number: wait until the user enters something.
								return;
							}
						}

						(arrowSize.ComboBox as Gtk.ComboBoxEntry).Entry.Text = newSize.ToString();

						CurveEngine selEngine = ee.SelectedCurveEngine;

						if (selEngine != null)
						{
							selEngine.Arrow1.ArrowSize = newSize;
							selEngine.Arrow2.ArrowSize = newSize;

							DrawCurves(false, false, false);
						}
					}
				};
			}

			if (arrowSizePlus == null)
			{
				arrowSizePlus = new ToolBarButton("Toolbar.PlusButton.png", "", Catalog.GetString("Increase arrow size"));
				arrowSizePlus.Clicked += new EventHandler(arrowSizePlus_Clicked);
			}

			#endregion Arrow Size


			#region Angle Offset

			if (arrowAngleOffsetLabel == null)
			{
				arrowAngleOffsetLabel = new ToolBarLabel(string.Format(" {0}: ", Catalog.GetString("Angle")));
			}

			if (arrowAngleOffsetMinus == null)
			{
				arrowAngleOffsetMinus = new ToolBarButton("Toolbar.MinusButton.png", "", Catalog.GetString("Decrease angle offset"));
				arrowAngleOffsetMinus.Clicked += new EventHandler(arrowAngleOffsetMinus_Clicked);
			}

			if (arrowAngleOffset == null)
			{
				arrowAngleOffset = new ToolBarComboBox(65, 9, true,
					"-30", "-25", "-20", "-15", "-10", "-5", "0", "5", "10", "15", "20", "25", "30");

				arrowAngleOffset.ComboBox.Changed += (o, e) =>
				{
					if (arrowAngleOffset.ComboBox.ActiveText.Length < 1)
					{
						//Ignore the change until the user enters something.
						return;
					}
					else if (arrowAngleOffset.ComboBox.ActiveText == "-")
					{
						//The user is trying to enter a negative value: ignore the change until the user enters more.
						return;
					}
					else
					{
						double newAngle = 15d;

						if (Double.TryParse(arrowAngleOffset.ComboBox.ActiveText, out newAngle))
						{
							if (newAngle < -89d)
							{
								//Less than -89: change it to -89.
								newAngle = -89d;
							}
							else if (newAngle > 89d)
							{
								//Greater than 89: change it to 89.
								newAngle = 89d;
							}
						}
						else
						{
							//Not a number: wait until the user enters something.
							return;
						}

						(arrowAngleOffset.ComboBox as Gtk.ComboBoxEntry).Entry.Text = newAngle.ToString();

						CurveEngine selEngine = ee.SelectedCurveEngine;

						if (selEngine != null)
						{
							selEngine.Arrow1.AngleOffset = newAngle;
							selEngine.Arrow2.AngleOffset = newAngle;

							DrawCurves(false, false, false);
						}
					}
				};
			}

			if (arrowAngleOffsetPlus == null)
			{
				arrowAngleOffsetPlus = new ToolBarButton("Toolbar.PlusButton.png", "", Catalog.GetString("Increase angle offset"));
				arrowAngleOffsetPlus.Clicked += new EventHandler(arrowAngleOffsetPlus_Clicked);
			}

			#endregion Angle Offset


			#region Length Offset

			if (arrowLengthOffsetLabel == null)
			{
				arrowLengthOffsetLabel = new ToolBarLabel(string.Format(" {0}: ", Catalog.GetString("Length")));
			}

			if (arrowLengthOffsetMinus == null)
			{
				arrowLengthOffsetMinus = new ToolBarButton("Toolbar.MinusButton.png", "", Catalog.GetString("Decrease length offset"));
				arrowLengthOffsetMinus.Clicked += new EventHandler(arrowLengthOffsetMinus_Clicked);
			}

			if (arrowLengthOffset == null)
			{
				arrowLengthOffset = new ToolBarComboBox(65, 8, true,
					"-30", "-25", "-20", "-15", "-10", "-5", "0", "5", "10", "15", "20", "25", "30");

				arrowLengthOffset.ComboBox.Changed += (o, e) =>
				{
					if (arrowLengthOffset.ComboBox.ActiveText.Length < 1)
					{
						//Ignore the change until the user enters something.
						return;
					}
					else if (arrowLengthOffset.ComboBox.ActiveText == "-")
					{
						//The user is trying to enter a negative value: ignore the change until the user enters more.
						return;
					}
					else
					{
						double newLength = 10d;

						if (Double.TryParse(arrowLengthOffset.ComboBox.ActiveText, out newLength))
						{
							if (newLength < -100d)
							{
								//Less than -100: change it to -100.
								newLength = -100d;
							}
							else if (newLength > 100d)
							{
								//Greater than 100: change it to 100.
								newLength = 100d;
							}
						}
						else
						{
							//Not a number: wait until the user enters something.
							return;
						}

						(arrowLengthOffset.ComboBox as Gtk.ComboBoxEntry).Entry.Text = newLength.ToString();

						CurveEngine selEngine = ee.SelectedCurveEngine;

						if (selEngine != null)
						{
							selEngine.Arrow1.LengthOffset = newLength;
							selEngine.Arrow2.LengthOffset = newLength;

							DrawCurves(false, false, false);
						}
					}
				};
			}

			if (arrowLengthOffsetPlus == null)
			{
				arrowLengthOffsetPlus = new ToolBarButton("Toolbar.PlusButton.png", "", Catalog.GetString("Increase length offset"));
				arrowLengthOffsetPlus.Clicked += new EventHandler(arrowLengthOffsetPlus_Clicked);
			}

			#endregion Length Offset


			if (showOtherArrowOptions)
			{
				tb.Add(arrowSizeLabel);
				tb.Add(arrowSizeMinus);
				tb.Add(arrowSize);
				tb.Add(arrowSizePlus);
				tb.Add(arrowAngleOffsetLabel);
				tb.Add(arrowAngleOffsetMinus);
				tb.Add(arrowAngleOffset);
				tb.Add(arrowAngleOffsetPlus);
				tb.Add(arrowLengthOffsetLabel);
				tb.Add(arrowLengthOffsetMinus);
				tb.Add(arrowLengthOffset);
				tb.Add(arrowLengthOffsetPlus);
			}
		}

		/// <summary>
		/// Set the Arrow options for the current curve to their respective values in the toolbar.
		/// </summary>
		private void SetArrowOptions()
		{
			CurveEngine selEngine = ee.SelectedCurveEngine;

			if (selEngine != null)
			{
				selEngine.Arrow1.Show = showArrowOneBox.Active;
				selEngine.Arrow2.Show = showArrowTwoBox.Active;

				showOtherArrowOptions = showArrowOneBox.Active || showArrowTwoBox.Active;

				if (showOtherArrowOptions)
				{
					Double.TryParse((arrowSize.ComboBox as Gtk.ComboBoxEntry).Entry.Text, out selEngine.Arrow1.ArrowSize);
					Double.TryParse((arrowAngleOffset.ComboBox as Gtk.ComboBoxEntry).Entry.Text, out selEngine.Arrow1.AngleOffset);
					Double.TryParse((arrowLengthOffset.ComboBox as Gtk.ComboBoxEntry).Entry.Text, out selEngine.Arrow1.LengthOffset);

					selEngine.Arrow1.ArrowSize = Utility.Clamp(selEngine.Arrow1.ArrowSize, 1d, 100d);
					selEngine.Arrow2.ArrowSize = selEngine.Arrow1.ArrowSize;
					selEngine.Arrow1.AngleOffset = Utility.Clamp(selEngine.Arrow1.AngleOffset, -89d, 89d);
					selEngine.Arrow2.AngleOffset = selEngine.Arrow1.AngleOffset;
					selEngine.Arrow1.LengthOffset = Utility.Clamp(selEngine.Arrow1.LengthOffset, -100d, 100d);
					selEngine.Arrow2.LengthOffset = selEngine.Arrow1.LengthOffset;
				}
			}
		}

		/// <summary>
		/// Set the Arrow options in the toolbar to their respective values for the current curve.
		/// </summary>
		private void SetToolbarArrowOptions()
		{
			CurveEngine selEngine = ee.SelectedCurveEngine;

			if (selEngine != null)
			{
				showArrowOneBox.Active = selEngine.Arrow1.Show;
				showArrowTwoBox.Active = selEngine.Arrow2.Show;

				if (showOtherArrowOptions)
				{
					(arrowSize.ComboBox as Gtk.ComboBoxEntry).Entry.Text = selEngine.Arrow1.ArrowSize.ToString();
					(arrowAngleOffset.ComboBox as Gtk.ComboBoxEntry).Entry.Text = selEngine.Arrow1.AngleOffset.ToString();
					(arrowLengthOffset.ComboBox as Gtk.ComboBoxEntry).Entry.Text = selEngine.Arrow1.LengthOffset.ToString();
				}
			}
		}


		#region ToolbarEventHandlers

		protected override void PlusButtonClickedEvent(object o, EventArgs args)
		{
			base.PlusButtonClickedEvent(o, args);

			DrawCurves(false, false, false);
		}

		protected override void MinusButtonClickedEvent(object o, EventArgs args)
		{
			base.MinusButtonClickedEvent(o, args);

			DrawCurves(false, false, false);
		}

		void arrowSizeMinus_Clicked(object sender, EventArgs e)
		{
			double newSize = 10d;

			if (Double.TryParse(arrowSize.ComboBox.ActiveText, out newSize))
			{
				--newSize;

				if (newSize < 1d)
				{
					newSize = 1d;
				}
			}
			else
			{
				newSize = 10d;
			}

			(arrowSize.ComboBox as Gtk.ComboBoxEntry).Entry.Text = newSize.ToString();
		}

		void arrowSizePlus_Clicked(object sender, EventArgs e)
		{
			double newSize = 10d;

			if (Double.TryParse(arrowSize.ComboBox.ActiveText, out newSize))
			{
				++newSize;

				if (newSize > 100d)
				{
					newSize = 100d;
				}
			}
			else
			{
				newSize = 10d;
			}

			(arrowSize.ComboBox as Gtk.ComboBoxEntry).Entry.Text = newSize.ToString();
		}

		void arrowAngleOffsetMinus_Clicked(object sender, EventArgs e)
		{
			double newAngle = 0d;

			if (Double.TryParse(arrowAngleOffset.ComboBox.ActiveText, out newAngle))
			{
				--newAngle;

				if (newAngle < -89d)
				{
					newAngle = -89d;
				}
			}
			else
			{
				newAngle = 0d;
			}

			(arrowAngleOffset.ComboBox as Gtk.ComboBoxEntry).Entry.Text = newAngle.ToString();
		}

		void arrowAngleOffsetPlus_Clicked(object sender, EventArgs e)
		{
			double newAngle = 0d;

			if (Double.TryParse(arrowAngleOffset.ComboBox.ActiveText, out newAngle))
			{
				++newAngle;

				if (newAngle > 89d)
				{
					newAngle = 89d;
				}
			}
			else
			{
				newAngle = 0d;
			}

			(arrowAngleOffset.ComboBox as Gtk.ComboBoxEntry).Entry.Text = newAngle.ToString();
		}

		void arrowLengthOffsetMinus_Clicked(object sender, EventArgs e)
		{
			double newLength = 10d;

			if (Double.TryParse(arrowLengthOffset.ComboBox.ActiveText, out newLength))
			{
				--newLength;

				if (newLength < -100d)
				{
					newLength = -100d;
				}
			}
			else
			{
				newLength = 10d;
			}

			(arrowLengthOffset.ComboBox as Gtk.ComboBoxEntry).Entry.Text = newLength.ToString();
		}

		void arrowLengthOffsetPlus_Clicked(object sender, EventArgs e)
		{
			double newLength = 10d;

			if (Double.TryParse(arrowLengthOffset.ComboBox.ActiveText, out newLength))
			{
				++newLength;

				if (newLength > 100d)
				{
					newLength = 100d;
				}
			}
			else
			{
				newLength = 10d;
			}

			(arrowLengthOffset.ComboBox as Gtk.ComboBoxEntry).Entry.Text = newLength.ToString();
		}

		#endregion ToolbarEventHandlers


		protected override Rectangle DrawShape (Rectangle rect, Layer l, bool drawControlPoints)
		{
			Document doc = PintaCore.Workspace.ActiveDocument;

			Rectangle? dirty = null;

			using (Context g = new Context(l.Surface))
			{
				g.AppendPath(doc.Selection.SelectionPath);
				g.FillRule = FillRule.EvenOdd;
				g.Clip();

				ee.ActiveCurveEngine.AntiAliasing = UseAntialiasing;

				g.Antialias = UseAntialiasing ? Antialias.Subpixel : Antialias.None;

				g.SetDash(DashPatternBox.GenerateDashArray(ee.ActiveCurveEngine.DashPattern, BrushWidth), 0.0);

				g.LineWidth = BrushWidth;

				//Draw the curves.
				for (int n = 0; n < ee.CEngines.Count; ++n)
				{
					List<ControlPoint> controlPoints = ee.CEngines[n].ControlPoints;

					if (controlPoints.Count > 0)
					{
						//Generate the points that make up the curve.
						ee.CEngines[n].GenerateCardinalSplinePolynomialCurvePoints();

						//Expand the invalidation rectangle as necessary.
						dirty = dirty.UnionRectangles(g.DrawPolygonal(ee.CEngines[n].GeneratedPoints, outline_color));
					}
				}

				g.SetDash(new double[] {}, 0.0);

				//Draw the arrows for all of the curves.
				for (int n = 0; n < ee.CEngines.Count; ++n)
				{
					PointD[] genPoints = ee.CEngines[n].GeneratedPoints;

					//For each curve currently being drawn/edited by the user.
					for (int i = 0; i < ee.CEngines[n].ControlPoints.Count; ++i)
					{
						if (ee.CEngines[n].Arrow1.Show)
						{
							if (genPoints.Length > 1)
							{
								dirty = dirty.UnionRectangles(ee.CEngines[n].Arrow1.Draw(g, outline_color, genPoints[0], genPoints[1]));
							}
						}

						if (ee.CEngines[n].Arrow2.Show)
						{
							if (genPoints.Length > 1)
							{
								dirty = dirty.UnionRectangles(ee.CEngines[n].Arrow2.Draw(g, outline_color,
									genPoints[genPoints.Length - 1], genPoints[genPoints.Length - 2]));
							}
						}
					}
				}

				if (drawControlPoints)
				{
					//Draw the control points for all of the curves.

					int controlPointSize = Math.Min(BrushWidth + 1, 5);
					double controlPointOffset = (double)controlPointSize / 2d;

					if (ee.SelectedPointIndex > -1)
					{
						//Draw a ring around the selected point.
						g.FillStrokedEllipse(
							new Rectangle(
								ee.SelectedPoint.Position.X - controlPointOffset * 4d,
								ee.SelectedPoint.Position.Y - controlPointOffset * 4d,
								controlPointOffset * 8d, controlPointOffset * 8d),
							ToolControl.FillColor, ToolControl.StrokeColor, 1);
					}

					//For each curve currently being drawn/edited by the user.
					for (int n = 0; n < ee.CEngines.Count; ++n)
					{
						List<ControlPoint> controlPoints = ee.CEngines[n].ControlPoints;

						//If the curve has one or more points.
						if (controlPoints.Count > 0)
						{
							//Draw the control points for the curve.
							for (int i = 0; i < controlPoints.Count; ++i)
							{
								//Skip drawing the hovered control point.
								if (ee.HoveredPointAsControlPoint > -1 && ee.HoverPoint.Distance(controlPoints[i].Position) < 1d)
								{
									continue;
								}

								// Draw the control point.
								g.FillStrokedEllipse(
									new Rectangle(
										controlPoints[i].Position.X - controlPointOffset,
										controlPoints[i].Position.Y - controlPointOffset,
										controlPointSize, controlPointSize),
									ToolControl.FillColor, ToolControl.StrokeColor, controlPointSize);
							}
						}
					}
					
					//Draw the hover point.
					if (!ee.ChangingTension && ee.HoverPoint.X > -1d)
					{
						g.FillStrokedEllipse(new Rectangle(
							ee.HoverPoint.X - controlPointOffset * 4d, ee.HoverPoint.Y - controlPointOffset * 4d,
							controlPointOffset * 8d, controlPointOffset * 8d), EditEngine.HoverColor, EditEngine.HoverColor, 1);
						g.FillStrokedEllipse(new Rectangle(
							ee.HoverPoint.X - controlPointOffset, ee.HoverPoint.Y - controlPointOffset,
							controlPointSize, controlPointSize), EditEngine.HoverColor, EditEngine.HoverColor, controlPointSize);
					}

					if (dirty != null)
					{
						//Inflate to accomodate for control points.
						dirty = dirty.Value.Inflate(controlPointSize * 8, controlPointSize * 8);
					}
				}
			}


			return dirty ?? new Rectangle(0d, 0d, 0d, 0d);
		}

		/// <summary>
		/// Draw all of the lines/curves that are currently being drawn/edited by the user.
		/// </summary>
		/// <param name="calculateOrganizedPoints">Whether or not to calculate the spatially organized
		/// points for mouse detection after drawing the curve.</param>
		/// <param name="finalize">Whether or not to finalize the drawing.</param>
		/// <param name="shiftKey">Whether or not the shift key is being pressed.</param>
		public void DrawCurves(bool calculateOrganizedPoints, bool finalize, bool shiftKey)
		{
			if (!surface_modified)
			{
				return;
			}

			Document doc = PintaCore.Workspace.ActiveDocument;

			Rectangle dirty;

			if (finalize)
			{
				doc.ToolLayer.Clear();

				ImageSurface undoSurface = null;
				// We only need to create a history item if there was a previous curve.
				if (ee.ActiveCurveEngine.ControlPoints.Count > 0) {
					undoSurface = doc.CurrentUserLayer.Surface.Clone ();
				}

				is_drawing = false;
				surface_modified = false;

				int previousSelectedPointIndex = ee.SelectedPointIndex;
				int previousSelectedPointCurveIndex = ee.SelectedPointCurveIndex;

				ee.SelectedPointIndex = -1;

				dirty = DrawShape(
					Utility.PointsToRectangle(shape_origin, new PointD(current_point.X, current_point.Y), shiftKey),
					doc.CurrentUserLayer, false);

				//Make sure that the undo surface isn't null and that there are actually points.
				if (undoSurface != null)
				{
					//Create a new CurvesHistoryItem so that the finalization of the curves can be undone.
					doc.History.PushNewItem(
						new CurvesHistoryItem(ee, Icon, Catalog.GetString("Line/Curve Finalized"),
							undoSurface, doc.CurrentUserLayer, previousSelectedPointIndex, previousSelectedPointCurveIndex));
				}

				//Clear out all of the old data.
				ee.CEngines = new CurveEngineCollection(true);
			}
			else
			{
				//Only calculate the hover point when there isn't a request to organize the generated points by spatial hashing.
				if (!calculateOrganizedPoints)
				{
					//Calculate the hover point, if any.

					int closestCurveIndex, closestPointIndex;
					PointD closestPoint;
					double closestDistance;

					OrganizedPointCollection.FindClosestPoint(ee.CEngines, current_point, out closestCurveIndex, out closestPointIndex, out closestPoint, out closestDistance);

					List<ControlPoint> controlPoints = ee.CEngines[closestCurveIndex].ControlPoints;

					//Determine if the user is hovering the mouse close enough to a line,
					//curve, or point that's currently being drawn/edited by the user.
					if (closestDistance < EditEngine.CurveClickStartingRange + BrushWidth * EditEngine.CurveClickThicknessFactor)
					{
						//User is hovering over a generated point on a line/curve.

						if (controlPoints.Count > closestPointIndex)
						{
							//Note: compare the current_point's distance here because it's the actual mouse position.
							if (current_point.Distance(controlPoints[closestPointIndex].Position) <	EditEngine.CurveClickStartingRange + BrushWidth * EditEngine.CurveClickThicknessFactor)
							{
								//Mouse hovering over a control point (on the "previous order" side of the point).

								ee.HoverPoint.X = controlPoints[closestPointIndex].Position.X;
								ee.HoverPoint.Y = controlPoints[closestPointIndex].Position.Y;
								ee.HoveredPointAsControlPoint = closestPointIndex;
							}
							else if (current_point.Distance(controlPoints[closestPointIndex - 1].Position) < EditEngine.CurveClickStartingRange + BrushWidth * EditEngine.CurveClickThicknessFactor)
							{
								//Mouse hovering over a control point (on the "following order" side of the point).

								ee.HoverPoint.X = controlPoints[closestPointIndex - 1].Position.X;
								ee.HoverPoint.Y = controlPoints[closestPointIndex - 1].Position.Y;
								ee.HoveredPointAsControlPoint = closestPointIndex - 1;
							}
						}

						if (ee.HoverPoint.X < 0d)
						{
							ee.HoverPoint.X = closestPoint.X;
							ee.HoverPoint.Y = closestPoint.Y;
						}
					}
				}



				doc.ToolLayer.Clear();

				dirty = DrawShape(
					Utility.PointsToRectangle(shape_origin, new PointD(current_point.X, current_point.Y), shiftKey),
					doc.ToolLayer, true);



				//Reset the hover point after each drawing.
				ee.HoverPoint = new PointD(-1d, -1d);
				ee.HoveredPointAsControlPoint = -1;
			}

			if (calculateOrganizedPoints)
			{
				//Organize the generated points for quick mouse interaction detection.

				//First, clear the previously organized points, if any.
				for (int n = 0; n < ee.CEngines.Count; ++n)
				{
					ee.CEngines[n].OrganizedPoints.ClearCollection();

					int pointIndex = 0;

					foreach (PointD p in ee.CEngines[n].GeneratedPoints)
					{
						ee.CEngines[n].OrganizedPoints.StoreAndOrganizePoint(new OrganizedPoint(new PointD(p.X, p.Y), pointIndex));

						//Keep track of the point's order in relation to the control points.
						if (ee.CEngines[n].ControlPoints.Count > pointIndex
							&& p.X == ee.CEngines[n].ControlPoints[pointIndex].Position.X
							&& p.Y == ee.CEngines[n].ControlPoints[pointIndex].Position.Y)
						{
							++pointIndex;
						}
					}
				}
			}

			// Increase the size of the dirty rect to account for antialiasing.
			if (UseAntialiasing)
			{
				dirty = dirty.Inflate(1, 1);
			}

			dirty = ((Rectangle?)dirty).UnionRectangles(last_dirty).Value;
			dirty = dirty.Clamp();
			doc.Workspace.Invalidate(dirty.ToGdkRectangle());
			last_dirty = dirty;
		}

		/// <summary>
		/// Calculate the modified position of current_point such that the angle between the adjacent point
		/// (if any) and current_point is snapped to the closest angle out of a certain number of angles.
		/// </summary>
		private void CalculateModifiedCurrentPoint()
		{
			CurveEngine selEngine = ee.SelectedCurveEngine;
			ControlPoint adjacentPoint;

			if (selEngine == null)
			{
				//Don't bother calculating a modified point because there is no selected curve.
				return;
			}
			else
			{
				if (ee.SelectedPointIndex > 0)
				{
					adjacentPoint = selEngine.ControlPoints[ee.SelectedPointIndex - 1];
				}
				else if (ee.SelectedPointIndex + 1 < selEngine.ControlPoints.Count)
				{
					adjacentPoint = selEngine.ControlPoints[ee.SelectedPointIndex + 1];
				}
				else
				{
					//Don't bother calculating a modified point because there is no reference point to align it with (there is only 1 point).
					return;
				}
			}

			PointD dir = new PointD(current_point.X - adjacentPoint.Position.X, current_point.Y - adjacentPoint.Position.Y);
			double theta = Math.Atan2(dir.Y, dir.X);
			double len = Math.Sqrt(dir.X * dir.X + dir.Y * dir.Y);

			theta = Math.Round(12 * theta / Math.PI) * Math.PI / 12;
			current_point = new PointD((adjacentPoint.Position.X + len * Math.Cos(theta)), (adjacentPoint.Position.Y + len * Math.Sin(theta)));
		}

		protected override void OnActivated()
		{
			PintaCore.Workspace.ActiveDocument.ToolLayer.Clear();
			PintaCore.Workspace.ActiveDocument.ToolLayer.Hidden = false;

			DrawCurves(false, false, false);

			PintaCore.Palette.PrimaryColorChanged += new EventHandler(Palette_PrimaryColorChanged);
			PintaCore.Palette.SecondaryColorChanged += new EventHandler(Palette_SecondaryColorChanged);

			base.OnActivated();
		}

		void Palette_PrimaryColorChanged(object sender, EventArgs e)
		{
			outline_color = PintaCore.Palette.PrimaryColor;

			DrawCurves(false, false, false);
		}

		void Palette_SecondaryColorChanged(object sender, EventArgs e)
		{
			fill_color = PintaCore.Palette.SecondaryColor;

			DrawCurves(false, false, false);
		}

		protected override void OnDeactivated()
		{
			PintaCore.Workspace.ActiveDocument.ToolLayer.Hidden = true;

			//Finalize the previous curve (if needed).
			DrawCurves(false, true, false);

			PintaCore.Palette.PrimaryColorChanged -= Palette_PrimaryColorChanged;
			PintaCore.Palette.SecondaryColorChanged += Palette_SecondaryColorChanged;

			base.OnDeactivated();
		}

		protected override void OnCommit()
		{
			PintaCore.Workspace.ActiveDocument.ToolLayer.Hidden = true;

			//Finalize the previous curve (if needed).
			DrawCurves(false, true, false);

			base.OnCommit();
		}

		protected override void OnKeyDown(Gtk.DrawingArea canvas, Gtk.KeyPressEventArgs args)
		{
			if (args.Event.Key == Gdk.Key.Delete)
			{
				if (ee.SelectedPointIndex > -1)
				{
					//Create a new CurveModifyHistoryItem so that the deletion of a control point can be undone.
					PintaCore.Workspace.ActiveDocument.History.PushNewItem(
                        new CurveModifyHistoryItem(ee, Icon, Catalog.GetString("Line/Curve Point Deleted")));


					List<ControlPoint> controlPoints = ee.SelectedCurveEngine.ControlPoints;


					undo_surface = PintaCore.Workspace.ActiveDocument.CurrentUserLayer.Surface.Clone();

					//Delete the selected point from the curve.
					controlPoints.RemoveAt(ee.SelectedPointIndex);

					//Set the newly selected point to be the median-most point on the curve, order-wise.
					if (controlPoints.Count > 0)
					{
						if (ee.SelectedPointIndex > controlPoints.Count / 2)
						{
							--ee.SelectedPointIndex;
						}
					}
					else
					{
						ee.SelectedPointIndex = -1;
					}

					surface_modified = true;

					ee.HoverPoint = new PointD(-1d, -1d);

					DrawCurves(true, false, false);
				}

				args.RetVal = true;
			}
			else if (args.Event.Key == Gdk.Key.Return)
			{
				//Finalize the previous curve (if needed).
				DrawCurves(false, true, false);

				args.RetVal = true;
			}
			else if (args.Event.Key == Gdk.Key.space)
			{
				ControlPoint selPoint = ee.SelectedPoint;

				if (selPoint != null)
				{
					//This can be assumed not to be null since selPoint was not null.
					CurveEngine selEngine = ee.SelectedCurveEngine;

					//Create a new CurveModifyHistoryItem so that the adding of a control point can be undone.
					PintaCore.Workspace.ActiveDocument.History.PushNewItem(
                        new CurveModifyHistoryItem(ee, Icon, Catalog.GetString("Line/Curve Point Added")));


					bool shiftKey = (args.Event.State & Gdk.ModifierType.ShiftMask) == Gdk.ModifierType.ShiftMask;
					bool ctrlKey = (args.Event.State & Gdk.ModifierType.ControlMask) == Gdk.ModifierType.ControlMask;

					PointD newPointPos;

					if (ctrlKey)
					{
						//Ctrl + space combo: same position as currently selected point.
						newPointPos = new PointD(selPoint.Position.X, selPoint.Position.Y);
					}
					else
					{
						shape_origin = new PointD(selPoint.Position.X, selPoint.Position.Y);

						if (shiftKey)
						{
							CalculateModifiedCurrentPoint();
						}

						//Space only: position of mouse (after any potential shift alignment).
						newPointPos = new PointD(current_point.X, current_point.Y);
					}

					//Place the new point on the outside-most end, order-wise.
					if ((double)ee.SelectedPointIndex < (double)selEngine.ControlPoints.Count / 2d)
					{
						ee.SelectedCurveEngine.ControlPoints.Insert(ee.SelectedPointIndex,
							new ControlPoint(new PointD(newPointPos.X, newPointPos.Y), EditEngine.DefaultMidPointTension));
					}
					else
					{
						ee.SelectedCurveEngine.ControlPoints.Insert(ee.SelectedPointIndex + 1,
							new ControlPoint(new PointD(newPointPos.X, newPointPos.Y), EditEngine.DefaultMidPointTension));

						++ee.SelectedPointIndex;
					}

					DrawCurves(true, false, shiftKey);
				}

				args.RetVal = true;
			}
			else if (args.Event.Key == Gdk.Key.Up)
			{
				//Make sure a control point is selected.
				if (ee.SelectedPointIndex > -1)
				{
					//Move the selected control point.
					ee.SelectedPoint.Position.Y -= 1d;

					DrawCurves(true, false, false);
				}

				args.RetVal = true;
			}
			else if (args.Event.Key == Gdk.Key.Down)
			{
				//Make sure a control point is selected.
				if (ee.SelectedPointIndex > -1)
				{
					//Move the selected control point.
					ee.SelectedPoint.Position.Y += 1d;

					DrawCurves(true, false, false);
				}

				args.RetVal = true;
			}
			else if (args.Event.Key == Gdk.Key.Left)
			{
				//Make sure a control point is selected.
				if (ee.SelectedPointIndex > -1)
				{
					if ((args.Event.State & Gdk.ModifierType.ControlMask) == Gdk.ModifierType.ControlMask)
					{
						//Change the selected control point to be the previous one, if applicable.
						if (ee.SelectedPointIndex > 0)
						{
							--ee.SelectedPointIndex;
						}
					}
					else
					{
						//Move the selected control point.
						ee.SelectedPoint.Position.X -= 1d;
					}

					DrawCurves(true, false, false);
				}

				args.RetVal = true;
			}
			else if (args.Event.Key == Gdk.Key.Right)
			{
				//Make sure a control point is selected.
				if (ee.SelectedPointIndex > -1)
				{
					if ((args.Event.State & Gdk.ModifierType.ControlMask) == Gdk.ModifierType.ControlMask)
					{
						//Change the selected control point to be the following one, if applicable.
						if (ee.SelectedPointIndex < ee.SelectedCurveEngine.ControlPoints.Count - 1)
						{
							++ee.SelectedPointIndex;
						}
					}
					else
					{
						//Move the selected control point.
						ee.SelectedPoint.Position.X += 1d;
					}

					DrawCurves(true, false, false);
				}

				args.RetVal = true;
			}
			else
			{
				base.OnKeyDown(canvas, args);
			}
		}
		
		protected override void OnKeyUp(Gtk.DrawingArea canvas, Gtk.KeyReleaseEventArgs args)
		{
			if (args.Event.Key == Gdk.Key.Delete || args.Event.Key == Gdk.Key.Return || args.Event.Key == Gdk.Key.space
				|| args.Event.Key == Gdk.Key.Up || args.Event.Key == Gdk.Key.Down
				|| args.Event.Key == Gdk.Key.Left || args.Event.Key == Gdk.Key.Right)
			{
				args.RetVal = true;
			}
			else
			{
				base.OnKeyUp(canvas, args);
			}
		}

		public override void AfterUndo()
		{
			surface_modified = true;


			CurveEngine actEngine = ee.ActiveCurveEngine;

			if (actEngine != null)
			{
				UseAntialiasing = actEngine.AntiAliasing;

				//Update the DashPatternBox to represent the current curve's DashPattern.
				(dashPBox.comboBox.ComboBox as Gtk.ComboBoxEntry).Entry.Text = actEngine.DashPattern;
			}


			//Draw the current state.
			DrawCurves(true, false, false);

			//Update the toolbar's arrow options.
			SetToolbarArrowOptions();

			base.AfterUndo();
		}

		public override void AfterRedo()
		{
			surface_modified = true;


			CurveEngine actEngine = ee.ActiveCurveEngine;

			if (actEngine != null)
			{
				UseAntialiasing = actEngine.AntiAliasing;

				//Update the DashPatternBox to represent the current curve's DashPattern.
				(dashPBox.comboBox.ComboBox as Gtk.ComboBoxEntry).Entry.Text = actEngine.DashPattern;
			}


			//Draw the current state.
			DrawCurves(true, false, false);

			//Update the toolbar's arrow options.
			SetToolbarArrowOptions();

			base.AfterRedo();
		}

		protected override void AfterBuildRasterization()
		{
			UseAntialiasing = true;

			base.AfterBuildRasterization();
		}

		protected override void OnMouseDown(Gtk.DrawingArea canvas, Gtk.ButtonPressEventArgs args, PointD point)
		{
			// If we are already drawing, ignore any additional mouse down events
			if (is_drawing)
				return;

			Document doc = PintaCore.Workspace.ActiveDocument;

			ee.LastMousePosition = point;

			shape_origin = new PointD(Utility.Clamp(point.X, 0, doc.ImageSize.Width - 1), Utility.Clamp(point.Y, 0, doc.ImageSize.Height - 1));
			current_point = shape_origin;

			bool shiftKey = (args.Event.State & Gdk.ModifierType.ShiftMask) == Gdk.ModifierType.ShiftMask;

			if (shiftKey)
			{
				CalculateModifiedCurrentPoint();
			}

			is_drawing = true;
			surface_modified = true;
			doc.ToolLayer.Hidden = false;

			outline_color = PintaCore.Palette.PrimaryColor;
			fill_color = PintaCore.Palette.SecondaryColor;


			//Right clicking changes tension.
			if (args.Event.Button == 1)
			{
				ee.ChangingTension = false;
			}
			else
			{
				ee.ChangingTension = true;
			}


			int closestCurveIndex, closestPointIndex;
			PointD closestPoint;
			double closestDistance;

            OrganizedPointCollection.FindClosestPoint(ee.CEngines, current_point,
				out closestCurveIndex, out closestPointIndex, out closestPoint, out closestDistance);

			bool clickedOnControlPoint = false;

			//Determine if the user clicked close enough to a line, curve, or point that's currently being drawn/edited by the user.
			if (closestDistance < EditEngine.CurveClickStartingRange + BrushWidth * EditEngine.CurveClickThicknessFactor)
			{
				//User clicked on a generated point on a line/curve.

				List<ControlPoint> controlPoints = ee.CEngines[closestCurveIndex].ControlPoints;

				//Note: compare the current_point's distance here because it's the actual mouse position.
				if (controlPoints.Count > closestPointIndex &&
					current_point.Distance(controlPoints[closestPointIndex].Position) < EditEngine.CurveClickStartingRange + BrushWidth * EditEngine.CurveClickThicknessFactor)
				{
					//User clicked on a control point (on the "previous order" side of the point).

					ee.ClickedWithoutModifying = true;

					ee.SelectedPointIndex = closestPointIndex;
					ee.SelectedPointCurveIndex = closestCurveIndex;

					clickedOnControlPoint = true;
				}
				else if (current_point.Distance(controlPoints[closestPointIndex - 1].Position) < EditEngine.CurveClickStartingRange + BrushWidth * EditEngine.CurveClickThicknessFactor)
				{
					//User clicked on a control point (on the "following order" side of the point).

					ee.ClickedWithoutModifying = true;

					ee.SelectedPointIndex = closestPointIndex - 1;
					ee.SelectedPointCurveIndex = closestCurveIndex;

					clickedOnControlPoint = true;
				}

				//Don't change anything here if right clicked.
				if (!ee.ChangingTension)
				{
					if (!clickedOnControlPoint)
					{
						//User clicked on a non-control point on a line/curve.

						//Create a new CurveModifyHistoryItem so that the adding of a control point can be undone.
						doc.History.PushNewItem(
                            new CurveModifyHistoryItem(ee, Icon, Catalog.GetString("Line/Curve Point Added")));

						controlPoints.Insert(closestPointIndex,
							new ControlPoint(new PointD(current_point.X, current_point.Y), EditEngine.DefaultMidPointTension));

						ee.SelectedPointIndex = closestPointIndex;
						ee.SelectedPointCurveIndex = closestCurveIndex;
					}
				}
			}

			bool ctrlKey = (args.Event.State & Gdk.ModifierType.ControlMask) == Gdk.ModifierType.ControlMask;

			//Create a new line/curve if the user simply clicks outside of any lines/curves or if the user control + clicks on an existing point.
			if (!ee.ChangingTension && ((ctrlKey && clickedOnControlPoint) || closestDistance >= EditEngine.CurveClickStartingRange + BrushWidth * EditEngine.CurveClickThicknessFactor))
			{
				PointD prevSelPoint;

				//First, store the position of the currently selected point.
				if (ee.SelectedPoint != null && ctrlKey)
				{
					prevSelPoint = new PointD(ee.SelectedPoint.Position.X, ee.SelectedPoint.Position.Y);
				}
				else
				{
					//This doesn't matter, other than the fact that it gets set to a value in order for the code to build.
					prevSelPoint = new PointD(0d, 0d);
				}



				//Next, take care of the old curve's data.

				//Finalize the previous curve (if needed).
				DrawCurves(false, true, false);

				CurveEngine actEngine = ee.ActiveCurveEngine;

				//Set the DashPattern for the finalized curve to be the same as the unfinalized curve's.
				actEngine.DashPattern = dashPBox.comboBox.ComboBox.ActiveText;

				//Verify that the user clicked inside the image bounds or that the user is
				//holding the Ctrl key (to ignore the Image bounds and draw a line on the edge).
				if ((point.X == shape_origin.X && point.Y == shape_origin.Y) || ctrlKey)
				{
					//Create a new CurvesHistoryItem so that the creation of a new curve can be undone.
					doc.History.PushNewItem(
                        new CurvesHistoryItem(ee, Icon, Catalog.GetString("Line/Curve Added"),
                            doc.CurrentUserLayer.Surface.Clone(), doc.CurrentUserLayer, ee.SelectedPointIndex, ee.SelectedPointCurveIndex));

					is_drawing = true;

					//Then create the first two points of the line/curve. The second point will follow the mouse around until released.
					if (ctrlKey && clickedOnControlPoint)
					{
						actEngine.ControlPoints.Add(new ControlPoint(new PointD(prevSelPoint.X, prevSelPoint.Y), EditEngine.DefaultEndPointTension));
						actEngine.ControlPoints.Add(
							new ControlPoint(new PointD(prevSelPoint.X + .01d, prevSelPoint.Y + .01d), EditEngine.DefaultEndPointTension));

						ee.ClickedWithoutModifying = false;
					}
					else
					{
						actEngine.ControlPoints.Add(new ControlPoint(new PointD(shape_origin.X, shape_origin.Y), EditEngine.DefaultEndPointTension));
						actEngine.ControlPoints.Add(
							new ControlPoint(new PointD(shape_origin.X + .01d, shape_origin.Y + .01d), EditEngine.DefaultEndPointTension));
					}

					ee.SelectedPointIndex = 1;
					ee.SelectedPointCurveIndex = 0;

					//Set the new curve's arrow options to be the same as the previous curve's.
					SetArrowOptions();

					//Set the DashPattern for the new curve to be the same as the previous curve's.
					actEngine.DashPattern = dashPBox.comboBox.ComboBox.ActiveText;
				}
			}

			//If the user right clicks outside of any lines/curves.
			if (closestDistance >= EditEngine.CurveClickStartingRange + BrushWidth * EditEngine.CurveClickThicknessFactor && ee.ChangingTension)
			{
				ee.ClickedWithoutModifying = true;
			}

			surface_modified = true;

			DrawCurves(false, false, shiftKey);
		}

		protected override void OnMouseUp(Gtk.DrawingArea canvas, Gtk.ButtonReleaseEventArgs args, PointD point)
		{
			is_drawing = false;

			ee.ChangingTension = false;

			DrawCurves(true, false, args.Event.IsShiftPressed());
		}

		protected override void OnMouseMove(object o, Gtk.MotionNotifyEventArgs args, PointD point)
		{
			Document doc = PintaCore.Workspace.ActiveDocument;

			current_point = new PointD(Utility.Clamp(point.X, 0, doc.ImageSize.Width - 1), Utility.Clamp(point.Y, 0, doc.ImageSize.Height - 1));

			bool shiftKey = (args.Event.State & Gdk.ModifierType.ShiftMask) == Gdk.ModifierType.ShiftMask;

			if (shiftKey)
			{
				CalculateModifiedCurrentPoint();
			}


			if (!is_drawing)
			{
				//Redraw everything to show a (temporary) highlighted control point when applicable.
				DrawCurves(false, false, shiftKey);
			}
			else
			{
				//Make sure a control point is selected.
				if (ee.SelectedPointIndex > -1)
				{
					if (ee.ClickedWithoutModifying)
					{
						//Create a new CurveModifyHistoryItem so that the modification of the curve can be undone.
						doc.History.PushNewItem(
                            new CurveModifyHistoryItem(ee, Icon, Catalog.GetString("Line/Curve Modified")));

						ee.ClickedWithoutModifying = false;
					}

					List<ControlPoint> controlPoints = ee.SelectedCurveEngine.ControlPoints;

					if (!ee.ChangingTension)
					{
						//Moving a control point.

						//Make sure the control point was moved.
						if (current_point.X != controlPoints[ee.SelectedPointIndex].Position.X
							|| current_point.Y != controlPoints[ee.SelectedPointIndex].Position.Y)
						{
							//Keep the tension value consistent.
							double movingPointTension = controlPoints[ee.SelectedPointIndex].Tension;

							//Update the control point's position.
							controlPoints.RemoveAt(ee.SelectedPointIndex);
							controlPoints.Insert(ee.SelectedPointIndex,
								new ControlPoint(new PointD(current_point.X, current_point.Y),
									movingPointTension));
						}
					}
					else
					{
						//Changing a control point's tension.

						//Unclamp the mouse position when changing tension.
						current_point = new PointD(point.X, point.Y);

						//Calculate the new tension based off of the movement of the mouse that's
						//perpendicular to the previous and following control points.

						PointD curPoint = controlPoints[ee.SelectedPointIndex].Position;
						PointD prevPoint, nextPoint;

						//Calculate the previous control point.
						if (ee.SelectedPointIndex > 0)
						{
							prevPoint = controlPoints[ee.SelectedPointIndex - 1].Position;
						}
						else
						{
							//There is none.
							prevPoint = curPoint;
						}

						//Calculate the following control point.
						if (ee.SelectedPointIndex < controlPoints.Count - 1)
						{
							nextPoint = controlPoints[ee.SelectedPointIndex + 1].Position;
						}
						else
						{
							//There is none.
							nextPoint = curPoint;
						}

						//The x and y differences are used as factors for the x and y change in the mouse position.
						double xDiff = prevPoint.X - nextPoint.X;
						double yDiff = prevPoint.Y - nextPoint.Y;
						double totalDiff = xDiff + yDiff;

						//Calculate the midpoint in between the previous and following points.
						PointD midPoint = new PointD((prevPoint.X + nextPoint.X) / 2d, (prevPoint.Y + nextPoint.Y) / 2d);

						double xChange = 0d, yChange = 0d;

						//Calculate the x change in the mouse position.
						if (curPoint.X <= midPoint.X)
						{
							xChange = current_point.X - ee.LastMousePosition.X;
						}
						else
						{
							xChange = ee.LastMousePosition.X - current_point.X;
						}

						//Calculate the y change in the mouse position.
						if (curPoint.Y <= midPoint.Y)
						{
							yChange = current_point.Y - ee.LastMousePosition.Y;
						}
						else
						{
							yChange = ee.LastMousePosition.Y - current_point.Y;
						}

						//Update the control point's tension.
						//Note: the difference factors are to be inverted for x and y change because this is perpendicular motion.
						controlPoints[ee.SelectedPointIndex].Tension +=
							Math.Round(Utility.Clamp((xChange * yDiff + yChange * xDiff) / totalDiff, -1d, 1d)) / 50d;

						//Restrict the new tension to range from 0d to 1d.
						controlPoints[ee.SelectedPointIndex].Tension =
							Utility.Clamp(controlPoints[ee.SelectedPointIndex].Tension, 0d, 1d);
					}

					surface_modified = true;

					DrawCurves(false, false, shiftKey);
				}
			}

			ee.LastMousePosition = current_point;
		}
	}
}
