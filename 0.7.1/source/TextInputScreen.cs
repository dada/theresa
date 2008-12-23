
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

public class TextInputScreen : ScreenBase {

private string _String = String.Empty;
public string String {
  get { return _String; }
  set { 
    _String = value;
    _Display.Text = _String;
    _Display.Invalidate();
  }
}

private BigButton _Display;
private BigButton _OK;
private BigButton _Cancel;

private BigButton[] _Keyboard;

public TextInputScreen(Skin skin) : base(skin) {

  // keyboard
  string[] kbd = new string[40] {
    "1", "2", "3", "4", "5", "6", "7", "8", "9", "0",
    "q", "w", "e", "r", "t", "y", "u", "i", "o", "p",
    "a", "s", "d", "f", "g", "h", "j", "k", "l", "<",
    "z", "x", "c", "v", "b", "n", "m", "-", ".", "«"
  };
  _Keyboard = new BigButton[40];
  for(int r = 0; r < 4; r++) {
    for(int c = 0; c < 10; c++) {
      int i = r*10+c;
      string t = kbd[i];
      int x = 23*c;
      int y = 50 + 23*r;
      _Keyboard[i] = new BigButton(Skin);
      _Keyboard[i].Bounds = new Rectangle(x, y, 22, 22);
      _Keyboard[i].Text = t;
      _Keyboard[i].Click += new EventHandler(OnKeypress);
      _Keyboard[i].Parent = this;
    }
  }

  _Display = new BigButton(Skin);
  _Display.Bounds = new Rectangle(10, 10, ClientSize.Width-20, 32);
  _Display.Text = _String;
  _Display.Parent = this;

  _OK = new BigButton(Skin);
  _OK.BackColor = Color.DarkGreen;
  _OK.Bounds = new Rectangle(10, 150, ClientSize.Width/2-15, 32);
  _OK.Text = "OK";
  _OK.Click += new EventHandler(OnOK);
  _OK.Parent = this;
  
  _Cancel = new BigButton(Skin);
  _Cancel.BackColor = Color.DarkRed;
  _Cancel.Bounds = new Rectangle(ClientSize.Width/2+5, 150, ClientSize.Width/2-15, 32);
  _Cancel.Text = "Cancel";
  _Cancel.Click += new EventHandler(OnCancel);
  _Cancel.Parent = this;
}

private void OnKeypress(object sender, EventArgs e) {
  BigButton b = (BigButton) sender;
  string k = b.Text;
  switch(k) {
  case "<":
    if(String.Length > 0) String = String.Remove(String.Length-1, 1);
    break;
  case "«":
    String = String.Empty;
    break;
  default:
    String += k;
    break;
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