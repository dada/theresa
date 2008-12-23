
Theresa v0.7.1 README

Theresa is a simple PocketPC/Windows (targeting the Compact .NET Framework 1.0)
client/server application that turns a Pocket PC PDA into a MIDI controller 
surface. well, that's the plan.

basically you start the server on the desktop PC (the server sits quietly in 
the system tray), then start the client on the (ActiveSync-connected)
Pocket PC et voila, you can use your finger on the PDA touchscreen to move 
faders, push buttons and XY pads.

faders, buttons and pads that can be freely assigned to MIDI CC. how sweet.

Theresa has customizable layouts, she's skinnable and can save/load presets.
last but not least, she's freeware (as in free beer) and open source.

so, if you can't afford a sturdy Doepfer, a powerful Mackie or a classy 
Novation Remote, and you have an old iPAQ or similar laying around, it's time 
to meet Theresa and rejoice.

if you have no idea of what I'm talking about, Theresa will probably be useless
to you. go play some videogame instead.

DISCLAIMER

I stole graphics for the provided skins from either free VST plugins (eg. 
Casiopea, Junox2) or from actual synth/device pictures googled on the net. I 
hope this isn't a problem. if someone prefers his/her own graphics to be 
removed from Theresa, I'll happily cease and desist. it's just eyecandy after 
all.

and of course, the usual stuff: Theresa is provided "as is", no warranty, no 
support, your own risk etc. etc.

REQUIREMENTS

 1. a Windows PC with the .NET Framework installed
 2. a Pocket PC / Windows Mobile PDA with the .NET Compact Framework (1.0, 
    later versions should be compatible) installed
 3. Activesync to connect them

Linux/Mono/SynCE could also work, didn't tested (testers welcome!).

INSTALLATION

 1. put the contents of the "client" directory somewhere on your Pocket PC
 2. put the contents of the "server" directory somewhere on your desktop PC
 3. start the TheresaServer.exe program on your desktop PC
 4. start the Theresa.exe program on your Pocket PC
 5. open the configure screen, select a MIDI output device
 6. enable the corresponding MIDI input device in your favourite music program
 7. map the parameters, do MIDI learn, tweak, tweak, tweak!

in the "vstmap" directory there are example MIDI mappings for some VST 
instruments. you can load them in VSTHost, then open the corresponding preset
in Theresa.

FURTHER INFORMATION

please visit http://dada.perl.it/theresa for more info.

yours sincerely,
Aldo "dada" Calpini