
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

public abstract class ScreenBase : Form, ISkinned {

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

private Skin _Skin;
public Skin Skin {
  get { return _Skin; }
  set { _Skin = value; ChangeSkin(); }
}

public ScreenBase(Skin skin) : base() {

  _Skin = skin;
  BackColor = _Skin.BackgroundColor;
  
  if(System.Environment.OSVersion.Platform != PlatformID.WinCE) {
    Width = 240;
    Height = 320 + 34;
  } else {
    this.WindowState = FormWindowState.Maximized;
    this.FormBorderStyle = FormBorderStyle.None;
    this.ControlBox = false;
    this.Menu = null;
  }

}

public void ChangeSkin() {
  foreach(Control c in Controls) {
    if(c is ISkinned) {
      ISkinned skinned = (ISkinned) c;
      if(skinned.Skin != _Skin) skinned.Skin = _Skin;
    }
  }
}

} // class

} // namespace