
using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
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
    if(_Config != null) UpdateButtons();
  }
}

private BigButton _CurrentName;
private BigButton _CurrentCC;
private BigButton _OK;
private BigButton _Cancel;

public ControllerConfigScreen(Skin skin) : base(skin) {

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

  _OK = new BigButton(Skin);
  _OK.BackColor = Color.DarkGreen;
  _OK.Bounds = new Rectangle(10, 90, ClientSize.Width/2-15, 32);
  _OK.Text = "OK";
  _OK.Click += new EventHandler(OnOK);
  _OK.Parent = this;
  
  _Cancel = new BigButton(Skin);
  _Cancel.BackColor = Color.DarkRed;
  _Cancel.Bounds = new Rectangle(ClientSize.Width/2+5, 90, ClientSize.Width/2-15, 32);
  _Cancel.Text = "Cancel";
  _Cancel.Click += new EventHandler(OnCancel);
  _Cancel.Parent = this;
}

private void UpdateButtons() {
  if(_CurrentName != null) _CurrentName.Text = "Name: " + _Config.Name;
  if(_CurrentCC != null) _CurrentCC.Text = "CC: " + _Config.CC;
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