
using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
#if UNDER_CE
using Microsoft.WindowsCE.Forms;
#endif

namespace Theresa {

public class BigButton : Control, ISkinned {

private Brush _BackBrush;
private Brush _ForeBrush;
protected Bitmap _OffScreenImage;
protected Graphics _OffScreenGraphics;

private static Font _Font;
private static ImageAttributes _MagentaKiller;

protected Bitmap _Picture;
public Bitmap Picture {
  get { return _Picture; }
  set { _Picture = value; }
}

private Skin _Skin;
public Skin Skin {
  get { return _Skin; }
  set { _Skin = value; }
}

public override Color BackColor {
  set { _BackBrush = new SolidBrush(value); }
}

public override string Text {
  get { return base.Text; }
  set { base.Text = value; CalcTextRect(); }
}

private Rectangle _RA;
private Rectangle _RUL;
private Rectangle _RUR;
private Rectangle _RBL;
private Rectangle _RBR;
private Rectangle _RIH;
private Rectangle _RIV;
private RectangleF _TextRect;

static BigButton() {
  _MagentaKiller = new ImageAttributes();
  _MagentaKiller.SetColorKey(Color.Magenta, Color.Magenta);  

  _Font = new Font(
    FontFamily.GenericSansSerif, 12, FontStyle.Bold
  );  

}

public BigButton(Skin skin) : base() {

  _RA = new Rectangle(0, 0, 0, 0);
  _RUL = new Rectangle(0, 0, 0, 0);
  _RUR = new Rectangle(0, 0, 0, 0);
  _RBL = new Rectangle(0, 0, 0, 0);
  _RBR = new Rectangle(0, 0, 0, 0);
  _RIH = new Rectangle(0, 0, 0, 0);
  _RIV = new Rectangle(0, 0, 0, 0);
  _TextRect = new RectangleF(0, 0, 0, 0);

  _BackBrush = new SolidBrush(Color.Gray);
  _ForeBrush = new SolidBrush(Color.White);

  _Skin = skin;

  this.Paint += new PaintEventHandler(OnPaint);
  this.Resize += new EventHandler(OnResize);
  if(System.Environment.OSVersion.Platform == PlatformID.WinCE) {
    OnResize(this, new EventArgs());
  }
}


protected override void OnPaintBackground(PaintEventArgs e) {
  // don't flicker
}

protected virtual void OnPaint(object sender, PaintEventArgs e) {
  if(_OffScreenGraphics == null) return;

  _OffScreenGraphics.Clear(Skin.BackgroundColor);

  // background
  _OffScreenGraphics.FillEllipse(_BackBrush, _RUL);
  _OffScreenGraphics.FillEllipse(_BackBrush, _RUR);
  _OffScreenGraphics.FillEllipse(_BackBrush, _RBL);
  _OffScreenGraphics.FillEllipse(_BackBrush, _RBR);
  _OffScreenGraphics.FillRectangle(_BackBrush, _RIH);
  _OffScreenGraphics.FillRectangle(_BackBrush, _RIV);

  if(_Picture != null) {
    // picture
    _OffScreenGraphics.DrawImage(
      _Picture, _RA, 
      0, 0, _Picture.Width, _Picture.Height, 
      GraphicsUnit.Pixel, _MagentaKiller
    );
  } else {
    // text
    _OffScreenGraphics.DrawString(Text, _Font, _ForeBrush, _TextRect);
  }
  e.Graphics.DrawImage(_OffScreenImage, 0, 0);
}

private void OnResize(object sender, EventArgs e) {
  if(Width == 0 || Height == 0) return;
  _OffScreenImage = new Bitmap(Width, Height);
  _OffScreenGraphics = Graphics.FromImage(_OffScreenImage);
  int w = ClientSize.Width;
  int h = ClientSize.Height;
  _RA = new Rectangle(0,    0,   w,   h);
  _RUL = new Rectangle(0,   0,   7,   7);
  _RUR = new Rectangle(w-8, 0,   7,   7);
  _RBL = new Rectangle(0,   h-8, 7,   7);
  _RBR = new Rectangle(w-8, h-8, 7,   7);
  _RIH = new Rectangle(0,   4,   w,   h-8);
  _RIV = new Rectangle(4,   0,   w-8, h);
  CalcTextRect();
}

private void CalcTextRect() {
  try {
  Graphics g = this.CreateGraphics();
  SizeF s = g.MeasureString(Text, _Font);
  int x = (ClientSize.Width - Convert.ToInt32(s.Width)) / 2;
  int y = (ClientSize.Height - Convert.ToInt32(s.Height)) / 2;
  if(x < 0) x = 0;
  if(y < 0) y = 0;
  _TextRect = new RectangleF(x, y, ClientSize.Width, ClientSize.Height);
  } catch(NullReferenceException) {
    _TextRect = new RectangleF(0, 0, 0, 0);
  }
}

public int MeasureString(string text) {
  Graphics g = this.CreateGraphics();
  SizeF s = g.MeasureString(text, _Font);
  return Convert.ToInt32(s.Width);  
}

} // class

} // namespace