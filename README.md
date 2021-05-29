# 20Road Remote Admin

Remote Admin is a little application to allow IT admins to remotely access devices and complete common diagnostics tasks, without having to bug the user.

This tool was created because existing tools either didn't do what I wanted, or were stuck behind an email sign-up page. 

## Current features:

* Connect to remote device over WinRM
* Gather basic device info like OS version, IP, model, serial, logged in users, is power connected etc.
* Quick access to c$ share, Computer Management, remote PowerShell
* Ability to run ConfigMgr client tasks e.g. Machine Policy refresh etc
* Run *gpupdate /force*
* List installed patches
* List install applications
* Add custom PowerShell scripts 
* See ConfigMgr collections
* 'Connect as' functionality

## Planned features

* Access to Software Center
* Add custom tabs using PowerShell scripts

[Documentation](/documentation/README.md)

