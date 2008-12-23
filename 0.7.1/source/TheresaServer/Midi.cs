
using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace Theresa {

public class MidiOut {

[DllImport("winmm.dll",
  EntryPoint="midiOutGetNumDevs", SetLastError = true
)]
private static extern int midiOutGetNumDevs();

[DllImport("winmm.dll",
  EntryPoint="midiOutGetDevCapsA", SetLastError = true
)]
private static extern int midiOutGetDevCaps(
  int device, out MIDIOUTCAPS caps, int capsize
);

[DllImport("winmm.dll",
  EntryPoint="midiOutOpen", SetLastError = true
)]
private static extern int midiOutOpen(
  out int handle, int device, int a, int b, int c
);

[DllImport("winmm.dll",
  EntryPoint="midiOutClose", SetLastError = true
)]
private static extern int midiOutClose(int device);


[DllImport("winmm.dll",
  EntryPoint="midiOutShortMsg", SetLastError = true
)]
private static extern int midiOutShortMsg(int device, int msg);


// source: http://www.dotnet247.com/247reference/msgs/16/82486.aspx
[StructLayout(LayoutKind.Sequential)] // , CharSet=CharSet.Auto)]
private struct MIDIOUTCAPS {
  public ushort wMid;
  public ushort wPid;
  public uint vDriverVersion;
  [MarshalAs(UnmanagedType.ByValTStr, SizeConst=32)]
  public string szPname; //public char[32] szPname;
  public ushort wTechnology;
  public ushort wVoices;
  public ushort wNotes;
  public ushort wChannelMask;
  public uint dwSupport;
}

private int _handle = 0;

public int Devices {
  get { return midiOutGetNumDevs(); }
}

public string[] GetDeviceNames() {
  int n = this.Devices;
  string[] r = new string[n];
  for(int i = 0; i < n; i++) {
    MIDIOUTCAPS c = new MIDIOUTCAPS();
    int rc = midiOutGetDevCaps(i, out c, Marshal.SizeOf(typeof(MIDIOUTCAPS)));
    r[i] = c.szPname;
  }
  return r;
}

public int Open(int device) {
  if(_handle != 0) midiOutClose(_handle);
  return midiOutOpen(out _handle, device, 0, 0, 0);
}

public int Close() {
  if(_handle != 0) {
    int r = midiOutClose(_handle);
    _handle = 0;
    return r;
  }
  else {
    return 0;
  }
}

public int Send(int msg) {
  return midiOutShortMsg(_handle, msg);
}

public int SendCC(byte channel, byte cc, byte value) {
  int msg = 0xB0 + channel | cc << 8 | value << 16;
  Console.WriteLine("SendCC(ch{0}, cc{1}, v{2}) = {3}",
    channel, cc, value, msg
  );
  return midiOutShortMsg(_handle, msg);
  
}

} // class


/*
[StructLayout(LayoutKind.Sequential)]
public class MIDIOUTCAPS {
  public Int16 wMid = 0;
  public Int16 wPid = 0;
  public Int32 vDriverVersion = 0;
  // public char[] szPname = new char[32];
  public char szPname01 = ' ';
  public char szPname02 = ' ';
  public char szPname03 = ' ';
  public char szPname04 = ' ';
  public char szPname05 = ' ';
  public char szPname06 = ' ';
  public char szPname07 = ' ';
  public char szPname08 = ' ';
  public char szPname09 = ' ';
  public char szPname10 = ' ';
  public char szPname11 = ' ';
  public char szPname12 = ' ';
  public char szPname13 = ' ';
  public char szPname14 = ' ';
  public char szPname15 = ' ';
  public char szPname16 = ' ';
  public char szPname17 = ' ';
  public char szPname18 = ' ';
  public char szPname19 = ' ';
  public char szPname20 = ' ';
  public char szPname21 = ' ';
  public char szPname22 = ' ';
  public char szPname23 = ' ';
  public char szPname24 = ' ';
  public char szPname25 = ' ';
  public char szPname26 = ' ';
  public char szPname27 = ' ';
  public char szPname28 = ' ';
  public char szPname29 = ' ';
  public char szPname30 = ' ';
  public char szPname31 = ' ';
  public char szPname32 = ' ';
  public Int16 wTechnology = 0;
  public Int16 wVoices = 0;
  public Int16 wChannelMask = 0;
  public Int32 dwSupport = 0;
}
*/

} // namespace