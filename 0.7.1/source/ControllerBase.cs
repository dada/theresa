
using System;
using System.Collections;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
#if UNDER_CE
using Microsoft.WindowsCE.Forms;
#endif

namespace Theresa {

interface IController {

int ID { get; set; }
Skin Skin { get; set; }
MidiDisplayMode DisplayMode { get; set; }
void ToggleDisplayMode();
event EventHandler Configure;
void LayoutLoaded();
string ToXml();
void FromXml(XmlNode n);

} // interface IController

interface IMidiController {

ControllerConfig Config { get; }
int Value { get; set; }
int Value2 { get; set; }
int Minimum { get; set; }
int Maximum { get; set; }
event ValueChangedEventHandler ValueChanged;

} // interface IMidiController

public class MidiControllerBase : Control, IController, IMidiController, ISkinned {

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

protected int _Value2 = 0;
public virtual int Value2 {
  get { return _Value2; }
  set { 
    try {
      _Value2 = value;
      if(_Value2 < _Minimum) _Value2 = _Minimum;
      if(_Value2 > _Maximum) _Value2 = _Maximum;
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

protected virtual bool ProcessMouseDown(object sender, MouseEventArgs e) {
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
  g.DrawString(
    GetLcdText(),
    _SmallFont, Skin.LcdForeground,
    _LcdRect.Left, _LcdRect.Top
  );
}

protected virtual string GetLcdText() {
  if(_DisplayMode == MidiDisplayMode.Name) {
    // name
    return _Config.Name;
  } else {
    // cc + value (flip/flop if has two?)
    string v = String.Empty;
    string n = _Config.CC == 0 ? "--" : _Config.CC.ToString();
    v = n + ": " + _Value.ToString();
    if(Width > 36 && _Config.CC2 != 0) { // what an hack...
      v += " / " + _Config.CC2.ToString() + ": " + _Value2.ToString();
    }
    return v;
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

public virtual string ToXml() {
  string xml = String.Format(
    "<Controller id=\"{0}\" name=\"{1}\" cc=\"{2}\" value=\"{3}\" cc2=\"{4}\" value2=\"{5}\" ",
    ID, Config.Name, 
    Config.CC, Value,
     Config.CC2, Value2
  );
  try {
    foreach(string n in Config.AdditionalProperties.Keys) {
      xml += String.Format("{0}=\"{1}\" ", 
        n, Config.AdditionalProperties[n].Value
      );
    }
  } catch(Exception ex) {
    Console.WriteLine("failed to save prop: " + ex.ToString());
  }
  xml += "/>";
  return xml;
}

public virtual void FromXml(XmlNode n) {
  if(n.Attributes["name"] != null) {
    Config.Name = n.Attributes["name"].Value;
  }
  if(n.Attributes["cc"] != null) {
    Config.CC = Convert.ToInt32(n.Attributes["cc"].Value);
  }
  if(n.Attributes["value"] != null) {
    Value = Convert.ToInt32(n.Attributes["value"].Value);
  }
  if(n.Attributes["cc2"] != null) {
    Config.CC2 = Convert.ToInt32(n.Attributes["cc2"].Value);
  }
  if(n.Attributes["value2"] != null) {
    Value2 = Convert.ToInt32(n.Attributes["value2"].Value);
  }
  foreach(string k in Config.AdditionalProperties.Keys) {
    if(n.Attributes[k] != null) {
      Config.AdditionalProperties[k].Value = Convert.ToInt32(n.Attributes[k].Value);
    }
  }
}

public virtual void LayoutLoaded() {
  // nothing to do
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

private string _CCMeaning = String.Empty;
public string CCMeaning {
  get { return _CCMeaning; }
  set { _CCMeaning = value; }
}

private int _CC2 = 0;
public int CC2 {
  get { return _CC2; }
  set { _CC2 = value; }
}

private string _CC2Meaning = String.Empty;
public string CC2Meaning {
  get { return _CC2Meaning; }
  set { _CC2Meaning = value; }
}

private AdditionalPropertyTable _AdditionalProperties;
public AdditionalPropertyTable AdditionalProperties {
  get { return _AdditionalProperties; }
}

public ControllerConfig() {
  _AdditionalProperties = new AdditionalPropertyTable();
}

public void CopyTo(ControllerConfig target) {
  target.Name = _Name;
  target.CC = _CC;
  target.CCMeaning = _CCMeaning;
  target.CC2 = _CC2;
  target.CC2Meaning = _CC2Meaning;
  target.AdditionalProperties.Clear();
  foreach(AdditionalProperty prop in _AdditionalProperties.Values) {
    AdditionalProperty targetprop = new AdditionalProperty();
    prop.CopyTo(targetprop);
    target.AdditionalProperties.Add(targetprop);
  }
}

} // class

public class AdditionalProperty {

private string _Name;
public string Name {
  get { return _Name; }
  set { _Name = value; }
}

private string _Label;
public string Label {
  get { return _Label; }
  set { _Label = value; }
}

private int _Min;
public int Min {
  get { return _Min; }
  set { _Min = value; }
}

private int _Max;
public int Max {
  get { return _Max; }
  set { _Max = value; }
}

private int _Value;
public int Value {
  get { return _Value; }
  set { _Value = value; }
}

public void CopyTo(AdditionalProperty target) {
  target.Name = _Name;
  target.Label = _Label;
  target.Min= _Min;
  target.Max = _Max;
  target.Value = _Value;
}

} // class

public class AdditionalPropertyTable : Hashtable {

public AdditionalProperty this[string name] {
  get { return (AdditionalProperty) base[name]; }
}

public void Add(AdditionalProperty item) {
  base.Add(item.Name, item);
}


public void Remove(string name) {
  base.Remove(name);
}

public void Remove(AdditionalProperty item) {
  base.Remove(item.Name);
}

} // class

} // namespace
