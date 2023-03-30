# MF-Video-Device-Enumerator

This example shows how USB webcam watcher/observer and how webcams and their supported formats enumeration can be implemented in Windows.
There are three solutions provided:
1) Using System.Management package and subscribing to WMI events.
2) Using Microsoft.Management.Infrastructure package and subscribing to WMI events similarly to System.Management. This package must be superior to System.Management and also provides remote management capabilities, but was last updated in 2019 and lacks documentation.
3) Using WinRT DeviceWatcher the way documentation suggests.

First two ways also use Vortice.MediaFoundation to get information about webcam and its supported formats using Media Foundation API.

Included benchmark shows that getting formats using Vortice is faster than WinRT capabilities(at least it was so in my case).

Nevertheless WinRT DeviceWatcher seems to react to webcam plugging/unplugging much faster than other two solutions(at least in WPF sample provided).
