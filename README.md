# Snowy

Snowy is a Windows desktop app to copy images from FlashAir by a wireless connection. You can copy images in bulk or select from thumbnails one by one. Also you can make this app check new images automatically at a regular interval.

Details: [English](https://emoacht.github.io/SnowyImageCopy/index.html) or [Japanese](https://emoacht.github.io/SnowyImageCopy/index_jp.html)

## Requirements & Download

### Ver 2.x

 - Windows 10 Anniversary Update (1607) or newer
 - .NET Framework __4.7.2__

Download: [Snowy Image Copy](https://www.microsoft.com/store/apps/9MTLPNGRW85L) (Microsoft Store)

### Ver 1.x

 - Windows 7 or 8.1
 - .NET Framework __4.5.2__

Download: [the latest release](https://github.com/emoacht/SnowyImageCopy/releases/latest)

## Development

This app is a WPF app developed in C#. It is tested with 4 generations of FlashAir: SD-WB008G, SD-WC016G (W-02), SD-WE016G (W-03) and SD-UWA032G (W-04).

Ver 2.x utilizes Per-Monitor DPI awareness features of Windows 10 Anniversary Update or newer.

As for FlashAir's API used in this app, refer to [FlashAir Developers](https://www.flashair-developers.com/)
 ([copy](https://flashair-developers.github.io/website/)).

## History

[History](HISTORY.md)

## License

 - MIT License

## Libraries

 - [Reactive Extensions](https://github.com/dotnet/reactive)
 - [XamlBehaviors for WPF](https://github.com/microsoft/XamlBehaviorsWpf)
 - [WPF Monitor Aware Window](https://github.com/emoacht/WpfMonitorAware)
 - [Desktop Toast](https://github.com/emoacht/DesktopToast)

## Developer

 - emoacht (emotom[atmark]pobox.com)

