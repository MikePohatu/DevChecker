# ![logo](/icons/logo-24.png) Documentation

* [Features](#features)
* [Connectivity](#connectivity)
  * [Device](#device)
  * [Server](#server)
* [User Settings](#user-settings)
* [Saving log output](#saving-log-output)

## Features

[Custom Actions](/documentation/customactions.md)

## Connectivity
In order to connect to the remote device and the ConfigMgr server, the appropriate connectivity and authentication needs to be available.

### Device
Primary connectivity to the remote device is made over WinRM. If not already configured, WinRM can be enabled on the device with the following command:

```
winrm quickconfig
```

WinRM encrypts traffic by default, even over HTTP, and authenticates using Kerberos. If you are connecting from a non-domain device and/or can't connect using Kerberos, you need to connect over HTTPS to encrypt the non-secure authentication methods.

The device must have a valid certificate capable of server authentication with a common name that matches the device name. WinRM over HTTPS can then be enabled with the following command:

```
winrm quickconfig -transport:https
```

Some connectivity is not done over WinRM as the native tool is simply launched from DevChecker. These include:

* Computer Management
* Access to C$ and other network shares

### Server

The ConfigMgr server is accessed over WMI using the default ConfigMgr using the root\SMS namespace. In future access may be added over the Administration Service which will use the default HTTPS connection.

---

## User Settings
When DevChecker is closed, the current connection entries are saved to a user config file. These settings are loaded back in on application start. This is saved in *%APPDATA%\20Road\DevChecker\conf.json*

---

## Saving log output
Due to technical limitations, the logging text in the output window cannot currently be selected for copy/paste. You can save the text from output pane to a text/log file by clicking the *Save log* link at the bottom right of the DevChecker window.