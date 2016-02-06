Snowy
=====

Snowy is a Windows desktop app to copy images from FlashAir by a wireless connection. You can copy images in bulk or select from thumbnails one by one. Also you can make this app check new images automatically at a regular interval.

##Requirements

 * Windows 7 or newer
 * .NET Framework 4.5.2

Tested on Windows 8.1 64bit with SD-WB008G (Class6), SD-WC016G (W-02 Class10) and SD-WE016G (W-03 Class10).

##Development

This app is a WPF app developed in C# with Visual Studio Professional 2013.

As for FlashAir's API used in this app, refer to [FlashAir Developers][1].

##History

Ver 1.2.0 2016-2-6

 - Added option to create dated folder
 - Fixed a few bugs

Ver 1.1.0 2015-8-22

 - Fixed a bug in SnowyTool that writes unnecessary lines to CONFIG file
 - Changed target framework to 4.5.2

Ver 1.0.1 2015-7-9

 - Changed files in OneDrive that are set to online-only to be shown in thumbnails

Ver 1.0.0 2015-3-31

 - First decent release

Ver 0.9.10 2015-3-3

 - Added color management support

Ver 0.9.9 2015-2-12

 - Added function to save current image on desktop and to send current image to clipboard
 - Changed copy source to accept not only root folder but also its descendants

Ver 0.9.8 2014-12-29

 - Fixed exception when total file size in specified dates exceeds 2GiB

Ver 0.9.7 2014-12-4

 - Added command line arguments to start auto check automatically
 - Modified handling of thumbnails for non-standard JPEG format files

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

##License

 - MIT License

##Other

 - Libraries: [Reactive Extensions][2], [WPF Monitor Aware Window][3], [Desktop Toast][4]

[1]: https://www.flashair-developers.com/en/
[2]: http://rx.codeplex.com/
[3]: https://github.com/emoacht/WpfMonitorAware
[4]: https://github.com/emoacht/DesktopToast
