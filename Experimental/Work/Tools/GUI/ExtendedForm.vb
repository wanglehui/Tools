Imports System.ComponentModel, EOS.API
Imports System.Runtime.InteropServices
Imports Tools.Experimental.API.Common
Imports Tools.Experimental.API
Imports System.Windows.Forms

'From �Tools http://codeplex.com/DTools
Namespace GUI
    ''' <summary><see cref="System.Windows.Forms.Form"/> with additional functionality based on Win32 API</summary>
    Public Class ExtendedForm : Inherits System.Windows.Forms.Form
#Region "CloseBox"
        ''' <summary>Possible states of system menu</summary>
        Public Enum SystemMenuState
            ''' <summary>Indicates that the menu item is disabled, but not grayed, so it cannot be selected, but visually seems like it can be selected. Button in title bar is greyed.
            ''' Note: User CANNOT press Alt+F4 (or use another method exluding force methods (kill)) to close window if used by instance member (not static (shared)) <see cref="ExtendedForm.CloseBoxEnabled"/>; if used with static (shared) <see cref="CloseBoxEnabled"/> user CAN use Alt+F4 (or other 'soft' methods) to close window.</summary>
            Disabled = enmEnableMenuItemStatus.MF_DISABLED
            ''' <summary>Indicates that the menu item is enabled and restored from a grayed state so that it can be selected.</summary>
            Enabled = enmEnableMenuItemStatus.MF_ENABLED
            ''' <summary>Indicates that the menu item is disabled and grayed so that it cannot be selected. Button in title bar is greyed.
            ''' Note: User CANNOT press Alt+F4 (or use another method exluding force methods (kill)) to close window if used by instance member (not static (shared)) <see cref="ExtendedForm.CloseBoxEnabled"/>; if used with static (shared) <see cref="CloseBoxEnabled"/> user CAN use Alt+F4 (or other 'soft' methods) to close window.</summary>
            Grayed = enmEnableMenuItemStatus.MF_GRAYED
        End Enum

        ''' <summary>Gets or sets state of selected item of system menu of window represented by <paramref name="WindowHandle"/></summary>
        ''' <param name="MenuItem">Item of system menu (NOTE: Only <see cref="SystemMenuItems.SC_CLOSE"/> works)</param>
        ''' <param name="WindowHandle">Handle to window which's menu should be queryed or altered</param>
        ''' <value>New state of menu item</value>
        ''' <returns>Curent state of menu item</returns>
        ''' <remarks>Getter is little bit destructive - it sets menu state to <see cref="enmEnableMenuItemStatus.MF_ENABLED"/> in order to get its state and then renew its state to just got value</remarks>
        ''' <exception cref="ArgumentException">Given <paramref name="MenuItem"/> doesnt exists</exception> 
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
        <Browsable(False)> _
        Private Shared Property SystemMenuItemEnabled(ByVal WindowHandle As Int32, ByVal MenuItem As API.SystemMenuItems) As SystemMenuState
            Get
                Dim Prev As enmPreviousMenuItemStatus = EnableMenuItem(GetSystemMenu(WindowHandle, 0), MenuItem, enmEnableMenuItemStatus.MF_BYCOMMAND Or enmEnableMenuItemStatus.MF_ENABLED)
                If Prev = enmPreviousMenuItemStatus.DoesNotExist Then Throw New ArgumentException("Menu item doesn't exist", "MenuItem")
                SystemMenuItemEnabled = Prev
                SystemMenuItemEnabled(WindowHandle, MenuItem) = Prev
            End Get
            Set(ByVal value As SystemMenuState)
                Select Case EnableMenuItem(GetSystemMenu(WindowHandle, 0), MenuItem, enmEnableMenuItemStatus.MF_BYCOMMAND Or value)
                    Case enmPreviousMenuItemStatus.DoesNotExist
                        Throw New ArgumentException("Menu item doesn't exist", "MenuItem")
                    Case Else
                End Select
            End Set
        End Property
        ''' <summary>Contains value of the <see cref="CloseBoxEnabled"/> property</summary>
        <EditorBrowsable(EditorBrowsableState.Never)> _
        Private _CloseBoxEnabled As SystemMenuState = SystemMenuState.Enabled
        ''' <summary>Gets or sets state of the close button ('X') of current <see cref="ExtendedForm"/></summary>
        ''' <value>New state of button</value>
        ''' <returns>Curent state of button</returns>
        ''' <remarks>Status of menu is not re-set after changing <see cref="MinimizeBox"/> or <see cref="MaximizeBox"/> property. You have to refresh user-visible status of this property manually!</remarks>
        ''' <exception cref="ArgumentException">Error while accessing system menu status (may be caused by no close item in system menu - e.g. because <see cref="CloseBox"/> is false)</exception>
        <Category("Window Style"), Description("Gets or sets state of the close button ('X') of current form. Cannot be chaged when CloseBox is False.")> _
        Public Property CloseBoxEnabled() As SystemMenuState
            Get
                Return _CloseBoxEnabled
            End Get
            Set(ByVal value As SystemMenuState)
                CloseBoxEnabled(Me) = value
                _CloseBoxEnabled = CloseBoxEnabled(Me)
            End Set
        End Property
        ''' <summary>Determines if designer should serialize the <see cref="CloseBoxEnabled"/> property's value</summary>
        ''' <returns>True when <see cref="CloseBoxEnabled"/> is not <see cref="SystemMenuState.Enabled"/> and <see cref="CloseBox"/> is True</returns>
        Private Function ShouldSerializeCloseBoxEnabled() As Boolean
            Return CloseBox AndAlso CloseBoxEnabled <> SystemMenuState.Enabled
        End Function
        ''' <summary>Resets the <see cref="CloseBoxEnabled"/> property to its default value <see cref="SystemMenuState.Enabled"/></summary>
        Private Sub ResetCloseBoxEnabled()
            Try
                CloseBoxEnabled = SystemMenuState.Enabled
            Catch ex As ArgumentException
                _CloseBoxEnabled = SystemMenuState.Enabled
            End Try
        End Sub
        ''' <summary>Gets or sets state of the close button ('X') of given <see cref="System.Windows.Forms.IWin32Window"/></summary>
        ''' <param name="Window">Window (form) to get or set state of close button</param>
        ''' <value>New state of button</value>
        ''' <returns>Curent state of button</returns>
        ''' <remarks>Value set via static (shared) property may be lost when window if minimized, maximized or restored</remarks>
        ''' <exception cref="ArgumentException">Error while accessing system menu status (may be caused by no close item in system menu)</exception>
        Public Shared Property CloseBoxEnabled(ByVal Window As System.Windows.Forms.IWin32Window) As SystemMenuState
            Get
                Return CloseBoxEnabled(Window.Handle)
            End Get
            Set(ByVal value As SystemMenuState)
                CloseBoxEnabled(Window.Handle) = value
            End Set
        End Property
        ''' <summary>Gets or sets state of the close button ('X') of window with given handle</summary>
        ''' <param name="WindowHandle">Handle of window to get or set state of close button</param>
        ''' <value>New state of button</value>
        ''' <returns>Curent state of button</returns>
        ''' <remarks>Value set via static (shared) property may be lost when window if minimized, maximized or restored</remarks>
        ''' <exception cref="ArgumentException">Error while accessing system menu status (may be caused by no close item in system menu)</exception>
        Public Shared Property CloseBoxEnabled(ByVal WindowHandle As IntPtr) As SystemMenuState
            Get
                Return SystemMenuItemEnabled(WindowHandle.ToInt32, SystemMenuItems.SC_CLOSE)
            End Get
            Set(ByVal value As SystemMenuState)
                SystemMenuItemEnabled(WindowHandle.ToInt32, SystemMenuItems.SC_CLOSE) = value
            End Set
        End Property
        ''' <summary>Contains value of the <see cref="CloseBox"/> property</summary>
        <EditorBrowsable(EditorBrowsableState.Never)> _
        Private _CloseBox As Boolean = True
        ''' <summary>Determines if form has close box in upper-right corner in its caption bar</summary>
        ''' <remarks>Setting this property to false causes mennuitem "Close" disappearing and the X button being grayed. User CANNOT use Alt+F4 or other 'non-killing' method to close the window. Property <see cref="CloseBoxEnabled"/> cannot be changed while <see cref="CloseBox"/> is False.
        ''' You'd better use <see cref="CloseBoxEnabled"/>
        ''' Windows Vista: The close (X) button is NOT grayed but does nothing when user click it.</remarks>
        <DefaultValue(True), Category("Window Style"), Description("Determines if form has close box in upper-right corner in its caption bar. Note: The X�button doesn't disappear, its only grayed (even not grayed on Vista). So, use rather CloseBoxEnabled.")> _
        Public Property CloseBox() As Boolean
            Get
                Return _CloseBox
            End Get
            Set(ByVal value As Boolean)
                If value <> CloseBox Then
                    If value Then
                        If GetSystemMenu(Me.Handle, APIBool.TRUE) = -1 Then
                            Throw New InvalidOperationException("Unable to obtain handle to system menu.")
                        End If
                        Me.CloseBoxEnabled = Me.CloseBoxEnabled
                    Else
                        If RemoveMenu(GetSystemMenu(Me.Handle, APIBool.FALSE), SystemMenuItems.SC_CLOSE, enmSelectMenuMethod.MF_BYCOMMAND) = 0 Then
                            Throw New Win32APIException
                        End If
                        DrawMenuBar(Me.Handle)
                    End If
                    _CloseBox = value
                End If
            End Set
        End Property
        ''' <summary>(Re)applies the <see cref="CloseBoxEnabled"/> property on the system menu</summary>
        Private Sub ApplyCloseBoxEnabled()
            If Me.CloseBox Then _
                Me.CloseBoxEnabled = Me.CloseBoxEnabled
        End Sub
#End Region
#Region "Window State Events"
        ''' <summary>Raised after the <see cref="WindowState"/> property is changed</summary>
        ''' <param name="sender">source of the event</param>
        ''' <param name="e">Event params (always <see cref="EventArgs.Empty"/>)</param>
        <Description("Raised after the WindowState property is changed")> _
        <Category("Action")> _
        Public Event WindowStateChanged(ByVal sender As Object, ByVal e As EventArgs)
        ''' <summary>Raises the <see cref="WindowStateChanged"/> event</summary>
        ''' <param name="e">Event arguments (always <see cref="EventArgs.Empty"/>)</param>
        ''' <remarks>Note to inheritors: Always call base class's method in order the event to be raised and additional functionalidy maintained in base class's method to work properly</remarks>
        Protected Overridable Sub OnWindowStateChanged(ByVal e As EventArgs)
            ApplyCloseBoxEnabled()
            RaiseEvent WindowStateChanged(Me, e)
        End Sub
#End Region
#Region "Events"
        ''' <summary>Processes Windows messages.</summary>
        ''' <param name="m">The Windows <see cref="Message"/> to process. </param>
        ''' <remarks>Note for inheritors: Always call base class's method <see cref="WndProc"/> unless you should block certain base class's functionality</remarks>
        <DebuggerStepThrough()> _
        Protected Overrides Sub WndProc(ByRef m As System.Windows.Forms.Message)
            'Pre process
            Select Case m.Msg
                Case Messages.WM_SYSCOMMAND
                    Select Case m.WParam
                        Case SystemMenuItems.SC_CLOSE
                            If CloseBox = False OrElse CloseBoxEnabled <> SystemMenuState.Enabled Then _
                                Return 'Disable possibility to close window using Alt+F4 when close button is disabled. This is not because the Vista issue related to the CloseBox property
                        Case Else
                            If AddedMenuItems.ContainsKey(m.WParam) Then
                                AddedMenuItems(m.WParam).PerformClick()
                            End If
                    End Select
            End Select

            MyBase.WndProc(m)

            'Post process
            Select Case m.Msg
                Case Messages.WM_SYSCOMMAND
                    Select Case m.WParam
                        Case WindowStateChangedNotifications.SC_RESTORE, WindowStateChangedNotifications.SC_MAXIMIZE, WindowStateChangedNotifications.SC_MINIMIZE
                            OnWindowStateChanged(EventArgs.Empty) 'State change events
                    End Select
                Case Messages.WM_ACTIVATE, Messages.WM_SIZE, Messages.WM_SYNCPAINT, Messages.WM_NCCREATE, Messages.WM_NCPAINT, Messages.WM_NCACTIVATE, Messages.WM_NCHITTEST, Messages.WM_PAINT
                    ProcessMessageForTitleButton(m)
            End Select

        End Sub
