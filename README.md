PKGThumb - PS4 .pkg icons in Windows Explorer
=============================================

<img width="2518" height="1282" alt="PKGThumb Preview" src="https://github.com/user-attachments/assets/65979b26-ba93-493d-a44b-8ec1985bf64d" />


MADE BY CUTLINGTON WITH CLAUDE AI

Shows each PS4 .pkg file's own icon (icon0.png) as its thumbnail in
Windows Explorer, instead of the same generic icon for every file.

No downloads or installs needed. It is compiled on your PC using the C#
compiler that is already built into Windows (.NET Framework 4).


INSTALL
-------
1. Extract the zip to any folder. Do NOT run install.bat from inside the zip.
2. Right-click install.bat and choose "Run as administrator".
3. In Explorer, set the folder view to Medium icons or larger.

The installer compiles the tool and copies it to C:\Program Files\PkgThumb,
so you can delete the extracted folder afterwards.

IMPORTANT: Windows Explorer restarts during install and uninstall. Your
taskbar, desktop icons and open folder windows disappear for a few seconds,
then come back by themselves. This is normal. Save any work in open folder
windows first. The scripts warn you and wait for a keypress before doing it.


IF EXPLORER DOES NOT COME BACK
------------------------------
1. Press Ctrl + Shift + Esc to open Task Manager.
   (If that does nothing, press Ctrl + Alt + Delete and choose Task Manager.)
2. Click "Run new task" (on older Windows: File, then Run new task).
3. Type  explorer.exe  and press Enter.
Your taskbar and desktop will return. If Task Manager won't open at all,
hold the power button or restart the PC; Explorer starts normally on boot.


UNINSTALL
---------
Right-click uninstall.bat and choose "Run as administrator".
It unregisters the tool and deletes C:\Program Files\PkgThumb.
To update, just run install.bat again from the new version.


NOTES
-----
- Works for every .pkg on the PC, not just one folder.
- Needs administrator rights because it registers a .NET component
  system-wide.
- Reads only the package header and the icon; it does not modify, copy or
  unpack your PKG files.
- Packages with an unusual or encrypted icon entry may still show the
  generic icon.
- Other programs that claim the .pkg file type can override this.
- Windows 10 / 11, 64-bit.
- You can read everything it does in PkgThumb.cs and install.bat before
  running them.


LICENSE
-------
MIT License - see LICENSE.txt. Provided as-is, with no warranty.
