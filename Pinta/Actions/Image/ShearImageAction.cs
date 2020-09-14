// 
// ResizeImageAction.cs
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
using Pinta.Dialogs;

namespace Pinta.Actions.Image
{
    class ShearImageAction : IActionHandler
	{
		#region IActionHandler Members
		public void Initialize()
		{
			PintaCore.Actions.Image.Shear.Activated += Activated;
		}

		public void Uninitialize()
		{
			PintaCore.Actions.Image.Shear.Activated -= Activated;
		}
		#endregion

		private void Activated(object sender, EventArgs e)
		{
			ShearImageDialog dialog = new ShearImageDialog();

            dialog.WindowPosition = Gtk.WindowPosition.CenterOnParent;

            int response = dialog.Run();

            if (response == (int)Gtk.ResponseType.Ok)
                dialog.SaveChanges();

            dialog.Destroy();
        }
	}
}
