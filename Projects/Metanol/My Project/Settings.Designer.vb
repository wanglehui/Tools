﻿'------------------------------------------------------------------------------
' <auto-generated>
'     This code was generated by a tool.
'     Runtime Version:2.0.50727.1378
'
'     Changes to this file may cause incorrect behavior and will be lost if
'     the code is regenerated.
' </auto-generated>
'------------------------------------------------------------------------------

Option Strict On
Option Explicit On


Namespace My
    
    <Global.System.Runtime.CompilerServices.CompilerGeneratedAttribute(),  _
     Global.System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "9.0.0.0"),  _
     Global.System.ComponentModel.EditorBrowsableAttribute(Global.System.ComponentModel.EditorBrowsableState.Advanced)>  _
    Partial Friend NotInheritable Class MySettings
        Inherits Global.System.Configuration.ApplicationSettingsBase
        
        Private Shared defaultInstance As MySettings = CType(Global.System.Configuration.ApplicationSettingsBase.Synchronized(New MySettings),MySettings)
        
#Region "My.Settings Auto-Save Functionality"
#If _MyType = "WindowsForms" Then
    Private Shared addedHandler As Boolean

    Private Shared addedHandlerLockObject As New Object

    <Global.System.Diagnostics.DebuggerNonUserCodeAttribute(), Global.System.ComponentModel.EditorBrowsableAttribute(Global.System.ComponentModel.EditorBrowsableState.Advanced)> _
    Private Shared Sub AutoSaveSettings(ByVal sender As Global.System.Object, ByVal e As Global.System.EventArgs)
        If My.Application.SaveMySettingsOnExit Then
            My.Settings.Save()
        End If
    End Sub
#End If
#End Region
        
        Public Shared ReadOnly Property [Default]() As MySettings
            Get
                
#If _MyType = "WindowsForms" Then
               If Not addedHandler Then
                    SyncLock addedHandlerLockObject
                        If Not addedHandler Then
                            AddHandler My.Application.Shutdown, AddressOf AutoSaveSettings
                            addedHandler = True
                        End If
                    End SyncLock
                End If
#End If
                Return defaultInstance
            End Get
        End Property
        
        '''<summary>
        '''Selected folder with images
        '''</summary>
        <Global.System.Configuration.UserScopedSettingAttribute(),  _
         Global.System.Configuration.SettingsDescriptionAttribute("Selected folder with images"),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("")>  _
        Public Property Folder() As String
            Get
                Return CType(Me("Folder"),String)
            End Get
            Set
                Me("Folder") = value
            End Set
        End Property
        
        <Global.System.Configuration.UserScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("0, 0")>  _
        Public Property MainLocation() As Global.System.Drawing.Point
            Get
                Return CType(Me("MainLocation"),Global.System.Drawing.Point)
            End Get
            Set
                Me("MainLocation") = value
            End Set
        End Property
        
        <Global.System.Configuration.UserScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("692, 536")>  _
        Public Property MainSize() As Global.System.Drawing.Size
            Get
                Return CType(Me("MainSize"),Global.System.Drawing.Size)
            End Get
            Set
                Me("MainSize") = value
            End Set
        End Property
        
        <Global.System.Configuration.UserScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("230")>  _
        Public Property MainSplitter() As Integer
            Get
                Return CType(Me("MainSplitter"),Integer)
            End Get
            Set
                Me("MainSplitter") = value
            End Set
        End Property
        
        <Global.System.Configuration.UserScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("359")>  _
        Public Property LeftSplitter() As Integer
            Get
                Return CType(Me("LeftSplitter"),Integer)
            End Get
            Set
                Me("LeftSplitter") = value
            End Set
        End Property
        
        <Global.System.Configuration.UserScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("")>  _
        Public Property LastVer() As String
            Get
                Return CType(Me("LastVer"),String)
            End Get
            Set
                Me("LastVer") = value
            End Set
        End Property
        
        <Global.System.Configuration.UserScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("Normal")>  _
        Public Property MainState() As Global.System.Windows.Forms.FormWindowState
            Get
                Return CType(Me("MainState"),Global.System.Windows.Forms.FormWindowState)
            End Get
            Set
                Me("MainState") = value
            End Set
        End Property
        
        <Global.System.Configuration.UserScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("286")>  _
        Public Property HorizontalSplitter() As Integer
            Get
                Return CType(Me("HorizontalSplitter"),Integer)
            End Get
            Set
                Me("HorizontalSplitter") = value
            End Set
        End Property
        
        <Global.System.Configuration.UserScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("275")>  _
        Public Property VerticalSplitter() As Integer
            Get
                Return CType(Me("VerticalSplitter"),Integer)
            End Get
            Set
                Me("VerticalSplitter") = value
            End Set
        End Property
        
        <Global.System.Configuration.UserScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("0, 0")>  _
        Public Property LargeLocation() As Global.System.Drawing.Point
            Get
                Return CType(Me("LargeLocation"),Global.System.Drawing.Point)
            End Get
            Set
                Me("LargeLocation") = value
            End Set
        End Property
        
        <Global.System.Configuration.UserScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("292, 273")>  _
        Public Property LargeSize() As Global.System.Drawing.Size
            Get
                Return CType(Me("LargeSize"),Global.System.Drawing.Size)
            End Get
            Set
                Me("LargeSize") = value
            End Set
        End Property
        
        <Global.System.Configuration.UserScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("Maximized")>  _
        Public Property LargeState() As Global.System.Windows.Forms.FormWindowState
            Get
                Return CType(Me("LargeState"),Global.System.Windows.Forms.FormWindowState)
            End Get
            Set
                Me("LargeState") = value
            End Set
        End Property
        
        <Global.System.Configuration.UserScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("False")>  _
        Public Property LargeShown() As Boolean
            Get
                Return CType(Me("LargeShown"),Boolean)
            End Get
            Set
                Me("LargeShown") = value
            End Set
        End Property
        
        <Global.System.Configuration.UserScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("")>  _
        Public Property KwAutoComplete() As String
            Get
                Return CType(Me("KwAutoComplete"),String)
            End Get
            Set
                Me("KwAutoComplete") = value
            End Set
        End Property
        
        <Global.System.Configuration.UserScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("")>  _
        Public Property KwSynonyms() As String
            Get
                Return CType(Me("KwSynonyms"),String)
            End Get
            Set
                Me("KwSynonyms") = value
            End Set
        End Property
    End Class
End Namespace

Namespace My
    
    <Global.Microsoft.VisualBasic.HideModuleNameAttribute(),  _
     Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
     Global.System.Runtime.CompilerServices.CompilerGeneratedAttribute()>  _
    Friend Module MySettingsProperty
        
        <Global.System.ComponentModel.Design.HelpKeywordAttribute("My.Settings")>  _
        Friend ReadOnly Property Settings() As Global.Metanol.My.MySettings
            Get
                Return Global.Metanol.My.MySettings.Default
            End Get
        End Property
    End Module
End Namespace
