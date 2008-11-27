
using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace Theresa {

public class ControllerConfigScreen : ScreenBase {

private ControllerConfig _Config;
public ControllerConfig Config {
  get { return _Config; }
  set { 
    _Config = value; 
    if(_Config != null) {
      BuildAdditionalProperties();
      UpdateButtons();
    }
  }
}


private Type _ControllerType;
public Type ControllerType {
  get { return _ControllerType; }
  set { 
    _ControllerType = value; 
    BuildAdditionalProperties();
  }
}

private BigButton _CurrentName;
private BigButton _CurrentCC;
private BigButton _CurrentCC2;
private BigButton _OK;
private BigButton _Cancel;

private ArrayList _AdditionalButtons;

public ControllerConfigScreen(Skin skin) : base(skin) {

  _AdditionalButtons = new ArrayList();

  _CurrentName = new BigButton(Skin);
  _CurrentName.Bounds = new Rectangle(10, 10, ClientSize.Width-20, 32);
  _CurrentName.Text = "Name: ";
  _CurrentName.Click += new EventHandler(EditName);
  _CurrentName.Parent = this;
  
  _CurrentCC = new BigButton(Skin);
  _CurrentCC.Bounds = new Rectangle(10, 50, ClientSize.Width-20, 32);
  _CurrentCC.Text = "CC: ";
  _CurrentCC.Click += new EventHandler(EditCC);
  _CurrentCC.Parent = this;

  _CurrentCC2 = new BigButton(Skin);
  _CurrentCC2.Bounds = new Rectangle(10, 90, ClientSize.Width-20, 32);
  _CurrentCC2.Text = "CC: ";
  _CurrentCC2.Click += new EventHandler(EditCC2);
  _CurrentCC2.Parent = this;

  _OK = new BigButton(Skin);
  _OK.BackColor = Color.DarkGreen;
  _OK.Bounds = new Rectangle(10, 130, ClientSize.Width/2-15, 32);
  _OK.Text = "OK";
  _OK.Click += new EventHandler(OnOK);
  _OK.Parent = this;
  
  _Cancel = new BigButton(Skin);
  _Cancel.BackColor = Color.DarkRed;
  _Cancel.Bounds = new Rectangle(ClientSize.Width/2+5, 130, ClientSize.Width/2-15, 32);
  _Cancel.Text = "Cancel";
  _Cancel.Click += new EventHandler(OnCancel);
  _Cancel.Parent = this;
}

private void BuildAdditionalProperties() {

  foreach(BigButton b in _AdditionalButtons) {
    b.Parent = null;
  }
  _AdditionalButtons.Clear();

  int y = 130;
  foreach(AdditionalProperty prop in _Config.AdditionalProperties.Values) {
    BigButton b = new BigButton(Skin);
    b.Bounds = new Rectangle(10, y, ClientSize.Width-20, 32);
    b.Name = prop.Name;
    b.Text = prop.Label;
    b.Click += new EventHandler(EditAdditionalProperty);
    b.Parent = this;
    _AdditionalButtons.Add(b);
    y += 40;
  }
  _OK.Top = y;
  _Cancel.Top = y;
}

private void UpdateButtons() {
  if(_CurrentName != null) _CurrentName.Text = "Name: " + _Config.Name;
  if(_CurrentCC != null) {
    string cc = _Config.CC == 0 ? "none" : _Config.CC.ToString();
    if(_Config.CCMeaning != String.Empty) {
	  _CurrentCC.Text = "CC (" + _Config.CCMeaning + "): " + cc;
	} else {
      _CurrentCC.Text = "CC: " + cc;
    }
  }
  if(_CurrentCC2 != null) {
    string cc = _Config.CC2 == 0 ? "none" : _Config.CC2.ToString();
    if(_Config.CC2Meaning != String.Empty) {
	  _CurrentCC2.Text = "CC (" + _Config.CC2Meaning + "): " + cc;
	} else {
      _CurrentCC2.Text = "CC: " + cc;
    }
  }
  foreach(BigButton b in _AdditionalButtons) {
    b.Text = _Config.AdditionalProperties[b.Name].Label + ": " + _Config.AdditionalProperties[b.Name].Value;
  }
}

private void EditName(object sender, EventArgs e) {
  TextInputScreen editor = Global.GetTextInputScreen(Skin);
  editor.String = _Config.Name;
  DialogResult r = editor.ShowDialog();
  if(r == DialogResult.OK) {
    _Config.Name = editor.String;
    UpdateButtons();
  }
}

private void EditCC(object sender, EventArgs e) {
  NumberInputScreen editor = Global.GetNumberInputScreen(Skin);
  editor.Number = _Config.CC;
  DialogResult r = editor.ShowDialog();
  if(r == DialogResult.OK) {
    _Config.CC = editor.Number;
    UpdateButtons();
  }
}

private void EditCC2(object sender, EventArgs e) {
  NumberInputScreen editor = Global.GetNumberInputScreen(Skin);
  editor.Number = _Config.CC2;
  DialogResult r = editor.ShowDialog();
  if(r == DialogResult.OK) {
    _Config.CC2 = editor.Number;
    UpdateButtons();
  }
}

private void EditAdditionalProperty(object sender, EventArgs e) {
  NumberInputScreen editor = Global.GetNumberInputScreen(Skin);
  BigButton b = (BigButton) sender;
  editor.Number = _Config.AdditionalProperties[b.Name].Value;
  DialogResult r = editor.ShowDialog();
  if(r == DialogResult.OK) {
    int n = editor.Number;
    if(n > _Config.AdditionalProperties[b.Name].Max) {
      n = _Config.AdditionalProperties[b.Name].Max;
    }
    if(n < _Config.AdditionalProperties[b.Name].Min) {
      n = _Config.AdditionalProperties[b.Name].Min;
    }
    _Config.AdditionalProperties[b.Name].Value = n;
    UpdateButtons();
  }
}


private void OnOK(object sender, EventArgs e) {
  DialogResult = DialogResult.OK;
  Close();
}

private void OnCancel(object sender, EventArgs e) {
  DialogResult = DialogResult.Cancel;
  Close();
}


} // class

} // namespace