
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

public class SettingsScreen : ScreenBase {

private Client _Master;
public Client Master {
  get { return _Master; }
  set { _Master = value; }
}

private BigButton _ChooseDevice;
private BigButton _ChooseSkin;
private BigButton _ChooseLayout;
private BigButton _Cancel;

public SettingsScreen(Client master) : base(master.CurrentSkin) {

  _Master = master;

  _ChooseDevice = new BigButton(Skin);
  _ChooseDevice.Bounds = new Rectangle(10, 10, ClientSize.Width-20, 32);
  // ChooseDevice.Text = "Device: " + Master.CurrentDevice.ToString();
  FitTextInButton(_ChooseDevice, "Device: ", Master.CurrentDevice.ToString());  
  _ChooseDevice.Click += new EventHandler(OnChooseDevice);
  _ChooseDevice.Parent = this;

  _ChooseSkin = new BigButton(Skin);
  _ChooseSkin.Bounds = new Rectangle(10, 50, ClientSize.Width-20, 32);
  _ChooseSkin.Text = "Skin: " + Master.CurrentSkin.Name;
  _ChooseSkin.Click += new EventHandler(OnChooseSkin);
  _ChooseSkin.Parent = this;

  _ChooseLayout = new BigButton(Skin);
  _ChooseLayout.Bounds = new Rectangle(10, 90, ClientSize.Width-20, 32);
  _ChooseLayout.Text = "Layout: " + Master.CurrentLayout;
  _ChooseLayout.Click += new EventHandler(OnChooseLayout);
  _ChooseLayout.Parent = this;
  
  _Cancel = new BigButton(Skin);
  _Cancel.BackColor = Color.DarkRed;
  _Cancel.Bounds = new Rectangle(10, 290, ClientSize.Width-20, 32);
  _Cancel.Text = "Close";
  _Cancel.Click += new EventHandler(OnCancel);
  _Cancel.Parent = this;
}

private void FitTextInButton(BigButton button, string text) {
  FitTextInButton(button, String.Empty, text);
}

private void FitTextInButton(BigButton button, string prefix, string text) {
  int w = button.MeasureString(prefix+text);
  while(w > button.Width - 10) {
    text = "..." + text.Substring(4, text.Length-4);
    w = button.MeasureString(prefix+text);
  }
  button.Text = prefix+text;
}

private void OnChooseDevice(object sender, EventArgs e) {
  SelectionScreen select = Global.GetSelectionScreen(Skin);
  select.Items.Clear();
  for(int i = 0; i < Master.Devices.Length; i++) {
    select.Items.Add(Master.Devices[i]);
  }
  select.ShowPage();
  DialogResult r = select.ShowDialog();
  if(r == DialogResult.OK) {
    Master.CurrentDevice = Master.Devices[select.SelectedIndex];
    FitTextInButton(_ChooseDevice, "Device: ", Master.CurrentDevice.ToString());
    // ChooseDevice.Text = "Device: " + Master.CurrentDevice.ToString();
  }
}

private void OnChooseSkin(object sender, EventArgs e) {
  SelectionScreen select = Global.GetSelectionScreen(Skin);
  select.Items.Clear();
  foreach(string file in Directory.GetFiles(AppPath + "\\Skins")) {
    if(file.ToLower().EndsWith(".xml")) {
      string name = Path.GetFileNameWithoutExtension(file);
      select.Items.Add(name);
    }
  }
  select.ShowPage();
  DialogResult r = select.ShowDialog();
  if(r == DialogResult.OK) {
    Skin skin = new Skin();
    skin.Load(select.Items[select.SelectedIndex].ToString());
    Master.ChangeSkin(skin);
    _ChooseSkin.Text = "Skin: " + Master.CurrentSkin.Name;
  }
}

private void OnChooseLayout(object sender, EventArgs e) {
  SelectionScreen select = Global.GetSelectionScreen(Skin);
  select.Items.Clear();
  foreach(string file in Directory.GetFiles(AppPath + "\\Layouts")) {
    if(file.ToLower().EndsWith(".xml")) {
      string name = Path.GetFileNameWithoutExtension(file);
      select.Items.Add(name);
    }
  }
  select.ShowPage();
  DialogResult r = select.ShowDialog();
  if(r == DialogResult.OK) {
    Master.LoadLayout(select.Items[select.SelectedIndex].ToString());
    _ChooseLayout.Text = "Layout: " + Master.CurrentLayout;
  }
}

private void OnCancel(object sender, EventArgs e) {
  DialogResult = DialogResult.Cancel;
  Close();
}

} // class

} // namespace