#End Region
#Region "Flash"
        Public Sub Flash(ByVal Count As UInt32, Optional ByVal Flags As dwFlags = dwFlags.FLASHW_ALL)
            FlashWindowEx(New FLASHWINFO(Me.Handle, Flags, Count, 0))
        End Sub
        Public Sub Flash(ByVal Timeout As Int32, Optional ByVal Flags As dwFlags = dwFlags.FLASHW_ALL)
            FlashWindowEx(New FLASHWINFO(Me.Handle, Flags, 0, Timeout))
        End Sub
        Public Sub StopFlashing()
            FlashWindowEx(New FLASHWINFO(Me.Handle, dwFlags.FLASHW_STOP, 0, 0))
        End Sub

        Private Declare Function FlashWindowEx Lib "user32.dll" (ByRef pwfi As FLASHWINFO) As Int32

        <StructLayout(LayoutKind.Sequential)> _
        Public Structure FLASHWINFO
            Public Sub New(ByVal hwnd As IntPtr, ByVal dwFlags As dwFlags, ByVal uCount As UInt32, ByVal dwTimeout As Int32)
                Me.hwnd = hwnd
                Me.dwFlags = dwFlags
                Me.uCount = uCount
                Me.dwTimeout = dwTimeout
                Me.cbSize = Convert.ToUInt32(Marshal.SizeOf(GetType(FLASHWINFO)))
            End Sub
            Public cbSize As UInt32
            Public hwnd As IntPtr
            Public dwFlags As dwFlags
            Public uCount As UInt32
            Public dwTimeout As Int32
        End Structure
        <Flags()> _
        Public Enum dwFlags As Int32
            ''' <summary>stop flashing</summary>
            FLASHW_STOP = 0
            ''' <summary>flash the window title</summary>
            FLASHW_CAPTION = 1
            ''' <summary>flash the taskbar button</summary>
            FLASHW_TRAY = 2
            ''' <summary>flash the window title and flash the taskbar button</summary>
            FLASHW_ALL = FLASHW_CAPTION Or FLASHW_TRAY
            ''' <summary>flash continuously</summary>
            FLASHW_TIMER = 4
            ''' <summary>flash until the window comes to the foreground</summary>
            FLASHW_TIMERNOFG = 12
        End Enum
