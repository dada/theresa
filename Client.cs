
using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace Theresa {

public delegate void ValueChangedEventHandler(object sender, EventArgs e);

public class Client : Form {

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

private Skin _CurrentSkin;
public Skin CurrentSkin {
  get { return _CurrentSkin; }
  set { ChangeSkin(value); }
}

private Skin _DefaultSkin;
public Skin DefaultSkin {
  get { return _DefaultSkin; }
}

private string _CurrentLayout;
public string CurrentLayout {
  get { return _CurrentLayout; }
  set { LoadLayout(value); }
}

private MidiDevice _CurrentDevice;
public MidiDevice CurrentDevice {
  get { return _CurrentDevice; }
  set {
    _CurrentDevice = value;
    OpenDevice(_CurrentDevice.ID);
  }
}

private ConnectionDetails _CurrentConnection;
public ConnectionDetails CurrentConnection {
  get { return _CurrentConnection; }
  set { _CurrentConnection = value; }
}
  
private MidiDevice[] _Devices;
public MidiDevice[] Devices {
  get { return _Devices; }
}

private int _LastDeviceUsed;

private BigButton _Quit;
private BigButton _DisplayMode;
private BigButton _Settings;
private BigButton _LoadPreset;
private BigButton _SavePreset;

private Bitmap _LogoBitmap;
private Control _Logo;

private TcpClient _Client;
private NetworkStream _Net;
private bool _Connected;
private System.Windows.Forms.Timer _TryToConnect;

private SettingsScreen _SettingsScreen;
private ControllerConfigScreen _ControllerConfigScreen;

private ImageAttributes _MagentaKiller;

public Client() {

  Assembly me = Assembly.GetExecutingAssembly();

  if(System.Environment.OSVersion.Platform != PlatformID.WinCE) {
    Width = 240;
    Height = 320 + 34;
  } else {
    this.WindowState = FormWindowState.Maximized;
    this.FormBorderStyle = FormBorderStyle.None;
    this.ControlBox = false;
    this.Menu = null;
  }

  _MagentaKiller = new ImageAttributes();
  _MagentaKiller.SetColorKey(Color.Magenta, Color.Magenta);    

  _CurrentConnection = new ConnectionDetails();
  _CurrentConnection.UseActiveSync = true;

  // load default skin
  _DefaultSkin = new Skin();
  _DefaultSkin.Load("Default");
  
  // reload last settings
  string layout = "Default";
  string skin = "Default";
  _LastDeviceUsed = 0;
  if(File.Exists(AppPath + "\\Last.xml")) {
    StreamReader file = new StreamReader(AppPath + "\\Last.xml");
    string xml = file.ReadToEnd();
    file.Close();
    XmlDocument x = new XmlDocument();
    x.LoadXml(xml);
    foreach(XmlNode n in x.ChildNodes[0].ChildNodes) {
      if(n.NodeType == XmlNodeType.Element) {
        switch(n.Name) {
        case "Connection":
          _CurrentConnection.UseActiveSync = Convert.ToBoolean(n.Attributes["activesync"].Value);
          _CurrentConnection.Host = n.Attributes["host"].Value;
          _CurrentConnection.Port = Convert.ToInt32(n.Attributes["port"].Value);
          break;
        case "Layout":
          layout = n.Attributes["name"].Value;
          break;
        case "Skin":
          skin = n.Attributes["name"].Value;
          break;
        case "Device":
          _LastDeviceUsed = Convert.ToInt32(n.Attributes["id"].Value);
          break;
        }
      }
    }
  }

  CurrentSkin = new Skin();
  CurrentSkin.Load(skin, _DefaultSkin);
  
  this.BackColor = CurrentSkin.BackgroundColor;
  this.Text = String.Format("Theresa v{0}.{1}.{2}", 
    me.GetName().Version.Major,
    me.GetName().Version.Minor,
    me.GetName().Version.Build
  );
  if(me.GetManifestResourceStream("Theresa.Mae.ico") != null) {
    this.Icon = new Icon(me.GetManifestResourceStream("Theresa.Mae.ico"));
  }

  if(me.GetManifestResourceStream("Theresa.Logo.png") != null) {
    _LogoBitmap = new Bitmap(me.GetManifestResourceStream("Theresa.Logo.png"));
  } else {
    _LogoBitmap = new Bitmap(87, 25);
  }
  _Logo = new Control();
  _Logo.BackColor = CurrentSkin.BackgroundColor;
  _Logo.Bounds = new Rectangle(0, 0, _LogoBitmap.Size.Width, _LogoBitmap.Size.Height);
  _Logo.Paint += new PaintEventHandler(PaintLogo);
  _Logo.Parent = this;

  _Quit = new BigButton(CurrentSkin);
  if(me.GetManifestResourceStream("Theresa.Button-Quit.png") != null) {
    _Quit.Picture = new Bitmap(me.GetManifestResourceStream("Theresa.Button-Quit.png"));
  }
  _Quit.BackColor = Color.DarkRed;
  _Quit.Bounds = new Rectangle(this.ClientSize.Width-25, 0, 24, 24);
  _Quit.Text = "Q";
  _Quit.Click += new EventHandler(OnQuit);
  _Quit.Parent = this;

  _Settings = new BigButton(CurrentSkin);
  if(me.GetManifestResourceStream("Theresa.Button-Configure.png") != null) {
    _Settings.Picture = new Bitmap(me.GetManifestResourceStream("Theresa.Button-Configure.png"));
  }
  _Settings.Bounds = new Rectangle(this.ClientSize.Width-50, 0, 24, 24);
  _Settings.Text = "C";
  _Settings.Click += new EventHandler(OnSettings);
  _Settings.Parent = this;

  _DisplayMode = new BigButton(CurrentSkin);
  if(me.GetManifestResourceStream("Theresa.Button-DisplayMode.png") != null) {
    _DisplayMode.Picture = new Bitmap(me.GetManifestResourceStream("Theresa.Button-DisplayMode.png"));  
  }
  _DisplayMode.Bounds = new Rectangle(this.ClientSize.Width-75, 0, 24, 24);
  _DisplayMode.Text = "#";
  _DisplayMode.Click += new EventHandler(OnChangeDisplayMode);
  _DisplayMode.Parent = this;

  _SavePreset = new BigButton(CurrentSkin);
  if(me.GetManifestResourceStream("Theresa.Button-Save.png") != null) {
    _SavePreset.Picture = new Bitmap(me.GetManifestResourceStream("Theresa.Button-Save.png"));  
  }
  _SavePreset.Bounds = new Rectangle(this.ClientSize.Width-100, 0, 24, 24);
  _SavePreset.Text = "S";
  _SavePreset.Click += new EventHandler(OnSavePreset);
  _SavePreset.Parent = this;

  _LoadPreset = new BigButton(CurrentSkin);
  if(me.GetManifestResourceStream("Theresa.Button-Quit.png") != null) {
    _LoadPreset.Picture = new Bitmap(me.GetManifestResourceStream("Theresa.Button-Load.png"));
  }
  _LoadPreset.Bounds = new Rectangle(this.ClientSize.Width-125, 0, 24, 24);
  _LoadPreset.Text = "L";
  _LoadPreset.Click += new EventHandler(OnLoadPreset);
  _LoadPreset.Parent = this;

  _TryToConnect = new System.Windows.Forms.Timer();
  _TryToConnect.Tick += new EventHandler(OnTryToConnect);
  _TryToConnect.Interval = 1000;
  _TryToConnect.Enabled = false;
  
  LoadLayout(layout);
  Connect();  

  if(_Connected) {
    CurrentDevice = Devices[_LastDeviceUsed];
  } else {
    _Devices = new MidiDevice[0];
    _CurrentDevice = new MidiDevice();
    _CurrentDevice.ID = 0;
    _CurrentDevice.Name = "none";
    Console.WriteLine("Can't connect");
    // _TryToConnect.Enabled = true;
  }
}

private void Connect() {
  _Client = new TcpClient();
  string host;
  int port;
  if(_CurrentConnection.UseActiveSync) {
    if(System.Environment.OSVersion.Platform == PlatformID.WinCE) {    
      host = "PPP_PEER";
      port = 7353;
    } else {
      host = "localhost";
      port = 7353;
    }
  } else {
    host = _CurrentConnection.Host;
    port = _CurrentConnection.Port;
  }
  try {
    _Client.Connect(host, port);
    _Client.NoDelay = true;
    _Connected = true;
  } 
  catch(SocketException ex) {
    Console.WriteLine("got exception: " + ex.ToString());
    // MessageBox.Show( "got exception: " + ex.ToString() + "\nMessage: " + ex.Message + "\nSocket Error Code: " + ex.ErrorCode.ToString() );
  }
  if(_Connected) {
    _Net = _Client.GetStream();
    GetDevices();
    if(_CurrentDevice != null && _CurrentDevice.Name == "none") {
      if(_LastDeviceUsed > _Devices.Length - 1) {
        OpenDevice(0);
      } else {
        OpenDevice(_LastDeviceUsed);
      }
    }
    // _TryToConnect.Enabled = false;
  } else {
    _Devices = new MidiDevice[0];
    _CurrentDevice = new MidiDevice();
    _CurrentDevice.ID = 0;
    _CurrentDevice.Name = "none";
  }
}

private void OnTryToConnect(object sender, EventArgs e) {
  Connect();
}

private void PaintLogo(object sender, PaintEventArgs e) {
  e.Graphics.DrawImage(
    _LogoBitmap, _Logo.Bounds, 
    0, 0, _LogoBitmap.Width, _LogoBitmap.Height, 
    GraphicsUnit.Pixel, _MagentaKiller
  );  
}

static void Main() {
  Application.Run(new Client());
}

public void ChangeSkin(Skin skin) {
  _CurrentSkin = skin;
  BackColor = skin.BackgroundColor;
  if(_Logo != null) _Logo.BackColor = CurrentSkin.BackgroundColor;
  foreach(Control c in Controls) {
    if(c is ISkinned) {
      ISkinned skinned = (ISkinned) c;
      skinned.Skin = CurrentSkin;
      c.Invalidate();
    }
  }
  Invalidate();
}

public void LoadLayout(string name) {

  for(int i = Controls.Count-1; i >= 0; i--) {
    Control c = Controls[i];
    if(c is IMidiController) {
      Controls.Remove(c);
      c.Dispose();
    }
  }
  _CurrentLayout = name;
  
  string filename = AppPath + "\\Layouts\\" + name + ".xml";
  StreamReader file = new StreamReader(filename);
  string xml = file.ReadToEnd();
  file.Close();
  XmlDocument x = new XmlDocument();
  x.LoadXml(xml);
  foreach(XmlNode n in x.ChildNodes[0].ChildNodes) {
    IController controller = null;
    if(n.NodeType == XmlNodeType.Element && n.Name == "Fader") {
      controller = (IController) new Fader(_CurrentSkin);
    }
    if(n.NodeType == XmlNodeType.Element && n.Name == "Button") {
      controller = (IController) new PushButton(_CurrentSkin);
    }
    if(n.NodeType == XmlNodeType.Element && n.Name == "XyPad") {
      controller = (IController) new XyPad(_CurrentSkin);
    }
    if(n.NodeType == XmlNodeType.Element && n.Name == "Pager") {
      controller = (IController) new Pager(_CurrentSkin);
    }
    if(controller != null) {
      controller.Skin = CurrentSkin;
      controller.ID = Convert.ToInt32(n.Attributes["id"].Value);
      controller.Configure += new EventHandler(OnControlConfigure);
      if(controller is IMidiController) {
        ((IMidiController)controller).Config.CC = 101 + Convert.ToInt32(n.Attributes["id"].Value);
        ((IMidiController)controller).ValueChanged += new ValueChangedEventHandler(OnControlChange);
      }
      Control control = (Control) controller;
      control.Left = Convert.ToInt32(n.Attributes["x"].Value);
      control.Top = Convert.ToInt32(n.Attributes["y"].Value) + _Quit.Height;
      control.Parent = this;
    } else {
      Console.WriteLine("Could not build: " + n.Name);
    }
  }

  // notify
  foreach(Control c in Controls) {
    if(c is IController) {
      ((IController) c).LayoutLoaded();
    }
  }
}

private void GetDevices() {
  _Net.Write(new byte[2] { 63, 35 }, 0, 2);
  _Net.Flush();
  byte[] n = new byte[1] { 0 };
  while(!_Net.DataAvailable) {
    Thread.Sleep(10);
  }
  if(_Net.Read(n, 0, 1) != 0) {
    int ndevices = Convert.ToInt32(n[0]);
    _Devices = new MidiDevice[ndevices];
    for(int i = 0; i < ndevices; i++) {
      _Net.Write(new byte[3] { 63, 110, Convert.ToByte(i) }, 0, 3);
      _Net.Flush();
      while(!_Net.DataAvailable) {
        Thread.Sleep(10);
      }
      byte[] namebuf = new byte[32];
      if(_Net.Read(namebuf, 0, 32) != 0) {
        string name = String.Empty;
        for(int j = 0; j < 32; j++) {
          name += Convert.ToChar(namebuf[j]);
        }
        Devices[i] = new MidiDevice();
        Devices[i].ID = i;
        Devices[i].Name = name;
      }
      else {
        Console.WriteLine("could not read!");
      }
    }
    // Devices.Items.Clear();
    // foreach(object dev in devices) {
    //   Devices.Items.Add(dev);
    // }
  } else {
    Console.WriteLine("could not read!");
  }

}

private void OpenDevice(int id) {
  byte[] o = new byte[2] { 111, Convert.ToByte(id) };
  _Net.Write(o, 0, 2);
  _Net.Flush();
  _CurrentDevice = Devices[id];
}

private void OnControlConfigure(object sender, EventArgs e) {
  IMidiController control = (IMidiController) sender;
  if(_ControllerConfigScreen == null) _ControllerConfigScreen = new ControllerConfigScreen(CurrentSkin);
  ControllerConfig cfg = new ControllerConfig();
  control.Config.CopyTo(cfg);
  _ControllerConfigScreen.Config = cfg;
  DialogResult r = _ControllerConfigScreen.ShowDialog();
  if(r == DialogResult.OK) {
    cfg.CopyTo(control.Config);
  }
}

private void OnChangeDisplayMode(object sender, EventArgs e) {
  foreach(Control c in Controls) {
    if(c is IController) {
      IController control = (IController) c;
      control.ToggleDisplayMode();
    }
  }
}

private void OnSettings(object sender, EventArgs e) {
  if(_SettingsScreen == null) _SettingsScreen = new SettingsScreen(this);
  _SettingsScreen.ShowDialog();
}

private void OnSavePreset(object sender, EventArgs e) {
  TextInputScreen savescreen = Global.GetTextInputScreen(CurrentSkin);
  DialogResult r = savescreen.ShowDialog();
  if(r == DialogResult.OK) {
    Preset p = new Preset();
    p.Master = this;
    p.Save(AppPath + "\\Presets\\" + savescreen.String + ".xml");
  }
}

private void OnLoadPreset(object sender, EventArgs e) {
  SelectionScreen loadscreen = Global.GetSelectionScreen(CurrentSkin);
  loadscreen.Items.Clear();
  foreach(string file in Directory.GetFiles(AppPath + "\\Presets")) {
    if(file.ToLower().EndsWith(".xml")) {
      string name = Path.GetFileNameWithoutExtension(file);
      loadscreen.Items.Add(name);
    }
  }
  loadscreen.ShowPage();
  DialogResult r = loadscreen.ShowDialog();
  if(r == DialogResult.OK) {
    Preset p = new Preset();
    p.Master = this;
    string name = loadscreen.Items[loadscreen.SelectedIndex].ToString();
    p.Load(AppPath + "\\Presets\\" + name + ".xml");
  }
}

public void OnControlChange(object sender, EventArgs e) {
  if(!_Connected) return;
  IMidiController control = (IMidiController) sender;
  if(_Connected) {
	if(control.Config.CC != 0) {
      byte value = Convert.ToByte(control.Value);
      byte cc = Convert.ToByte(control.Config.CC);
	  _Net.Write(new byte[3] { 99, cc, value }, 0, 3);
      _Net.Flush();
	}
	if(control.Config.CC2 != 0) {
      byte value = Convert.ToByte(control.Value2);
      byte cc = Convert.ToByte(control.Config.CC2);	
	  _Net.Write(new byte[3] { 99, cc, value }, 0, 3);
      _Net.Flush();
    }	  
  }
}

private void OnQuit(object sender, EventArgs e) {
  StreamWriter file = new StreamWriter(AppPath + "\\Last.xml");
  file.WriteLine("<Theresa file=\"last\">");
  file.WriteLine("\t<Skin name=\"{0}\" />", CurrentSkin.Name);
  file.WriteLine("\t<Layout name=\"{0}\" />", CurrentLayout);
  file.WriteLine("\t<Device id=\"{0}\" />", CurrentDevice.ID);
  file.WriteLine("\t<Connection activesync=\"{0}\" host=\"{1}\" port=\"{2}\" />", 
    CurrentConnection.UseActiveSync,
    CurrentConnection.Host, CurrentConnection.Port
  );
  file.WriteLine("</Theresa>");
  file.Close();
  Application.Exit();
}

} // class


public class MidiDevice {
  public int ID;
  public string Name;
  
  public override string ToString() {
    return Name;
  }

} // class

public class ConnectionDetails {

private bool _UseActiveSync = true;
public bool UseActiveSync {
  get { return _UseActiveSync; }
  set { _UseActiveSync = value; }
}

private string _Host = String.Empty;
public string Host {
  get { return _Host; }
  set { _Host = value; }
}

private int _Port = 7353;
public int Port {
  get { return _Port; }
  set { _Port = value; }
}

public override string ToString() {
  if(_UseActiveSync) {
    return "(ActiveSync)";
  } else {
    return "(Wifi) " + _Host;
  }
}

public void CopyTo(ConnectionDetails target) {
  target.UseActiveSync = _UseActiveSync;
  target.Host = _Host;
  target.Port = _Port;
}

} // class

} // namespace
