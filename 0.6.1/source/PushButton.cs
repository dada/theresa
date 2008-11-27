
using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
#if UNDER_CE
using Microsoft.WindowsCE.Forms;
#endif

namespace Theresa {

public class PushButton : MidiControllerBase, IMidiController, ISkinned {

private Brush _BlackBrush;
private ImageAttributes _MagentaKiller;

public PushButton(Skin skin) : base(skin) {

  Width = 36;
  Height = 45;

  _BlackBrush = new SolidBrush(Color.Black);

  _MagentaKiller = new ImageAttributes();
  _MagentaKiller.SetColorKey(Color.Magenta, Color.Magenta);  

  this.MouseDown += new MouseEventHandler(OnMouseDown);
  InitGraphics();
}

protected override void PaintController(Graphics g) {

  base.PaintController(g);

  if(_Value == _Minimum) {
    g.DrawImage(
      Skin.ButtonOff, 
      new Rectangle(1, 11, ClientSize.Width-2, ClientSize.Height-11), 
      0, 0, Skin.ButtonOff.Width, Skin.ButtonOff.Height, 
      GraphicsUnit.Pixel, _MagentaKiller
    );
  } else {
    g.DrawImage(
      Skin.ButtonOn, 
      new Rectangle(1, 11, ClientSize.Width-2, ClientSize.Height-11), 
      0, 0, Skin.ButtonOn.Width, Skin.ButtonOn.Height, 
      GraphicsUnit.Pixel, _MagentaKiller
    );
  }
}

protected virtual void OnMouseDown(object sender, MouseEventArgs e) {
  if(!base.ProcessMouseDown(sender, e)) {
    if(_Value == _Minimum) {
      Value = _Maximum;
    } else {
      Value = _Minimum;
    }
    Invalidate();
  }
}

} // class

} // namespace