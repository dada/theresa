
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

public class ConnectionScreen : ScreenBase {

private BigButton _UseActiveSync;
private BigButton _Host;
private BigButton _Cancel;

private ConnectionDetails _Connection;
public ConnectionDetails Connection {
  get { return _Connection; }
}

public ConnectionScreen(Skin skin) : base(skin) {

  _Connection = new ConnectionDetails();
  
  _UseActiveSync = new BigButton(Skin);
  _UseActiveSync.Bounds = new Rectangle(10, 10, ClientSize.Width-20, 32);
  _UseActiveSync.Text = "Use ActiveSync";
  _UseActiveSync.Click += new EventHandler(SelectActiveSync);
  _UseActiveSync.Parent = this;
  
  _Host = new BigButton(Skin);
  _Host.Bounds = new Rectangle(10, 50, ClientSize.Width-20, 32);
  _Host.FitText("Use Wifi: ", _Connection.Host);
  _Host.Click += new EventHandler(EditHost);
  _Host.Parent = this;

  _Cancel = new BigButton(Skin);
  _Cancel.BackColor = Color.DarkRed;
  _Cancel.Bounds = new Rectangle(10, 290, ClientSize.Width-20, 32);
  _Cancel.Text = "Close";
  _Cancel.Click += new EventHandler(OnCancel);
  _Cancel.Parent = this;
}

private void SelectActiveSync(object sender, EventArgs e) {
  _Connection.UseActiveSync = true;
  DialogResult = DialogResult.OK;
  Close();
}

private void EditHost(object sender, EventArgs e) {
  TextInputScreen editor = Global.GetTextInputScreen(Skin);
  editor.String = _Connection.Host;
  DialogResult r = editor.ShowDialog();
  if(r == DialogResult.OK) {
    _Connection.UseActiveSync = false;
    _Connection.Host = editor.String;
    DialogResult = r;
    Close();
  }
}

private void OnCancel(object sender, EventArgs e) {
  DialogResult = DialogResult.Cancel;
  Close();
}

} // class

} // namespace