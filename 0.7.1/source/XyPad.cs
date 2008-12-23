
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

public class XyPad : MidiControllerBase, IMidiController, ISkinned {

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

protected bool _IsMoving = false;
protected int _StartX;
protected int _StartY;
protected int _StartVX;
protected int _StartVY;

protected Rectangle _SelRect;
protected Rectangle _BackRect;
protected Brush _BackBrush;
protected Brush _SelBrush;

protected double _TempX;
protected double _TempY;
protected double _StepX;
protected double _StepY;

protected Timer BackToRest;

protected ImageAttributes _MagentaKiller;

public override int Value {
  set {
    base.Value = value;
    CalcSelRect();
  }
}

public override int Value2 {
  set {
    base.Value2 = value;
    CalcSelRect();
  }
}

public XyPad(Skin skin) : base(skin) {

  AdditionalProperty propSpeed = new AdditionalProperty();
  propSpeed.Name = "speed";
  propSpeed.Label = "Speed";
  propSpeed.Min = 0;
  propSpeed.Max = 10;  
  propSpeed.Value = 0;
  _Config.AdditionalProperties.Add(propSpeed);

  AdditionalProperty propRestX = new AdditionalProperty();
  propRestX.Name = "restx";
  propRestX.Label = "Rest X";
  propRestX.Min = 0;
  propRestX.Max = 127;  
  propRestX.Value = 64;
  _Config.AdditionalProperties.Add(propRestX);

  AdditionalProperty propRestY = new AdditionalProperty();
  propRestY.Name = "resty";
  propRestY.Label = "Rest Y";
  propRestY.Min = 0;
  propRestY.Max = 127;  
  propRestY.Value = 64;
  _Config.AdditionalProperties.Add(propRestY);

  _Config.CCMeaning = "X";
  _Config.CC2Meaning = "Y";
  
  BackToRest = new Timer();
  BackToRest.Interval = 20;
  BackToRest.Tick += new EventHandler(BackToRest_Tick);

  Width = 138;
  Height = 138;

  _BackBrush = new SolidBrush(Color.Black);
  _SelBrush = new SolidBrush(Color.Red);
  _BackRect = new Rectangle(1, 11, Width-2, 127);
  _SelRect = new Rectangle(1, 30, 34, 30);
  _Value = 64;
  _Value2 = 64;
  CalcSelRect();
  
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
    Skin.XyPadBackground, _BackRect, 
    0, 0, Skin.XyPadBackground.Width, Skin.XyPadBackground.Height, 
    GraphicsUnit.Pixel, _MagentaKiller
  );

  // sel
  g.DrawImage(
    Skin.XyPadForeground, _SelRect, 
    0, 0, _SelRect.Width, _SelRect.Height, 
    GraphicsUnit.Pixel, _MagentaKiller
  );

}

protected virtual void OnMouseDown(object sender, MouseEventArgs e) {
  if(!base.ProcessMouseDown(sender, e)) {
    if(_SelRect.Contains(e.X, e.Y)) {
      _IsMoving = true;
      _StartX = e.X;
      _StartY = e.Y;
      _StartVX = Value;
      _StartVY = Value2;
      Capture = true;
      BackToRest.Enabled = false;
    }
  }
}

protected virtual void OnMouseUp(object sender, MouseEventArgs e) {
  _IsMoving = false;
  Capture = false;
  double speed = Convert.ToDouble(_Config.AdditionalProperties["speed"].Value);
  double restx = Convert.ToDouble(_Config.AdditionalProperties["restx"].Value);
  double resty = Convert.ToDouble(_Config.AdditionalProperties["resty"].Value);
  if(speed > 0.0) {
    double dist = Math.Sqrt(Math.Pow(restx - Convert.ToDouble(Value), 2.0) + Math.Pow(resty - Convert.ToDouble(Value2), 2.0));
    int steptime = Convert.ToInt32(dist * ((11.0 - speed) * 20.0) / 127.0);
    BackToRest.Interval = steptime;
    _StepX = (restx - Convert.ToDouble(Value)) / (11.0 - speed);
    _TempX = Value;
    _StepY = (resty - Convert.ToDouble(Value2)) / (11.0 - speed);
    _TempY = Value2;
    Console.WriteLine("X Value=" + Value + " Rest=" + restx + " Step=" + _StepX);
    Console.WriteLine("Y Value=" + Value2 + " Rest=" + resty + " Step=" + _StepY);
    BackToRest.Enabled = true;
  }
}

protected virtual void OnMouseMove(object sender, MouseEventArgs e) {
  if(_IsMoving) {
    Value = _StartVX + (e.X-_StartX);
    Value2 = _StartVY + (_StartY-e.Y);
  }
}

protected virtual void CalcSelRect() {
  _SelRect.X = Convert.ToInt32(_BackRect.X + (_Value) * (_BackRect.Width-_SelRect.Width) / (_Maximum-_Minimum));
  _SelRect.Y = Convert.ToInt32(_BackRect.Y + (_Maximum-_Value2) * (_BackRect.Height-_SelRect.Height) / (_Maximum-_Minimum));
}

protected virtual void BackToRest_Tick(object sender, EventArgs e) {
  Console.WriteLine("StepX=" + _StepX + " StepY=" + _StepY);
 _TempX += _StepX;
  _TempY += _StepY;
  Value = Convert.ToInt32(_TempX);
  Value2 = Convert.ToInt32(_TempY);
  Console.WriteLine("Value=" + Value + " Value2=" + Value2);
  CalcSelRect();
  if(Value == _Config.AdditionalProperties["restx"].Value 
  && Value2 == _Config.AdditionalProperties["resty"].Value) {
    BackToRest.Enabled = false;
  }
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