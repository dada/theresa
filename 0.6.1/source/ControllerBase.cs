
using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
#if UNDER_CE
using Microsoft.WindowsCE.Forms;
#endif

namespace Theresa {

interface IMidiController {

int ID { get; set; }
Skin Skin { get; set; }
ControllerConfig Config { get; }
MidiDisplayMode DisplayMode { get; set; }
void ToggleDisplayMode();
int Value { get; set; }
int Minimum { get; set; }
int Maximum { get; set; }
event ValueChangedEventHandler ValueChanged;
event EventHandler Configure;

} // interface IMidiController

public class MidiControllerBase : Control, IMidiController, ISkinned {

protected int _ID;
public int ID {
  get { return _ID; }
  set { _ID = value; }
}

protected ControllerConfig _Config;
public ControllerConfig Config {
  get { return _Config; }
}

protected Skin _Skin;
public Skin Skin {
  get { return _Skin; }
  set { _Skin = value; }
}

protected MidiDisplayMode _DisplayMode = MidiDisplayMode.Numbers;
public MidiDisplayMode DisplayMode {
  get { return _DisplayMode; }
  set { _DisplayMode = value; }
}


protected int _Minimum = 0;
public int Minimum {
  get { return _Minimum; }
  set { 
    _Minimum = value;
    if(_Value < _Minimum) _Value = _Minimum;
  }
}

protected int _Maximum = 127;
public int Maximum {
  get { return _Maximum; }
  set { 
    _Maximum = value;
    if(_Value > _Maximum) _Value = _Maximum;
  }
}

protected int _Value = 0;
public virtual int Value {
  get { return _Value; }
  set { 
    try {
      _Value = value;
      if(_Value < _Minimum) _Value = _Minimum;
      if(_Value > _Maximum) _Value = _Maximum;
      OnValueChanged(this);
      Invalidate();
    } catch(Exception ex) {
      MessageBox.Show("Value.set " + ex.ToString());
    }
  }
}

public event ValueChangedEventHandler ValueChanged;
public event EventHandler Configure;

protected Bitmap _OffScreenImage;
protected Graphics _OffScreenGraphics;
protected static Font _SmallFont;

protected Rectangle _LcdRect;

static MidiControllerBase() {
  _SmallFont = new Font(
    FontFamily.GenericSansSerif, 7, FontStyle.Regular
  );  
}

public MidiControllerBase(Skin skin) : base() {
  _Skin = skin;
  _Config = new ControllerConfig();
  this.Paint += new PaintEventHandler(OnPaint);
  this.Resize += new EventHandler(OnResize);
  InitGraphics();
}

protected override void Dispose(bool disposing) {
  base.Dispose(disposing);
}

protected void OnValueChanged(object sender) {
  if(ValueChanged != null) {
    ValueChanged(sender, new EventArgs());
  }
}

protected bool ProcessMouseDown(object sender, MouseEventArgs e) {
  if(_LcdRect.Contains(e.X, e.Y)) {
    if(Configure != null) {
      Configure(sender, new EventArgs());
    }
    return true;
  } else {
    return false;
  }
}

protected void InitGraphics() {
  if(Width == 0 || Height == 0) return;
  _OffScreenImage = new Bitmap(Width, Height);
  _OffScreenGraphics = Graphics.FromImage(_OffScreenImage);  
  _LcdRect = new Rectangle(0, 0, ClientSize.Width, 10);
}

protected virtual void OnPaint(object sender, PaintEventArgs e) {
  if(_OffScreenGraphics == null) return;
  _OffScreenGraphics.Clear(Skin.BackgroundColor);  
  PaintController(_OffScreenGraphics);
  e.Graphics.DrawImage(_OffScreenImage, 0, 0);
}

protected virtual void PaintController(Graphics g) {
  // lcd rect
  g.FillRectangle(Skin.LcdBackground, _LcdRect);
  g.DrawRectangle(Skin.LcdBorder, _LcdRect);
  if(_DisplayMode == MidiDisplayMode.Name) {
    // name
    g.DrawString(
      _Config.Name,
      _SmallFont, Skin.LcdForeground,
      _LcdRect.Left, _LcdRect.Top
    );
  } else {
    // cc + value
    string v = String.Empty;
    v = _Config.CC.ToString() + ": " + _Value.ToString();
    g.DrawString(
      v,
      _SmallFont, Skin.LcdForeground,
      _LcdRect.Left, _LcdRect.Top
    );
  }
}

protected override void OnPaintBackground(PaintEventArgs e) {
  // don't flicker
}

private void OnResize(object sender, EventArgs e) {
  InitGraphics();
}

public void ToggleDisplayMode() {
  _DisplayMode = 
    _DisplayMode == MidiDisplayMode.Name
      ? MidiDisplayMode.Numbers
      : MidiDisplayMode.Name;
  Invalidate();
}

} // class MidiControllerBase

public enum MidiDisplayMode {
  Name,
  Numbers
}

public class ControllerConfig {

private string _Name = String.Empty;
public string Name {
  get { return _Name; }
  set { _Name = value; }
}

private int _CC = 0;
public int CC {
  get { return _CC; }
  set { _CC = value; }
}

} // class


} // namespace