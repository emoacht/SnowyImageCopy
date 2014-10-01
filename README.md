Snowy
=====

Snowy is a Windows desktop app to copy images from FlashAir by a wireless connection. You can copy images in bulk or select from thumbnails one by one. Also you can make this app check new images automatically at a regular interval.

##Requirements

 * Windows 8 or newer
 * Windows 7 (required to install .NET Framework 4.5)

Tested on Windows 8.1 64bit with SD-WB008G (Class6) and SD-WC016G (W-02 Class10).

##Development

This app is a WPF app developed in C# with Visual Studio Professional 2013.

As for FlashAir's API used in this app, refer to [FlashAir Developers][1].

##History

Ver 0.9.6 2014-10-1

 - Added function to delete image in FlashAir upon copy

Ver 0.9.5 2014-09-12

 - Fixed bugs in auto check

Ver 0.9.4 2014-06-12

 - Changed paths in Options pane to be instantly reflected

Ver 0.9.3 2014-06-08

 - Added Per-Monitor DPI Awareness
 - Added shorter intervals to Auto Check Interval

Ver 0.9.2 2014-05-08

 - Fixed exception when a directory name in FlashAir is longer

Ver 0.9.1 2014-04-23

 - Improved exceptions handling

Ver 0.9.0 2014-04-10

 - Initial release

##Other

 - Libraries: [Reactive Extensions][2], [WPF Per-Monitor DPI Aware Window][3]
 - License: MIT License

[1]: https://www.flashair-developers.com/en/
[2]: http://rx.codeplex.com/
[3]: https://github.com/emoacht/WpfPerMonitorDpi