#End Region
#Region "HelpButton"
        Private WithEvents CustomHelpButton As WinTrayButton
        Private Sub ProcessMessageForTitleButton(ByRef m As Message)
            If Me.FormBorderStyle <> Windows.Forms.FormBorderStyle.None AndAlso Me.HelpButton AndAlso Me.ControlBox AndAlso ((Me.FormBorderStyle = Windows.Forms.FormBorderStyle.FixedToolWindow OrElse Me.FormBorderStyle = Windows.Forms.FormBorderStyle.SizableToolWindow) OrElse _
                (Me.MinimizeBox OrElse Me.MaximizeBox)) Then
                If CustomHelpButton Is Nothing Then
                    CustomHelpButton = New WinTrayButton(Me)
                    CustomHelpButton.ButtonIsHelpButton = True
                    CustomHelpButton.ButtonMenuText = "N�pov�da"
                End If
                If Me.FormBorderStyle = Windows.Forms.FormBorderStyle.FixedToolWindow OrElse Me.FormBorderStyle = Windows.Forms.FormBorderStyle.SizableToolWindow Then
                    CustomHelpButton.ButtonTrayIndex = 2
                ElseIf Me.MinimizeBox OrElse Me.MaximizeBox Then
                    CustomHelpButton.ButtonTrayIndex = 4
                Else
                    CustomHelpButton.ButtonTrayIndex = 2
                End If
            Else
                If CustomHelpButton IsNot Nothing Then
                    CustomHelpButton.Dispose()
                    CustomHelpButton = Nothing
                End If

            End If
        End Sub
