# MMTTY_XMMT_OCX_VB6_Csharp_Interop
MMTTY XMMT.OCX VB6 -> C# Interop demonstrates Spectrum control updates from MMTTY via its XMMT.ocx. This old VB6 style ocx only partially works with interop because of "unsafe" data usage. The code here shows how to connect those non-working events and calls to the modern .NET GUI environment.

Implementation allows use of the ocx controls via interop so these can easily be added to the GUI directly by the IDE. Where needed the special dispatch code is used to handle otherwise incompatible data interfaces. The majority of the XMMT control features are Interop compatible so making further app deveopment easier.

Project developed on the VS2026 IDE with the help of Gemini & ChatGPT AI

See /XMMTwrapper1/XMMT_OCX_VB6_C#_Interop.md for outline of issues and solutions found.

MMTTY is the go-to ham radio RTTY application. Find it here http://hamsoft.ca/pages/mmtty.php. This program requires that MMTTY is installed and the path to the installation is currently hard-coded, so adjust that to suit. Also its associated XMMT.ocx needs to be registered. The ocx gives good access to MMTTY functionality and hence is a good vehicle for development of custom RTTY applications.
