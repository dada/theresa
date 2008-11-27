
using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
#if UNDER_CE
using Microsoft.WindowsCE.Forms;
#endif

namespace Theresa {

public class Fader : MidiControllerBase, IMidiController, ISkinned {

public string AppPath {
  get {
    if(System.Environment.OSVersion.Platform == PlatformID.WinCE) {
      return Path.GetDirectoryName( 
        System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase
      );
    } else {
      return ".";
    }
  }
}

private bool _IsMoving = false;
private int _StartY;
private int _StartV;

private Rectangle _SelRect;
private Rectangle _BackRect;
private Brush _BackBrush;
private Brush _SelBrush;

private ImageAttributes _MagentaKiller;

public override int Value {
  set {
    base.Value = value;
    base.Value2 = 127 - value;
    CalcSelRect();
  }
}

public Fader(Skin skin) : base(skin) {

  _Config.CCMeaning = "normal";
  _Config.CC2Meaning = "inverted";

  Width = 36;
  Height = 138;

  _BackBrush = new SolidBrush(Color.Black);
  _SelBrush = new SolidBrush(Color.Red);
  _BackRect = new Rectangle(1, 11, Width-2, 127);
  _SelRect = new Rectangle(1, 20, Width-2, 30);

  _MagentaKiller = new ImageAttributes();
  _MagentaKiller.SetColorKey(Color.Magenta, Color.Magenta);  

  this.MouseDown += new MouseEventHandler(OnMouseDown);
  this.MouseMove += new MouseEventHandler(OnMouseMove);
  this.MouseUp += new MouseEventHandler(OnMouseUp);
  CalcSelRect();
}

protected override void PaintController(Graphics g) {
  base.PaintController(g);

  // back
  g.DrawImage(
    Skin.FaderBackground, _BackRect, 
    0, 0, Skin.FaderBackground.Width, Skin.FaderBackground.Height, 
    GraphicsUnit.Pixel, _MagentaKiller
  );

  // sel
  g.DrawImage(
    Skin.FaderForeground, _SelRect, 
    0, 0, _SelRect.Width, _SelRect.Height, 
    GraphicsUnit.Pixel, _MagentaKiller
  );

}

protected virtual void OnMouseDown(object sender, MouseEventArgs e) {
  if(!base.ProcessMouseDown(sender, e)) {
    if(_SelRect.Contains(e.X, e.Y)) {
      _IsMoving = true;
      _StartY = e.Y;
      _StartV = Value;
      Capture = true;
    }
  }
}

protected void OnMouseUp(object sender, MouseEventArgs e) {
  _IsMoving = false;
  Capture = false;
}

protected void OnMouseMove(object sender, MouseEventArgs e) {
  if(_IsMoving) {
    Value = _StartV + (_StartY-e.Y);
  }
}

private void CalcSelRect() {
  _SelRect.Y = Convert.ToInt32(_BackRect.Y + (_Maximum-_Value) * (_BackRect.Height-_SelRect.Height) / (_Maximum-_Minimum));
}

protected int GetValueFromPosition(Point p) {
  int y = p.Y - _BackRect.Top;
  if(y < 0) y = 0;
  if(y > _BackRect.Height) y = _BackRect.Height;
  y = _BackRect.Height - y;
  double v;
  double r = 0.0;
  if(y == 0) {
    v = _Minimum;
  } else {
    r = y / Convert.ToDouble(_BackRect.Height);
    v = _Minimum + r * (_Maximum-_Minimum);
  }
  return Convert.ToInt32(v);
}

} // class

} // namespace