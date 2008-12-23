using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace Theresa {

public class Server : Form {

private NetworkStream _Net;
private MidiOut _Midi;
private Icon _IconNormal;
private Icon _IconOn;
private Icon _IconOff;
private NotifyIcon _Tray;
private Thread _ServerThread;

static void Main() {
  Application.Run(new Server());
}

public Server() {
  _Midi = new MidiOut();

  Assembly me = Assembly.GetExecutingAssembly();
  _IconNormal = new Icon(me.GetManifestResourceStream("TheresaServer.Theresa.ico"));
  _IconOn = new Icon(me.GetManifestResourceStream("TheresaServer.TheresaOn.ico"));
  _IconOff = new Icon(me.GetManifestResourceStream("TheresaServer.TheresaOff.ico"));

  _Tray = new NotifyIcon();
  _Tray.Icon = _IconNormal;
  _Tray.Text = String.Format("Theresa v{0}.{1}.{2}", 
    me.GetName().Version.Major,
    me.GetName().Version.Minor,
    me.GetName().Version.Build
  );
  _Tray.Visible = true;

  ContextMenu popup = new ContextMenu();
  MenuItem quit = new MenuItem();
  quit.Text = "Quit";
  quit.Click += new EventHandler(OnQuit);
  popup.MenuItems.Add(quit);
  _Tray.ContextMenu = popup;

  _ServerThread = new Thread(new ThreadStart(StartServer));
  Console.WriteLine("Starting server...");
  _ServerThread.Start();
  this.Load += new EventHandler(OnLoad);
  this.VisibleChanged +=new EventHandler(OnShow);
}

private void StartServer() {
  try {
    Assembly me = Assembly.GetExecutingAssembly();
    TcpListener server = new TcpListener(IPAddress.Any, 7353);
    server.Start();
    while(true) {
      Console.WriteLine("Waiting...");
      _Tray.Icon = _IconOff;
      _Tray.Text = = String.Format("Theresa v{0}.{1}.{2}", 
        me.GetName().Version.Major,
        me.GetName().Version.Minor,
        me.GetName().Version.Build
      );
      TcpClient client = server.AcceptTcpClient();
      Console.WriteLine("Got connection");
      _Tray.Icon = _IconOn;
      _Tray.Text = String.Format("Theresa v{0}.{1}.{2} CONNECTED", 
        me.GetName().Version.Major,
        me.GetName().Version.Minor,
        me.GetName().Version.Build
      );
      client.NoDelay = true;
      client.ReceiveBufferSize = 1;
      _Net = client.GetStream();
      byte[] c = new byte[1] { 0 };
      try {
        while(_Net.Read(c, 0, 1) != 0) {
          ExecuteCommand(c[0]);
        }
      } catch(IOException) {
      }
      Console.WriteLine("Bye bye");
      _Midi.Close();
    }
  } catch(ThreadAbortException) {
    return;
  }
}

private void ExecuteCommand(byte c) {
  byte[] b = new byte[1] { 0 };
  switch(c) {
  case 63:
    Console.WriteLine("got query");
    if(_Net.Read(b, 0, 1) != 0) {
      switch(b[0]) {
      case 35:
        Console.WriteLine("got query.number");
        _Net.Write(new byte[1] { Convert.ToByte(_Midi.Devices) }, 0, 1);
        break;
      case 110:
        Console.WriteLine("got query.name");
        if(_Net.Read(b, 0, 1) != 0) {
          int d = Convert.ToInt32(b[0]);
          string[] names = _Midi.GetDeviceNames();
          string name = names[d].PadRight(32, ' ');
          byte[] namebytes = new byte[32];
          char[] namechars = name.ToCharArray();
          for(int i = 0; i < 32; i++) {
            namebytes[i] = Convert.ToByte(namechars[i]);
          }
          _Net.Write(namebytes, 0, 32);
        }
        break;
      }
    }
    break;
  case 99:
    Console.WriteLine("got control");
    byte cc = 0;
    if(_Net.Read(b, 0, 1) != 0) {
      cc = b[0];
    }
    if(cc != 0 && _Net.Read(b, 0, 1) != 0) {
      _Midi.SendCC(1, cc, b[0]);
    }
    break;
  case 111:
    Console.WriteLine("got open");
    if(_Net.Read(b, 0, 1) != 0) {
      Console.WriteLine(String.Format("opening device={0}", b[0]));
    }
    _Midi.Open(b[0]);
    break;
  }
}

private void OnQuit(object sender, EventArgs e) {
  _ServerThread.Abort();
  _Tray.Dispose();
  Application.Exit();
  System.Environment.Exit(0);
}

private void OnLoad(object sender, EventArgs e) {
  this.Visible = false;
}

private void OnShow(object sender, EventArgs e) {
  this.Visible = false;
}

} // class

} // namespace
