# ![logo](/documentation/images/logo-24.png) DevChecker 



DevChecker is a little application to allow IT admins to remotely access devices and complete common diagnostics tasks, without having to bug the user.

This tool was created because existing tools either didn't do what I wanted, or were stuck behind an email sign-up page. 

## Current features:

* Connect to remote device over WinRM
* Gather basic device info like OS version, IP, model, serial, logged in users, is power connected etc.
* Quick access to c$ share, Computer Management, remote PowerShell
* Ability to run ConfigMgr client tasks e.g. Machine Policy refresh etc
* List and control device services
* List and kill device processes
* List installed patches
* List install applications
* Show BitLocker status
* Run *gpupdate /force*
* Add custom PowerShell scripts to run on client
* See ConfigMgr collections
* 'Connect as' functionality

## Planned features

* Access to ConfigMgr Software Center
* Add custom tabs using PowerShell scripts
* Run ConfigMgr scripts
* List and install available updates
* List printers and print drivers
* Shutdown, restart, logoff functions

[Documentation](/documentation/README.md)