#End Region
#Region "System menu"
        ''' <summary>Skladuje polo�ky p�idan� do syst�mov�ho menu</summary>
        Private AddedMenuItems As New Dictionary(Of Integer, MenuItem)
        ''' <summary>P�id� polo�ku do syst�mov�ho menu</summary>
        ''' <param name="Index">Index polo�ky</param>
        ''' <param name="Item">Polo�ka</param>
        ''' <exception cref="API.Win32APIException">Chyba p�i p�id�v�n�</exception>
        Public Function AddSystemMenuItem(ByVal Item As MenuItem, ByVal Index As Integer) As Integer
            Dim systemMenuHandle As IntPtr = API.GUI.GetSystemMenu(Me.Handle, False)
            Dim Struct As Object = GetType(MenuItem).GetMethod("CreateMenuItemInfo", Reflection.BindingFlags.Instance Or Reflection.BindingFlags.NonPublic).Invoke(Item, New Object() {})
            Dim info As MENUITEMINFO
            Dim ptr As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(Struct))
            Try
                Marshal.StructureToPtr(Struct, ptr, False)
                info = Marshal.PtrToStructure(ptr, GetType(MENUITEMINFO))
            Finally
                Marshal.FreeHGlobal(ptr)
            End Try
            While AddedMenuItems.ContainsKey(info.wID)
                info.wID += 1
            End While
            If API.GUI.InsertMenuItem(systemMenuHandle, Index, True, info) = 0 Then
                Throw New API.Win32APIException
            End If
            AddedMenuItems.Add(info.wID, Item)
            API.GUI.DrawMenuBar(Me.Handle)
        End Function
#End Region

    End Class
End Namespace