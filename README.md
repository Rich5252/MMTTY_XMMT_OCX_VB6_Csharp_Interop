# MMTTY_XMMT_OCX_VB6_Csharp_Interop
MMTTY XMMT.OCX VB6 -> C# Interop demonstrates Spectrum control updates from MMTTY and method to connect old VB6 ActiveX that have unsupported (unsafe) data types to modern .NET GUI environments.

Implementation uses the Interop controls together with the special interface where requierd for incompatible data interfaces. The majority of the XMMT controls are Interop compatible so making further app deveopment easier.

Project developed on VS2026 IDE with the help of Gemini & ChatGPT AI

See /XMMTwrapper1/XMMT_OCX_VB6_C#_Interop.md for outline of issues and solutions found.

MMTTY is the go-to ham radio RTTY application. Find it here http://hamsoft.ca/pages/mmtty.php. This program requires that MMTTY is installed and the path to the installation is currently hard-coded, so adjust that to suit. Also its associated XMMT.ocx needs to be registered. The ocx gives good access to MMTTY functionality and hence is a good vehicle for development of custom RTTY applications.
