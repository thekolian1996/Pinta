// 
// ResizeImageDialog.cs
//  
// Author:
//       Mykola Franchuk <thekolian1996@gmail.com>
// 
// Copyright (c) 2020 Mykola Franchuk
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
using Gtk;
using Mono.Unix;
using Pinta.Core;

namespace Pinta.Dialogs
{
    class ShearImageDialog : Dialog
    {
		private SpinButton widthSpinner;
		private SpinButton heightSpinner;


		public ShearImageDialog() : base(Catalog.GetString("Shear Image"), PintaCore.Chrome.MainWindow,
											DialogFlags.Modal,
											Gtk.Stock.Cancel, Gtk.ResponseType.Cancel,
											Gtk.Stock.Ok, Gtk.ResponseType.Ok)
		{
			Build();




		}

		#region Public Methods
		public void SaveChanges()
		{
			PintaCore.Workspace.ShearImage(widthSpinner.Value/100, heightSpinner.Value/100);
		}

		#endregion



		#region Private Methods
		private void Build()
		{
			Icon = PintaCore.Resources.GetIcon("Menu.Image.Resize.png");
			WindowPosition = WindowPosition.CenterOnParent;

			DefaultWidth = 300;
			DefaultHeight = 200;

			widthSpinner = new SpinButton(1, 10000, 1);
			heightSpinner = new SpinButton(1, 10000, 1);

			const int spacing = 6;
			var main_vbox = new VBox() { Spacing = spacing, BorderWidth = 12 };

			var hbox_width = new HBox() { Spacing = spacing };
			hbox_width.PackStart(new Label(Catalog.GetString("Horizontal:")), false, false, 0);
			hbox_width.PackStart(widthSpinner, false, false, 0);
			hbox_width.PackStart(new Label(Catalog.GetString("pixels")), false, false, 0);
			main_vbox.PackStart(hbox_width, false, false, 0);

			var hbox_height = new HBox() { Spacing = spacing };
			hbox_height.PackStart(new Label(Catalog.GetString("Vertical:")), false, false, 0);
			hbox_height.PackStart(heightSpinner, false, false, 0);
			hbox_height.PackStart(new Label(Catalog.GetString("pixels")), false, false, 0);
			main_vbox.PackStart(hbox_height, false, false, 0);

			VBox.BorderWidth = 2;
			VBox.Add(main_vbox);

			ShowAll();
		}
		#endregion


	}
}
