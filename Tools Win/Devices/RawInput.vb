﻿Imports System.ComponentModel, System.Linq, Tools.ExtensionsT
Imports System.Runtime.InteropServices
Imports System.Runtime.CompilerServices
Imports Tools.ComponentModelT

#If Config <= Nightly Then 'Stage: Nighlty
Namespace DevicesT.RawInputT
    ''' <summary>Re</summary>
    Public Class InputDevice
        Implements IEquatable(Of InputDevice)
        ''' <summary>CTor from handle</summary>
        ''' <param name="hDevice">Device handle</param>
        ''' <exception cref="ArgumentNullException"><paramref name="hDevice"/> is <see cref="IntPtr.Zero"/></exception>
        Protected Friend Sub New(ByVal hDevice As IntPtr)
            If hDevice = IntPtr.Zero Then Throw New ArgumentNullException("hDevice")
            _DeviceHandle = hDevice
        End Sub
        ''' <summary>CTor form handle with device type</summary>
        ''' <param name="hDevice">Device handle</param>
        ''' <exception cref="ArgumentNullException"><paramref name="hDevice"/> is <see cref="IntPtr.Zero"/></exception>
        ''' <param name="DeviceType">Type of device. This value is used only for deisplaying purposes and is not used actual operation of the class.</param>
        Protected Friend Sub New(ByVal hDevice As IntPtr, ByVal DeviceType As DeviceType)
            Me.New(hDevice)
            Me.DeviceType = DeviceType
        End Sub
        ''' <summary>Returns a <see cref="T:System.String" /> that represents the current <see cref="InputDevice" />.</summary>
        ''' <returns>A <see cref="T:System.String" /> that represents the current <see cref="InputDevice" />.</returns>
        ''' <filterpriority>2</filterpriority>
        Public Overrides Function ToString() As String
            Return If(DeviceType <> -1, DeviceType.ToString & " ", "") & DeviceHandle.ToString
        End Function
        ''' <summary>Contains device type. Is used only for displaying purposes.</summary>
        Private DeviceType As DeviceType = -1
        ''' <summary>Gets al the raw input devices present at system</summary>
        ''' <returns>Array of raw input devices represented by <see cref="InputDevice"/> class</returns>
        ''' <exception cref="API.Win32APIException">Error while obtaining list of devices</exception>
        Public Shared Function GetAllDevices() As InputDevice()
            Dim Number% = 0
            Dim Ret = API.RawInput.GetRawInputDeviceList(Nothing, Number, API.RawInput.RAWINPUTDEVICELIST.Size)
            If Ret = -1 Then Throw New API.Win32APIException
            Dim Devices(Number - 1) As API.RawInput.RAWINPUTDEVICELIST
            Ret = API.RawInput.GetRawInputDeviceList(Devices, Number, API.RawInput.RAWINPUTDEVICELIST.Size)
            If Ret = -1 Then Throw New API.Win32APIException
            Return (From Device In Devices Select New InputDevice(Device.hDevice, Device.dwType)).ToArray
        End Function
        ''' <summary>Contains value of the <see cref="DeviceHandle"/> property</summary>
        Private ReadOnly _DeviceHandle As IntPtr
        ''' <summary>Gets device handle</summary>
        ''' <returns>System handle of this device</returns>
        <EditorBrowsable(EditorBrowsableState.Advanced)> _
        Public ReadOnly Property DeviceHandle() As IntPtr
            Get
                Return _DeviceHandle
            End Get
        End Property
        ''' <summary>Gets name of the device as string</summary>
        ''' <exception cref="Win32Exception">Error while obtaining device name</exception>
        ''' <remarks>You can get more information about device by useing <see cref="GetDeviceName"/></remarks>
        ''' <seelaso cref="GetDeviceName"/>
        Public Function GetDeviceNameString$()
            'See http://www.codeproject.com/KB/system/rawinput.aspx
            Dim pcbSize = 0UI
            Dim ret = API.RawInput.GetRawInputDeviceInfo(DeviceHandle, API.DeviceInfoTypes.RIDI_DEVICENAME, IntPtr.Zero, pcbSize)
            If ret < 0 Then Throw New API.Win32APIException
            If pcbSize > 0 Then
                Dim pData As IntPtr = Marshal.AllocHGlobal(CInt(pcbSize))
                Try
                    ret = API.RawInput.GetRawInputDeviceInfo(DeviceHandle, API.DeviceInfoTypes.RIDI_DEVICENAME, pData, pcbSize)
                    If ret < 0 Then Throw New API.Win32APIException
                    Dim deviceName$ = Marshal.PtrToStringAnsi(pData)
                    Return deviceName
                Finally
                    Marshal.FreeHGlobal(pData)
                End Try
            Else : Return ""
            End If
        End Function
        ''' <summary>Gets parsed device name. Parsed device name provides more information about the device.</summary>
        ''' <exception cref="Win32Exception">Error while obtaining device name</exception>
        ''' <exception cref="FormatException">Device name obtained from <see cref="GetDeviceNameString"/> has unexpected format and cannot be parsed.</exception>
        ''' <seelaso cref="GetDeviceNameString"/><seelaso cref="RawDeviceName"/>
        Public Function GetDeviceName() As RawDeviceName
            Return New RawDeviceName(GetDeviceNameString)
        End Function
        ''' <summary>Gets hardware information about raw input device represented by this instance</summary>
        ''' <exception cref="API.Win32APIException">Error while obtaining device info</exception>
        ''' <exception cref="RawInputException">Device of unknown type was returned by system</exception>
        Public Function GetDeviceInfo() As DeviceInfo
            Dim Size As UInteger = 0UI
            Dim ret = API.RawInput.GetRawInputDeviceInfo(DeviceHandle, API.RawInput.DeviceInfoTypes.RIDI_DEVICEINFO, IntPtr.Zero, Size)
            If ret < 0 Then Throw New API.Win32APIException
            Dim Struct As API.RawInput.RID_DEVICE_INFO
            Struct.cbSize = API.RID_DEVICE_INFO.Size
            Dim pData As IntPtr = Marshal.AllocHGlobal(Math.Max(CInt(Size), API.RawInput.RID_DEVICE_INFO.Size))
            Marshal.StructureToPtr(Struct, pData, False)
            Try
                ret = API.RawInput.GetRawInputDeviceInfo(DeviceHandle, API.RawInput.DeviceInfoTypes.RIDI_DEVICEINFO, pData, API.RawInput.RID_DEVICE_INFO.Size)
                If ret < 0 Then Throw New API.Win32APIException
                Struct = Marshal.PtrToStructure(pData, GetType(API.RawInput.RID_DEVICE_INFO))
                Try
                    Return DeviceInfo.GetDeviceInfo(Struct, DeviceHandle)
                Catch ex As InvalidEnumArgumentException
                    Throw New RawInputException(ResourcesT.ExceptionsWin.UnknownRawInputDeviceType0.f(Struct.dwType), ex)
                End Try
            Finally
                Marshal.FreeHGlobal(pData)
            End Try
        End Function
#Region "Equals"
        ''' <summary>Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="InputDevice" />.</summary>
        ''' <returns>true if the specified <see cref="T:System.Object" /> is equal to the current <see cref="InputDevice" />; otherwise, false.</returns>
        ''' <param name="obj">The <see cref="T:System.Object" /> to compare with the current <see cref="InputDevice" />.</param>
        ''' <remarks>Comparison is done by overloaded type-safe <see cref="Equals"/> function. Use that function instead.
        ''' <para>Note for inheritors: Type-safe overload of this method can be overriden.</para> </remarks>
        <Obsolete("Use type-safe overload instead"), EditorBrowsable(EditorBrowsableState.Never)> _
        Public NotOverridable Overloads Overrides Function Equals(ByVal obj As Object) As Boolean
            Return TypeOf obj Is InputDevice AndAlso Me.Equals(DirectCast(obj, InputDevice))
        End Function

        ''' <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        ''' <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
        ''' <param name="other">An object to compare with this object.</param>
        ''' <remarks>Two <see cref="InputDevice">InputDevices</see> are consideret equal when both have same <see cref="DeviceHandle"/></remarks>
        Public Overridable Overloads Function Equals(ByVal other As InputDevice) As Boolean Implements System.IEquatable(Of InputDevice).Equals
            If other Is Nothing Then Return False
            Return other.DeviceHandle = Me.DeviceHandle
        End Function
        ''' <summary>Compares two instances of <see cref="InputDevice"/> class</summary>
        ''' <param name="a">A <see cref="InputDevice"/></param>
        ''' <param name="b">A <see cref="InputDevice"/></param>
        ''' <returns>True when both <paramref name="a"/> and <paramref name="b"/> are null or both are not null and has same <see cref="DeviceHandle"/></returns>
        ''' <remarks>This operator uses the <see cref="Equals"/> function</remarks>
        Public Shared Operator =(ByVal a As InputDevice, ByVal b As InputDevice) As Boolean
            Return a Is b OrElse (a IsNot Nothing AndAlso b IsNot Nothing AndAlso a.Equals(b))
        End Operator
        ''' <summary>Compares two instances of <see cref="InputDevice"/> class</summary>
        ''' <param name="a">A <see cref="InputDevice"/></param>
        ''' <param name="b">A <see cref="InputDevice"/></param>
        ''' <returns>True when exactly one of <paramref name="a"/>, <paramref name="b"/> is null or tehy are not null and their <see cref="DeviceHandle"/> differs</returns>
        ''' <remarks>This operator uses the <see cref="Equals"/> function</remarks>
        Public Overloads Shared Operator <>(ByVal a As InputDevice, ByVal b As InputDevice) As Boolean
            Return Not a = b
        End Operator
#End Region
    End Class

    ''' <summary>Parses raw device name obtained by <see cref="InputDevice.GetDeviceName"/> and performs operation with it</summary>
    Public Class RawDeviceName
        Implements IEquatable(Of RawDeviceName), ICloneable(Of RawDeviceName), IFormattable

        ''' <summary>Parses raw device name</summary>
        ''' <param name="Name">Raw device name in format <c>\\??\<em><see cref="ClassCode">ClassCode</see></em>#<em><see cref="SubClassCode">SubClassCode</see></em>#<em><see cref="ProtocolCode">ProtocolCode</see></em></c>#<em><see cref="ClassGuid">ClassGuid</see></em></param>
        ''' <returns>Instance of <see cref="RawDeviceName"/> representing parsed <paramref name="Name"/></returns>
        ''' <exception cref="FormatException"><paramref name="Name"/> is not valid raw device name</exception>
        ''' <exception cref="ArgumentNullException"><paramref name="Name"/> is null</exception>
        Public Shared Function Parse(ByVal Name$) As RawDeviceName
            '\\??\ACPI#PNP0303#3&13c0b0c5&0#{884b96c3-56ef-11d1-bc8c-00a0c91405dd}
            If Name Is Nothing Then Throw New ArgumentNullException("Name")
            Const RawDeviceNameStart$ = "\\?\"
            If Not Name.StartsWith(RawDeviceNameStart$) Then Throw New FormatException(ResourcesT.ExceptionsWin.RawDeviceNameHasUnexpectedFormatItMustStartWith0.f(RawDeviceNameStart$))
            Dim Parts = Name.Substring(RawDeviceNameStart$.Length).Split("#"c)
            If Parts.Length <> 4 Then Throw New FormatException(ResourcesT.ExceptionsWin.RawDeviceNameHasInvalidFormatItShouldConsistOf4Parte)
            Dim ClassGuid As Guid
            Try
                ClassGuid = New Guid(Parts(3))
            Catch ex As Exception
                Throw New FormatException(ResourcesT.ExceptionsWin.TheCalassGuidPartOfDeviceNameIsInInvalidFormatSeeInnerException, ex)
            End Try
            Try
                Return New RawDeviceName(Parts(0), Parts(1), Parts(2), ClassGuid)
            Catch ex As ArgumentException
                Throw New FormatException(ex.Message, ex)
            End Try
        End Function
        ''' <summary>CTor from string - parses device name sting</summary>
        ''' <param name="Name">Raw device name in format <c>\\??\<em><see cref="ClassCode">ClassCode</see></em>#<em><see cref="SubClassCode">SubClassCode</see></em>#<em><see cref="ProtocolCode">ProtocolCode</see></em></c>#<em><see cref="ClassGuid">ClassGuid</see></em></param>
        ''' <exception cref="FormatException"><paramref name="Name"/> is not valid raw device name</exception>
        ''' <exception cref="ArgumentNullException"><paramref name="Name"/> is null</exception>
        Public Sub New(ByVal Name$)
            Dim dName = Parse(Name)
            Init(dName.ClassCode, dName.SubClassCode, dName.ProtocolCode, dName.ClassGuid)
        End Sub
        ''' <summary>CTor - creates new instance of <see cref="RawDeviceName"/> class</summary>
        ''' <param name="ClassCode">Class code of device</param>
        ''' <param name="SubClassCode">Sub class code of device</param>
        ''' <param name="ProtocolCode">Protocol code of device</param>
        ''' <param name="ClassGuid">Guid of device class</param>
        ''' <exception cref="ArgumentNullException"><paramref name="ClassCode"/>, <paramref name="SubClassCode"/> or <paramref name="ProtocolCode"/> is null</exception>
        ''' <exception cref="ArgumentException"><paramref name="ClassCode"/>, <paramref name="SubClassCode"/> or <paramref name="ProtocolCode"/> is and empty string</exception>
        Public Sub New(ByVal ClassCode$, ByVal SubClassCode$, ByVal ProtocolCode$, ByVal ClassGuid As Guid)
            Init(ClassCode, SubClassCode, ProtocolCode, ClassGuid)
        End Sub
        ''' <summary>Initializes this instance of <see cref="RawDeviceName"/></summary>
        ''' <remarks>Call from CTors only</remarks>
        ''' <param name="ClassCode">Class code of device</param>
        ''' <param name="SubClassCode">Sub class code of device</param>
        ''' <param name="ProtocolCode">Protocol code of device</param>
        ''' <param name="ClassGuid">Guid of device class</param>
        ''' <exception cref="ArgumentNullException"><paramref name="ClassCode"/>, <paramref name="SubClassCode"/> or <paramref name="ProtocolCode"/> is null</exception>
        ''' <exception cref="ArgumentException"><paramref name="ClassCode"/>, <paramref name="SubClassCode"/> or <paramref name="ProtocolCode"/> is and empty string</exception>
        Private Sub Init(ByVal ClassCode$, ByVal SubClassCode$, ByVal ProtocolCode$, ByVal ClassGuid As Guid)
            If ClassCode Is Nothing Then Throw New ArgumentNullException("ClassCode")
            If SubClassCode Is Nothing Then Throw New ArgumentNullException("SubClassCode")
            If ProtocolCode Is Nothing Then Throw New ArgumentNullException("ProtocolCode")
            If ClassCode = "" Then Throw New ArgumentException(ResourcesT.ExceptionsWin.CannotBeAnEmptyString.f("ClassCode"), "ClassCode")
            If SubClassCode = "" Then Throw New ArgumentException(ResourcesT.ExceptionsWin.CannotBeAnEmptyString.f("SubClassCode"), "SubClassCode")
            If ProtocolCode = "" Then Throw New ArgumentException(ResourcesT.ExceptionsWin.CannotBeAnEmptyString.f("ProtocolCode"), "ProtocolCode")
            _ClassCode = ClassCode
            _SubClassCode = SubClassCode
            _ProtocolCode = ProtocolCode
            _ClassGuid = ClassGuid
        End Sub

        ''' <summary>Copy CTor - creates new instance of <see cref="RawDeviceName"/> from existing one</summary>
        ''' <param name="Other">Instance to clone</param>
        ''' <exception cref="ArgumentNullException"><paramref name="Other"/> is null</exception>
        Public Sub New(ByVal Other As RawDeviceName)
            If Other Is Nothing Then Throw New ArgumentNullException("Other")
            Me.Init(Other.ClassCode, Other.SubClassCode, Other.ProtocolCode, Other.ClassGuid)
        End Sub
        ''' <summary>Contains value of the <see cref="ClassCode"/> property</summary>
        Private _ClassCode$
        ''' <summary>Gets class code of the device</summary>
        ''' <returns>Device class code</returns>
        Public ReadOnly Property ClassCode$()
            Get
                Return _ClassCode
            End Get
        End Property
        ''' <summary>Contains value of the <see cref="SubClassCode"/> property</summary>
        Private _SubClassCode$
        ''' <summary>Gets sub class code of the device</summary>
        ''' <returns>Device sub class code</returns>
        Public ReadOnly Property SubClassCode$()
            Get
                Return _SubClassCode
            End Get
        End Property
        ''' <summary>Contains value of the <see cref="ProtocolCode"/> property</summary>
        Private _ProtocolCode$
        ''' <summary>Gets protocol code of the device</summary>
        ''' <returns>Device protocol code</returns>
        Public ReadOnly Property ProtocolCode$()
            Get
                Return _ProtocolCode
            End Get
        End Property
        ''' <summary>Contains value of the <see cref="ClassGuid"/> property</summary>
        Private _ClassGuid As Guid
        ''' <summary>Gets class guid of the device</summary>
        ''' <returns>Device class guid</returns>
        Public ReadOnly Property ClassGuid() As Guid
            Get
                Return _ClassGuid
            End Get
        End Property
        ''' <summary>Gets registry path where information about this device is stored</summary>
        ''' <returns>Registry path under HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Enum according to formaing string R</returns>
        ''' <seelaso cref="ToString"/>
        Public ReadOnly Property RegistryPath$()
            Get
                Return String.Format("HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Enum\{0:R}", Me)
            End Get
        End Property
        ''' <summary>Reads device description form the registry</summary>
        ''' <returns>User friendly device description</returns>
        ''' <remarks>Funtion reads value DeviceDesc from key <see cref="RegistryPath"/> and attempts to strim user-unfriendly prefix of description (part before ;)</remarks>
        ''' <exception cref="System.Security.SecurityException">The user does not have the permissions required to read from the registry key.</exception>
        ''' <exception cref="System.IO.IOException">The <see cref="Microsoft.Win32.RegistryKey" /> that contains the specified value has been marked for deletion.</exception>
        ''' <exception cref="System.UnauthorizedAccessException">The user does not have the necessary registry rights.</exception>
        Public Function GetDeviceDescription$()
            Dim Key = Tools.RegistryT.OpenKey(RegistryPath)
            Dim desc = Key.GetValue("DeviceDesc", "", Microsoft.Win32.RegistryValueOptions.None).ToString
            If InStr(desc, ";") > 0 Then
                Dim AfterSemicolon = desc.Split(";"c, 2)(1)
                Return If(AfterSemicolon.Length <> 0, AfterSemicolon, desc)
            End If
            Return desc
        End Function
#Region "Interface implementation"
        ''' <summary>Creates a new object that is a copy of the current instance.</summary>
        ''' <returns>A new object that is a copy of this instance</returns>
        ''' <remarks>Use type-safe <see cref="Clone"/> instead</remarks>
        <Obsolete("Use type-safe Clone instead")> _
        Private Function ICloneable_Clone() As Object Implements System.ICloneable.Clone
            Return Clone()
        End Function
        ''' <summary>Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="RawDeviceName" />.</summary>
        ''' <returns>true if the specified <see cref="T:System.Object" /> is equal to the current <see cref="RawDeviceName" />; otherwise, false.</returns>
        ''' <param name="obj">The <see cref="T:System.Object" /> to compare with the current <see cref="RawDeviceName" />.</param>
        ''' <remarks>Use type-safe overload instead
        ''' <para>Note for inheritors: Type safe overload chan be overriden.</para></remarks>
        <EditorBrowsable(EditorBrowsableState.Never), Obsolete("Use type-safe overload instead")> _
        Public NotOverridable Overloads Overrides Function Equals(ByVal obj As Object) As Boolean
            If obj Is Nothing OrElse Not TypeOf obj Is RawDeviceName Then Return False
            Return Equals(DirectCast(obj, RawDeviceName))
        End Function
        ''' <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        ''' <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
        ''' <param name="other">An object to compare with this object.</param>
        ''' <remarks>Two <see cref="RawDeviceName">RawDeviceNames</see> are considered same if all following properties has same value (strins are compared in case-insensitive culture-independent way): <see cref="ClassCode"/>, <see cref="SubClassCode"/>, <see cref="ProtocolCode"/>, <see cref="ClassGuid"/></remarks>
        Public Overridable Overloads Function Equals(ByVal other As RawDeviceName) As Boolean Implements System.IEquatable(Of RawDeviceName).Equals
            Dim c As StringComparer = StringComparer.InvariantCultureIgnoreCase
            Return other IsNot Nothing AndAlso (other Is Me OrElse ( _
               c.Equals(Me.ClassCode, other.ClassCode) AndAlso c.Equals(Me.SubClassCode, other.SubClassCode) AndAlso c.Equals(Me.ProtocolCode, other.ProtocolCode) AndAlso Me.ClassGuid = other.ClassGuid))
        End Function
        ''' <summary>Returns a <see cref="T:System.String" /> that represents the current <see cref="RawDeviceName" />.</summary>
        ''' <returns>A <see cref="T:System.String" /> that represents the current <see cref="RawDeviceName" />.</returns>
        ''' <remarks>This function uses the G format.</remarks>
        Public Overrides Function ToString() As String
            Return String.Format(Globalization.CultureInfo.InvariantCulture, "\\?\{0}#{1}#{2}#{3:B}", ClassCode, SubClassCode, ProtocolCode, ClassGuid)
        End Function
        ''' <summary>Formats the value of the current instance using the specified format.</summary>
        ''' <returns>A <see cref="T:System.String" /> containing the value of the current instance in the specified format.</returns>
        ''' <param name="format">The <see cref="T:System.String" /> specifying the format to use. -or-
        ''' null to use the default format defined for the type of the <see cref="T:System.IFormattable" /> implementation.</param>
        ''' <param name="formatProvider">Ignored. This class always uses <see cref="Globalization.CultureInfo.InvariantCulture"/> as both formats must be culture invariant.</param>
        ''' <remarks>Suported formats are
        ''' <list type="table"><listheader><term>Format string</term><description>Result</description></listheader>
        ''' <item><term>null or an empty string</term><description>G is used</description></item>
        ''' <item><term>G or g</term><description>General format. Same as used by <see cref="Parse"/>. That is <c>\\?\{0}#{1}#{2}#{3:B}</c></description></item>
        ''' <item><term>R or r</term><description>Registry fomat. Part of registry path unde which more information about the device can be obtained. That is <c>{0}\{1}\{2}</c></description></item></list>
        ''' where
        ''' <list type="table"><item><term>{0}</term><description><see cref="ClassCode"/></description></item>
        ''' <item><term>{1}</term><description><see cref="SubClassCode"/></description></item>
        ''' <item><term>{2}</term><description><see cref="ProtocolCode"/></description></item>
        ''' <item><term>{3}</term><description><see cref="ClassGuid"/></description></item></list></remarks>
        ''' <seelaso cref="Guid.ToString"/><seelaso cref="String.Format"/>
        ''' <exception cref="FormatException"><paramref name="format"/> is not one of predefined formating strings (null, "", "G", "g", "R", "r")</exception>
        Public Overridable Overloads Function ToString(ByVal format As String, ByVal formatProvider As System.IFormatProvider) As String Implements System.IFormattable.ToString
            If format Is Nothing OrElse format = "" OrElse format = "G" OrElse format = "g" Then
                Return Me.ToString
            ElseIf format = "R" OrElse format = "r" Then
                Return String.Format(Globalization.CultureInfo.InvariantCulture, "{0}\{1}\{2}", ClassCode, SubClassCode, ProtocolCode)
            Else
                Throw New FormatException(ResourcesT.ExceptionsWin.InvalidFormatSpecifier)
            End If
        End Function

        ''' <summary>Creates a new object that is a copy of the current instance.</summary>
        ''' <returns>A new object that is a copy of this instance</returns>
        Public Overridable Function Clone() As RawDeviceName Implements ICloneable(Of RawDeviceName).Clone
            Return New RawDeviceName(Me)
        End Function
        ''' <summary>Compares two instances of the <see cref="RawDeviceName"/> class</summary>
        ''' <param name="a">A <see cref="RawDeviceName"/></param>
        ''' <param name="b">A <see cref="RawDeviceName"/></param>
        ''' <returns>True where both <paramref name="a"/> and <paramref name="b"/> are null or they are not null and equals according to the <see cref="Equals"/> method</returns>
        Public Shared Operator =(ByVal a As RawDeviceName, ByVal b As RawDeviceName) As Boolean
            Return a Is b OrElse (a IsNot Nothing AndAlso a.Equals(b))
        End Operator
        ''' <summary>Compares two instances of the <see cref="RawDeviceName"/> class</summary>
        ''' <param name="a">A <see cref="RawDeviceName"/></param>
        ''' <param name="b">A <see cref="RawDeviceName"/></param>
        ''' <returns>True where exactly one of <paramref name="a"/> and <paramref name="b"/> is null or they are not null and does not equal according to the <see cref="Equals"/> method</returns>
        Public Shared Operator <>(ByVal a As RawDeviceName, ByVal b As RawDeviceName) As Boolean
            Return Not a = b
        End Operator
#End Region
    End Class

    ''' <summary>Base class for Device information classes</summary>
    ''' <remarks>This class is not intended to be derived</remarks>
    <EditorBrowsable(EditorBrowsableState.Advanced)> _
    Public MustInherit Class DeviceInfo
        ''' <summary>CTor from device type and handle</summary>
        ''' <param name="DeviceType">Type of device</param>
        ''' <param name="DeviceHandle">Handle to device</param>
        ''' <exception cref="InvalidEnumArgumentException"><paramref name="DeviceType"/> is not member of <see cref="RawInputT.DeviceType"/></exception>
        ''' <exception cref="ArgumentNullException"><paramref name="DeviceHandle"/> is <see cref="IntPtr.Zero"/></exception>
        ''' <exception cref="TypeMismatchException">Type of derived class does not match value of <paramref name="DeviceType"/></exception>
        Protected Sub New(ByVal DeviceType As DeviceType, ByVal DeviceHandle As IntPtr)
            If Not DeviceType.IsDefined Then Throw New InvalidEnumArgumentException("DeviceType")
            If DeviceHandle = IntPtr.Zero Then Throw New ArgumentNullException("DeviceHandle")
            Select Case DeviceType
                Case RawInputT.DeviceType.Hid : If Not TypeOf Me Is HidInfo Then Throw New TypeMismatchException(Me, GetType(HidInfo), ResourcesT.ExceptionsWin.When0Is1ThenTypeOfThisInstanceMustBe2.f("DeviceType", DeviceType, GetType(HidInfo).Name))
                Case RawInputT.DeviceType.Mouse : If Not TypeOf Me Is MouseInfo Then Throw New TypeMismatchException(Me, GetType(MouseInfo), ResourcesT.ExceptionsWin.When0Is1ThenTypeOfThisInstanceMustBe2.f("DeviceType", DeviceType, GetType(MouseInfo).Name))
                Case RawInputT.DeviceType.Keyboard : If Not TypeOf Me Is KeyboardInfo Then Throw New TypeMismatchException(Me, GetType(KeyboardInfo), ResourcesT.ExceptionsWin.When0Is1ThenTypeOfThisInstanceMustBe2.f("DeviceType", DeviceType, GetType(KeyboardInfo).Name))
            End Select
            Me._DeviceType = DeviceType
            _DeviceHandle = DeviceHandle
        End Sub
        ''' <summary>Contains value of the <see cref="DeviceType"/> property</summary>
        Private ReadOnly _DeviceType As DeviceType
        ''' <summary>Contains value of the <see cref="DeviceHandle"/> property</summary>
        Private ReadOnly _DeviceHandle As IntPtr
        ''' <summary>Gets handle of device</summary>
        ''' <returns>System handle of device this instance contains info about</returns>
        Public ReadOnly Property DeviceHandle() As IntPtr
            Get
                Return _DeviceHandle
            End Get
        End Property
        ''' <summary>Gets device type</summary>
        ''' <returns>Type of device. Value of this property determines type of this instance.</returns>
        Public ReadOnly Property DeviceType() As DeviceType
            Get
                Return _DeviceType
            End Get
        End Property
        ''' <summary>Gets instance of <see cref="DeviceInfo"/>-derived class appropriate for type of given device</summary>
        ''' <param name="DeviceInfo">Result of <see cref="API.RawInput.GetRawInputDeviceInfo"/>(<see cref="API.RawInput.DeviceInfoTypes.RIDI_DEVICEINFO"/>)</param>
        ''' <param name="DeviceHandle">Hande of device for which <paramref name="DeviceInfo"/> was obtained</param>
        ''' <returns>Instance of <see cref="DeviceInfo"/>-derived class according to device type</returns>
        ''' <exception cref="InvalidEnumArgumentException"><paramref name="DeviceInfo"/>.<see cref="API.RawInput.RID_DEVICE_INFO.dwType">dwType</see> is not member of <see cref="API.RawInput.DeviceTypes"/></exception>
        ''' <exception cref="ArgumentNullException"><paramref name="DeviceHandle"/> is <see cref="IntPtr.Zero"/></exception>
        Friend Shared Function GetDeviceInfo(ByVal DeviceInfo As API.RawInput.RID_DEVICE_INFO, ByVal DeviceHandle As IntPtr) As DeviceInfo
            Select Case DeviceInfo.dwType
                Case API.DeviceTypes.RIM_TYPEHID : Return New HidInfo(DeviceInfo.hid, DeviceHandle)
                Case API.DeviceTypes.RIM_TYPEKEYBOARD : Return New KeyboardInfo(DeviceInfo.keyboard, DeviceHandle)
                Case API.DeviceTypes.RIM_TYPEMOUSE : Return New MouseInfo(DeviceInfo.mouse, DeviceHandle)
                Case Else : Throw New InvalidEnumArgumentException("DeviceInfo.dwType", DeviceInfo.dwType, GetType(API.RawInput.DeviceTypes))
            End Select
        End Function
        ''' <summary>Gets device this instance hold information about</summary>
        ''' <returns>Instance of <see cref="InputDevice"/> class that equals to instance this info was created for</returns>
        Public Function GetDevice() As InputDevice
            Return New InputDevice(Me.DeviceHandle, Me.DeviceType)
        End Function
        ''' <summary>Gets name of the device</summary>
        ''' <exception cref="Win32Exception">Error while obtaining device name</exception>
        Public Function GetDeviceNameString$()
            Return GetDevice.GetDeviceNameString
        End Function
        ''' <summary>Gets parsed device name. Parsed device name provides more information about the device.</summary>
        ''' <exception cref="Win32Exception">Error while obtaining device name</exception>
        ''' <exception cref="FormatException">Device name obtained from <see cref="GetDeviceNameString"/> has unexpected format and cannot be parsed.</exception>
        ''' <seelaso cref="GetDeviceNameString"/><seelaso cref="RawDeviceName"/>
        Public Function GetDeviceName() As RawDeviceName
            Return New RawDeviceName(GetDeviceNameString)
        End Function
        ''' <summary>Reads device description form the registry</summary>
        ''' <returns>Human-readable device description</returns>
        ''' <exception cref="Win32Exception">Error while obtaining device name</exception>
        ''' <exception cref="FormatException">Device name obtained from <see cref="GetDeviceNameString"/> has unexpected format and cannot be parsed.</exception>
        ''' <exception cref="System.Security.SecurityException">The user does not have the permissions required to read from the registry key.</exception>
        ''' <exception cref="System.IO.IOException">The <see cref="Microsoft.Win32.RegistryKey" /> that contains the specified value has been marked for deletion.</exception>
        ''' <exception cref="System.UnauthorizedAccessException">The user does not have the necessary registry rights.</exception>
        ''' <seelaso cref="RawDeviceName.GetDeviceDescription"/>
        Public Function GetDeviceDescription() As String
            Return GetDeviceName.GetDeviceDescription
        End Function
    End Class
    ''' <summary>Provides information about mouse device</summary>
    Public NotInheritable Class MouseInfo
        Inherits DeviceInfo
        ''' <exception cref="ArgumentNullException"><paramref name="DeviceHandle"/> is <see cref="IntPtr.Zero"/></exception>
        Friend Sub New(ByVal MouseInfo As API.RawInput.RID_DEVICE_INFO_MOUSE, ByVal DeviceHandle As IntPtr)
            MyBase.New(RawInputT.DeviceType.Mouse, DeviceHandle)
            Me.Info = MouseInfo
        End Sub
        ''' <summary>Contains mouse information from API call</summary>
        Private Info As API.RawInput.RID_DEVICE_INFO_MOUSE
        ''' <summary>Gets ID for the mouse device.</summary>
        ''' <remarks>ID for the mouse device</remarks>
        Public ReadOnly Property DeviceId%()
            Get
                Return Info.dwId
            End Get
        End Property
        ''' <summary>Gets number of buttons for the mouse.</summary>
        ''' <returns>Number of buttons for the mouse.</returns>
        Public ReadOnly Property NumberOfButtons%()
            Get
                Return Info.dwNumberOfButtons
            End Get
        End Property
        ''' <summary>Gets number of data points per second.</summary>
        ''' <returns>Number of data points per second.</returns>
        ''' <remarks> This information may not be applicable for every mouse device.</remarks>
        Public ReadOnly Property SampleRate%()
            Get
                Return Info.dwSampleRate
            End Get
        End Property
        ''' <summary>Gets value indicating if the mouse has a wheel for hozirontal scrolling</summary>
        ''' <returns>TRUE if the mouse has a wheel for horizontal scrolling; otherwise, FALSE.</returns>
        ''' <remarks>Note:  This member is only supported under Windows Vista and later versions.</remarks>
        Public ReadOnly Property HasHorizontalWheel() As Boolean
            Get
                Return Info.fHasHorizontalWheel
            End Get
        End Property
    End Class
    ''' <summary>Provides information about keyboard device</summary>
    Public NotInheritable Class KeyboardInfo
        Inherits DeviceInfo
        ''' <exception cref="ArgumentNullException"><paramref name="DeviceHandle"/> is <see cref="IntPtr.Zero"/></exception>
        Friend Sub New(ByVal KeyboardInfo As API.RawInput.RID_DEVICE_INFO_KEYBOARD, ByVal DeviceHandle As IntPtr)
            MyBase.New(RawInputT.DeviceType.Keyboard, DeviceHandle)
            Me.Info = KeyboardInfo
        End Sub
        ''' <summary>Contains keyboard information from API call</summary>
        Private Info As API.RawInput.RID_DEVICE_INFO_KEYBOARD
        ''' <summary>Gets type of the keyboard. </summary>
        ''' <returns>Type of the keyboard.</returns>
        Public ReadOnly Property Type%()
            Get
                Return Info.dwType
            End Get
        End Property
        ''' <summary>Gets subtype of the keyboard. </summary>
        ''' <returns>Subtype of the keyboard.</returns>
        Public ReadOnly Property SubType%()
            Get
                Return Info.dwSubType
            End Get
        End Property
        ''' <summary>Gets scan code mode. </summary>
        ''' <returns>Scan code mode.</returns>
        Public ReadOnly Property KeyboardMode%()
            Get
                Return Info.dwKeyboardMode
            End Get
        End Property
        ''' <summary>Gets number of function keys on the keyboard.</summary>
        ''' <returns>Number of function keys on the keyboard.</returns>
        Public ReadOnly Property NumberOfFunctionKeys%()
            Get
                Return Info.dwNumberOfFunctionKeys
            End Get
        End Property
        ''' <summary>Gets number of LED indicators on the keyboard.</summary>
        ''' <returns>Number of LED indicators on the keyboard.</returns>
        Public ReadOnly Property NumberOfIndicators%()
            Get
                Return Info.dwNumberOfIndicators
            End Get
        End Property
        ''' <summary>Gets total number of keys on the keyboard. </summary>
        ''' <returns>Total number of keys on the keyboard.</returns>
        Public ReadOnly Property NumberOfKeys%()
            Get
                Return Info.dwNumberOfKeysTotal
            End Get
        End Property
    End Class
    ''' <summary>Provides information about HID device</summary>
    Public NotInheritable Class HidInfo
        Inherits DeviceInfo
        ''' <exception cref="ArgumentNullException"><paramref name="DeviceHandle"/> is <see cref="IntPtr.Zero"/></exception>
        Friend Sub New(ByVal HidInfo As API.RawInput.RID_DEVICE_INFO_HID, ByVal DeviceHandle As IntPtr)
            MyBase.New(RawInputT.DeviceType.Hid, DeviceHandle)
            Me.Info = HidInfo
        End Sub
        ''' <summary>Contains keyboard information from API call</summary>
        Private Info As API.RawInput.RID_DEVICE_INFO_HID
        ''' <summary>Gets vendor ID for the HID. </summary>
        ''' <returns>Vendor ID for the HID.</returns>
        Public ReadOnly Property VendorId%()
            Get
                Return Info.dwVendorId
            End Get
        End Property
        ''' <summary>Gets product ID for the HID. </summary>
        ''' <returns>Product ID for the HID.</returns>
        Public ReadOnly Property ProductId%()
            Get
                Return Info.dwProductId
            End Get
        End Property
        ''' <summary>Gets version number for the HID. </summary>
        ''' <returns>Version number for the HID.</returns>
        Public ReadOnly Property Version%()
            Get
                Return Info.dwVersionNumber
            End Get
        End Property
        ''' <summary>Gets top-level collection Usage Page for the device. </summary>
        ''' <returns>Top-level collection Usage Page for the device.</returns>
        Public ReadOnly Property UsagePage() As Integer
            Get
                Return Info.usUsagePage
            End Get
        End Property
        ''' <summary>Gets top-level collection Usage for the device. </summary>
        ''' <returns>Top-level collection Usage for the device.</returns>
        Public ReadOnly Property Usage%()
            Get
                Return Info.usUsage
            End Get
        End Property
    End Class

    ''' <summary>Raw input device types</summary>
    Public Enum DeviceType
        ''' <summary>The device is a keyboard.</summary>
        Keyboard = API.RawInput.DeviceTypes.RIM_TYPEKEYBOARD
        ''' <summary>The device is a mouse.</summary>
        Mouse = API.RawInput.DeviceTypes.RIM_TYPEMOUSE
        ''' <summary>The device is an Human Interface Device (HID) that is not a keyboard and not a mouse.</summary>
        Hid = API.RawInput.DeviceTypes.RIM_TYPEHID
    End Enum

    ''' <summary>Exception thrown when operation not allowed or not supported by curent state of raw input object is attempted</summary>
    Public Class RawInputException : Inherits InvalidOperationException
        ''' <summary>Initializes a new instance of the <see cref="RawInputException" /> class with a specified error message.</summary>
        ''' <param name="message">The message that describes the error.</param>
        Public Sub New(ByVal message$)
            MyBase.New(message)
        End Sub
        ''' <summary>Initializes a new instance of the <see cref="RawInputException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
        ''' <param name="message">The error message that explains the reason for the exception.</param>
        ''' <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException"/> parameter is not a null reference (Nothing in Visual Basic), the current exception is raised in a catch block that handles the inner exception.</param>
        Public Sub New(ByVal message$, ByVal innerException As Exception)
            MyBase.New(message, innerException)
        End Sub
    End Class

    ''' <summary>Contains extension methods used by raw input</summary>
    Public Module RawInputExtensions
        ''' <summary>Gets enumeration that lists defined usages for given usage page</summary>
        ''' <param name="UsagePage">Usage page to get usages for</param>
        ''' <returns>Type ther represents enumeration containing usages for given usgae page or null when there are no usages for given usage page or usage page is vendor-specific, <see cref="UsagePages.Undefined"/> or reserved.</returns>
        <Extension()> Function GetUsagesEnumeration(ByVal UsagePage As UsagePages) As Type
            Select Case UsagePage
                Case UsagePages.Undefined : Return Nothing
                Case UsagePages.GenericDesktopControls : Return GetType(Usages_GenericDesktopControls)
                Case UsagePages.SimulationControls : Return GetType(Usages_SimulationControls)
                Case UsagePages.VRControls : Return GetType(Usages_VRControls)
                Case UsagePages.SportControls : Return GetType(Usages_VRControls)
                Case UsagePages.GameControls : Return GetType(Usages_GameControls)
                Case UsagePages.GenericDeviceControls : Return GetType(Usages_GenericDeviceControls)
                Case UsagePages.KeyboardKeypad : Return GetType(Usages_KeyboardKeypad)
                Case UsagePages.LEDs : Return GetType(Usages_LEDs)
                Case UsagePages.Button : Return GetType(Usages_Button)
                Case UsagePages.Ordinal : Return GetType(Usages_Ordinal)
                Case UsagePages.Telephony : Return GetType(Usages_Telephony)
                Case UsagePages.Consumer : Return GetType(Usages_Consumer)
                Case UsagePages.Digitizer : Return GetType(Usages_Digitizer)
                Case UsagePages.PIDPage : Return GetType(Usages_PIDPage)
                Case UsagePages.Unicode : Return Nothing
                Case UsagePages.AlphanumericDisplay : Return GetType(Usages_AlphanumericDisplay)
                Case UsagePages.MedicalInstruments : Return GetType(Usages_MedicalInstruments)
                Case UsagePages.UsbMonitor : Return GetType(Usages_UsbMonitor)
                Case UsagePages.UsbEnumeratedValues : Return GetType(Usages_UsbEnumeratedValues)
                Case UsagePages.VesaVirtualControls : Return GetType(Usages_VesaVirtualControls)
                Case UsagePages.MonitorPage3 : Return Nothing
                Case UsagePages.PowerDevice : Return GetType(Usages_PowerDevice)
                Case UsagePages.BatterySystem : Return GetType(Usages_BatterySystem)
                Case UsagePages.PowerPage2 : Return Nothing
                Case UsagePages.PowerPage3 : Return Nothing
                Case UsagePages.BarCodeScanners : Return GetType(Usages_BarCodeScanners)
                Case UsagePages.Scale : Return GetType(Usages_Scale)
                Case UsagePages.MSRDevices : Return GetType(Usages_MSRDevices)
                Case UsagePages.PointOfScaleReserved : Return Nothing
                Case UsagePages.CameraControl : Return Nothing
                Case UsagePages.Arcade : Return GetType(Usages_Arcade)
                Case UsagePages.VendorDefinedMin To UsagePages.VendorDefinedMax : Return Nothing
                Case Else : Return Nothing
            End Select
        End Function
        ''' <summary>Gets array of all known usages for given usage page</summary>
        ''' <param name="UsagePage">Usage page to get usages for</param>
        ''' <returns>Array of values from enumeration obtained from <see cref="GetUsagesEnumeration"/>, if that function returns null this function returns an empty array</returns>
        ''' <param name="AddUnspecifiedMembers">When true, for enumerations that allows values that are not specified in them (such as <see cref="Usages_Button"/>), adds such values to the enumeration; when false only values that are member of the enumeration wil be returned.</param>
        <Extension()> Public Function GetUsages(ByVal UsagePage As UsagePages, Optional ByVal AddUnspecifiedMembers As Boolean = False) As [Enum]()
            Dim EnumType = UsagePage.GetUsagesEnumeration
            If EnumType Is Nothing Then Return New [Enum]() {}
            Dim ret = ValuesFromRangeAreValidAttribute.GetAllValues(EnumType)
            If ret Is Nothing Then Return New [Enum]() {}
            Return ret
        End Function
        ''' <summary>For member of any enumeration gets its usage type</summary>
        ''' <param name="Usage">Enumeration member representing usage to get usage type for</param>
        ''' <returns><see cref="UsageTypes">UsageType</see> for <paramref name="Usage"/>; if usage type is unknown, 0 is returned</returns>
        ''' <remarks>Usage is obtained from <see cref="UsageTypeAttribute"/> applied to enumeration member constant, or if <paramref name="Usage"/> is not member of its enumeration (according to <see cref="TypeTools.IsDefined"/>) and the enumeration has <see cref="ValuesFromRangeAreValidAttribute"/> attribute atached and <paramref name="Usage"/> fals into range of the attribute and enumeration has <see cref="UndefinedMembersUsageType"/> attribute attached, too, value of that attribute is returned</remarks>
        Public Function GetUsageType(ByVal Usage As [Enum]) As UsageTypes
            If Usage.IsDefined Then
                Dim cns = Usage.GetConstant
                Dim attr = cns.GetAttribute(Of UsageTypeAttribute)()
                If attr IsNot Nothing Then Return attr.UsageType Else Return 0
            Else
                Dim Attr = Usage.GetType.GetAttribute(Of UndefinedMembersUsageType)()
                If Attr IsNot Nothing Then
                    If ValuesFromRangeAreValidAttribute.IsValidValue(Usage) Then
                        Return Attr.UsageType
                    Else
                        Return 0
                    End If
                Else
                    Return 0
                End If
            End If
        End Function
#Region "Specific usgae page methods"
        ''' <summary>Gets usage type of given usage</summary>
        ''' <param name="Usage">Usage to get usage type for</param>
        ''' <returns>Usage type for given <paramref name="Usage"/>. If no usage type is defined returns 0.</returns>
        ''' <remarks>This function uses generic non-extension function <see cref="M:Tools.DevicesT.RawInputT.RawInputExtensions.GetUsageType(System.Enum)"/></remarks>
        <Extension()> Public Function GetUsageType(ByVal Usage As Usages_GenericDesktopControls) As UsageTypes
            Return GetUsageType(DirectCast(Usage, [Enum]))
        End Function
        ''' <summary>Gets usage type of given usage</summary>
        ''' <param name="Usage">Usage to get usage type for</param>
        ''' <returns>Usage type for given <paramref name="Usage"/>. If no usage type is defined returns 0.</returns>
        ''' <remarks>This function uses generic non-extension function <see cref="M:Tools.DevicesT.RawInputT.RawInputExtensions.GetUsageType(System.Enum)"/></remarks>
        <Extension()> Public Function GetUsageType(ByVal Usage As Usages_SimulationControls) As UsageTypes
            Return GetUsageType(DirectCast(Usage, [Enum]))
        End Function
        ''' <summary>Gets usage type of given usage</summary>
        ''' <param name="Usage">Usage to get usage type for</param>
        ''' <returns>Usage type for given <paramref name="Usage"/>. If no usage type is defined returns 0.</returns>
        ''' <remarks>This function uses generic non-extension function <see cref="M:Tools.DevicesT.RawInputT.RawInputExtensions.GetUsageType(System.Enum)"/></remarks>
        <Extension()> Public Function GetUsageType(ByVal Usage As Usages_VRControls) As UsageTypes
            Return GetUsageType(DirectCast(Usage, [Enum]))
        End Function
        ''' <summary>Gets usage type of given usage</summary>
        ''' <param name="Usage">Usage to get usage type for</param>
        ''' <returns>Usage type for given <paramref name="Usage"/>. If no usage type is defined returns 0.</returns>
        ''' <remarks>This function uses generic non-extension function <see cref="M:Tools.DevicesT.RawInputT.RawInputExtensions.GetUsageType(System.Enum)"/></remarks>
        <Extension()> Public Function GetUsageType(ByVal Usage As Usages_SportControls) As UsageTypes
            Return GetUsageType(DirectCast(Usage, [Enum]))
        End Function
        ''' <summary>Gets usage type of given usage</summary>
        ''' <param name="Usage">Usage to get usage type for</param>
        ''' <returns>Usage type for given <paramref name="Usage"/>. If no usage type is defined returns 0.</returns>
        ''' <remarks>This function uses generic non-extension function <see cref="M:Tools.DevicesT.RawInputT.RawInputExtensions.GetUsageType(System.Enum)"/></remarks>
        <Extension()> Public Function GetUsageType(ByVal Usage As Usages_GameControls) As UsageTypes
            Return GetUsageType(DirectCast(Usage, [Enum]))
        End Function
        ''' <summary>Gets usage type of given usage</summary>
        ''' <param name="Usage">Usage to get usage type for</param>
        ''' <returns>Usage type for given <paramref name="Usage"/>. If no usage type is defined returns 0.</returns>
        ''' <remarks>This function uses generic non-extension function <see cref="M:Tools.DevicesT.RawInputT.RawInputExtensions.GetUsageType(System.Enum)"/></remarks>
        <Extension()> Public Function GetUsageType(ByVal Usage As Usages_GenericDeviceControls) As UsageTypes
            Return GetUsageType(DirectCast(Usage, [Enum]))
        End Function
        ''' <summary>Gets usage type of given usage</summary>
        ''' <param name="Usage">Usage to get usage type for</param>
        ''' <returns>Usage type for given <paramref name="Usage"/>. If no usage type is defined returns 0.</returns>
        ''' <remarks>This function uses generic non-extension function <see cref="M:Tools.DevicesT.RawInputT.RawInputExtensions.GetUsageType(System.Enum)"/></remarks>
        <Extension()> Public Function GetUsageType(ByVal Usage As Usages_KeyboardKeypad) As UsageTypes
            Return GetUsageType(DirectCast(Usage, [Enum]))
        End Function
        ''' <summary>Gets usage type of given usage</summary>
        ''' <param name="Usage">Usage to get usage type for</param>
        ''' <returns>Usage type for given <paramref name="Usage"/>. If no usage type is defined returns 0.</returns>
        ''' <remarks>This function uses generic non-extension function <see cref="M:Tools.DevicesT.RawInputT.RawInputExtensions.GetUsageType(System.Enum)"/></remarks>
        <Extension()> Public Function GetUsageType(ByVal Usage As Usages_LEDs) As UsageTypes
            Return GetUsageType(DirectCast(Usage, [Enum]))
        End Function
        ''' <summary>Gets usage type of given usage</summary>
        ''' <param name="Usage">Usage to get usage type for</param>
        ''' <returns>Usage type for given <paramref name="Usage"/>. If no usage type is defined returns 0.</returns>
        ''' <remarks>This function uses generic non-extension function <see cref="M:Tools.DevicesT.RawInputT.RawInputExtensions.GetUsageType(System.Enum)"/></remarks>
        <Extension()> Public Function GetUsageType(ByVal Usage As Usages_Button) As UsageTypes
            Return GetUsageType(DirectCast(Usage, [Enum]))
        End Function
        ''' <summary>Gets usage type of given usage</summary>
        ''' <param name="Usage">Usage to get usage type for</param>
        ''' <returns>Usage type for given <paramref name="Usage"/>. If no usage type is defined returns 0.</returns>
        ''' <remarks>This function uses generic non-extension function <see cref="M:Tools.DevicesT.RawInputT.RawInputExtensions.GetUsageType(System.Enum)"/></remarks>
        <Extension()> Public Function GetUsageType(ByVal Usage As Usages_Ordinal) As UsageTypes
            Return GetUsageType(DirectCast(Usage, [Enum]))
        End Function
        ''' <summary>Gets usage type of given usage</summary>
        ''' <param name="Usage">Usage to get usage type for</param>
        ''' <returns>Usage type for given <paramref name="Usage"/>. If no usage type is defined returns 0.</returns>
        ''' <remarks>This function uses generic non-extension function <see cref="M:Tools.DevicesT.RawInputT.RawInputExtensions.GetUsageType(System.Enum)"/></remarks>
        <Extension()> Public Function GetUsageType(ByVal Usage As Usages_Telephony) As UsageTypes
            Return GetUsageType(DirectCast(Usage, [Enum]))
        End Function
        ''' <summary>Gets usage type of given usage</summary>
        ''' <param name="Usage">Usage to get usage type for</param>
        ''' <returns>Usage type for given <paramref name="Usage"/>. If no usage type is defined returns 0.</returns>
        ''' <remarks>This function uses generic non-extension function <see cref="M:Tools.DevicesT.RawInputT.RawInputExtensions.GetUsageType(System.Enum)"/></remarks>
        <Extension()> Public Function GetUsageType(ByVal Usage As Usages_Consumer) As UsageTypes
            Return GetUsageType(DirectCast(Usage, [Enum]))
        End Function
        ''' <summary>Gets usage type of given usage</summary>
        ''' <param name="Usage">Usage to get usage type for</param>
        ''' <returns>Usage type for given <paramref name="Usage"/>. If no usage type is defined returns 0.</returns>
        ''' <remarks>This function uses generic non-extension function <see cref="M:Tools.DevicesT.RawInputT.RawInputExtensions.GetUsageType(System.Enum)"/></remarks>
        <Extension()> Public Function GetUsageType(ByVal Usage As Usages_Digitizer) As UsageTypes
            Return GetUsageType(DirectCast(Usage, [Enum]))
        End Function
        ''' <summary>Gets usage type of given usage</summary>
        ''' <param name="Usage">Usage to get usage type for</param>
        ''' <returns>Usage type for given <paramref name="Usage"/>. If no usage type is defined returns 0.</returns>
        ''' <remarks>This function uses generic non-extension function <see cref="M:Tools.DevicesT.RawInputT.RawInputExtensions.GetUsageType(System.Enum)"/></remarks>
        <Extension()> Public Function GetUsageType(ByVal Usage As Usages_PIDPage) As UsageTypes
            Return GetUsageType(DirectCast(Usage, [Enum]))
        End Function
        ''' <summary>Gets usage type of given usage</summary>
        ''' <param name="Usage">Usage to get usage type for</param>
        ''' <returns>Usage type for given <paramref name="Usage"/>. If no usage type is defined returns 0.</returns>
        ''' <remarks>This function uses generic non-extension function <see cref="M:Tools.DevicesT.RawInputT.RawInputExtensions.GetUsageType(System.Enum)"/></remarks>
        <Extension()> Public Function GetUsageType(ByVal Usage As Usages_AlphanumericDisplay) As UsageTypes
            Return GetUsageType(DirectCast(Usage, [Enum]))
        End Function
        ''' <summary>Gets usage type of given usage</summary>
        ''' <param name="Usage">Usage to get usage type for</param>
        ''' <returns>Usage type for given <paramref name="Usage"/>. If no usage type is defined returns 0.</returns>
        ''' <remarks>This function uses generic non-extension function <see cref="M:Tools.DevicesT.RawInputT.RawInputExtensions.GetUsageType(System.Enum)"/></remarks>
        <Extension()> Public Function GetUsageType(ByVal Usage As Usages_MedicalInstruments) As UsageTypes
            Return GetUsageType(DirectCast(Usage, [Enum]))
        End Function
        ''' <summary>Gets usage type of given usage</summary>
        ''' <param name="Usage">Usage to get usage type for</param>
        ''' <returns>(Always zero) Usage type for given <paramref name="Usage"/>. If no usage type is defined returns 0.</returns>
        ''' <remarks>This function uses generic non-extension function <see cref="M:Tools.DevicesT.RawInputT.RawInputExtensions.GetUsageType(System.Enum)"/>
        ''' <para>Documentation <a href="http://www.usb.org/developers/devclass_docs/usbmon10.pdf">USB Monitor Control Class Specification</a> doe not provide information about usage types for this usage page, so that information is not included in the <see cref="Usages_UsbMonitor"/> enumeration. Thus this function always returns zero.</para></remarks>
        <Extension()> Public Function GetUsageType(ByVal Usage As Usages_UsbMonitor) As UsageTypes
            Return GetUsageType(DirectCast(Usage, [Enum]))
        End Function
        ''' <summary>Gets usage type of given usage</summary>
        ''' <param name="Usage">Usage to get usage type for</param>
        ''' <returns>(Always zero) Usage type for given <paramref name="Usage"/>. If no usage type is defined returns 0.</returns>
        ''' <remarks>This function uses generic non-extension function <see cref="M:Tools.DevicesT.RawInputT.RawInputExtensions.GetUsageType(System.Enum)"/>
        ''' <para>Documentation <a href="http://www.usb.org/developers/devclass_docs/usbmon10.pdf">USB Monitor Control Class Specification</a> doe not provide information about usage types for this usage page, so that information is not included in the <see cref="Usages_UsbMonitor"/> enumeration. Thus this function always returns zero.</para></remarks>
        <Extension()> Public Function GetUsageType(ByVal Usage As Usages_UsbEnumeratedValues) As UsageTypes
            Return GetUsageType(DirectCast(Usage, [Enum]))
        End Function
        ''' <summary>Gets usage type of given usage</summary>
        ''' <param name="Usage">Usage to get usage type for</param>
        ''' <returns>(Always zero) Usage type for given <paramref name="Usage"/>. If no usage type is defined returns 0.</returns>
        ''' <remarks>This function uses generic non-extension function <see cref="M:Tools.DevicesT.RawInputT.RawInputExtensions.GetUsageType(System.Enum)"/>
        ''' <para>Documentation <a href="http://www.usb.org/developers/devclass_docs/usbmon10.pdf">USB Monitor Control Class Specification</a> doe not provide information about usage types for this usage page, so that information is not included in the <see cref="Usages_UsbMonitor"/> enumeration. Thus this function always returns zero.</para></remarks>
        <Extension()> Public Function GetUsageType(ByVal Usage As Usages_VesaVirtualControls) As UsageTypes
            Return GetUsageType(DirectCast(Usage, [Enum]))
        End Function
        ''' <summary>Gets usage type of given usage</summary>
        ''' <param name="Usage">Usage to get usage type for</param>
        ''' <returns>Usage type for given <paramref name="Usage"/>. If no usage type is defined returns 0.</returns>
        ''' <remarks>This function uses generic non-extension function <see cref="M:Tools.DevicesT.RawInputT.RawInputExtensions.GetUsageType(System.Enum)"/></remarks>
        <Extension()> Public Function GetUsageType(ByVal Usage As Usages_PowerDevice) As UsageTypes
            Return GetUsageType(DirectCast(Usage, [Enum]))
        End Function
        ''' <summary>Gets usage type of given usage</summary>
        ''' <param name="Usage">Usage to get usage type for</param>
        ''' <returns>Usage type for given <paramref name="Usage"/>. If no usage type is defined returns 0.</returns>
        ''' <remarks>This function uses generic non-extension function <see cref="M:Tools.DevicesT.RawInputT.RawInputExtensions.GetUsageType(System.Enum)"/></remarks>
        <Extension()> Public Function GetUsageType(ByVal Usage As Usages_BatterySystem) As UsageTypes
            Return GetUsageType(DirectCast(Usage, [Enum]))
        End Function

        ''' <summary>Gets usage type of given usage</summary>
        ''' <param name="Usage">Usage to get usage type for</param>
        ''' <returns>Usage type for given <paramref name="Usage"/>. If no usage type is defined returns 0.</returns>
        ''' <remarks>This function uses generic non-extension function <see cref="M:Tools.DevicesT.RawInputT.RawInputExtensions.GetUsageType(System.Enum)"/></remarks>
        <Extension()> Public Function GetUsageType(ByVal Usage As Usages_BarCodeScanners) As UsageTypes
            Return GetUsageType(DirectCast(Usage, [Enum]))
        End Function
        ''' <summary>Gets usage type of given usage</summary>
        ''' <param name="Usage">Usage to get usage type for</param>
        ''' <returns>Usage type for given <paramref name="Usage"/>. If no usage type is defined returns 0.</returns>
        ''' <remarks>This function uses generic non-extension function <see cref="M:Tools.DevicesT.RawInputT.RawInputExtensions.GetUsageType(System.Enum)"/></remarks>
        <Extension()> Public Function GetUsageType(ByVal Usage As Usages_Scale) As UsageTypes
            Return GetUsageType(DirectCast(Usage, [Enum]))
        End Function
        ''' <summary>Gets usage type of given usage</summary>
        ''' <param name="Usage">Usage to get usage type for</param>
        ''' <returns>Usage type for given <paramref name="Usage"/>. If no usage type is defined returns 0.</returns>
        ''' <remarks>This function uses generic non-extension function <see cref="M:Tools.DevicesT.RawInputT.RawInputExtensions.GetUsageType(System.Enum)"/></remarks>
        <Extension()> Public Function GetUsageType(ByVal Usage As Usages_MSRDevices) As UsageTypes
            Return GetUsageType(DirectCast(Usage, [Enum]))
        End Function
        ''' <summary>Gets usage type of given usage</summary>
        ''' <param name="Usage">Usage to get usage type for</param>
        ''' <returns>Usage type for given <paramref name="Usage"/>. If no usage type is defined returns 0.</returns>
        ''' <remarks>This function uses generic non-extension function <see cref="M:Tools.DevicesT.RawInputT.RawInputExtensions.GetUsageType(System.Enum)"/></remarks>
        <Extension()> Public Function GetUsageType(ByVal Usage As Usages_Arcade) As UsageTypes
            Return GetUsageType(DirectCast(Usage, [Enum]))
        End Function
#End Region

    End Module
#Region "Usages"
    ''' <summary>Defines HID usage pages</summary>
    ''' <remarks>Values that are not defined in this enumeration and are not within range <see cref="UsagePages.VendorDefinedMin"/> ÷ <see cref="UsagePages.VendorDefinedMax"/> are reserved
    ''' <para>For more information see <a href="http://www.usb.org/developers/devclass_docs/Hut1_12.pdf">HID Usage Tables</a></para></remarks>
    Public Enum UsagePages As Integer
        ''' <summary>Undefined</summary>
        Undefined = &H0
        ''' <summary>Generic Desktop Controls   </summary>
        GenericDesktopControls = &H1
        ''' <summary>Simulation Controls   </summary>
        SimulationControls = &H2
        ''' <summary>VR Controls   </summary>
        VRControls = &H3
        ''' <summary>Sport Controls   </summary>
        SportControls = &H4
        ''' <summary>Game Controls   </summary>
        GameControls = &H5
        ''' <summary>Generic Device Controls   </summary>
        GenericDeviceControls = &H6
        ''' <summary>Keyboard/Keypad   </summary>
        KeyboardKeypad = &H7
        ''' <summary>LEDs </summary>
        LEDs = &H8
        ''' <summary>Button </summary>
        Button = &H9
        ''' <summary>Ordinal </summary>
        Ordinal = &HA
        ''' <summary>Telephony </summary>
        Telephony = &HB
        ''' <summary>Consumer </summary>
        Consumer = &HC
        ''' <summary>Digitizer </summary>
        Digitizer = &HD
        '''' <summary>Reserved </summary>
        'Reserved = &HE
        ''' <summary>USB Physical Interface Device definitions for force feedback and related devices.</summary>
        PIDPage = &HF
        ''' <summary>Unicode </summary>
        Unicode = &H10
        '''' <summary>Reserved  </summary>
        'Reserved = &H11 - 13
        ''' <summary>Alphanumeric Display   </summary>
        AlphanumericDisplay = &H14
        '''' <summary>Reserved  </summary>
        'Reserved = &H15 - 3.0F
        ''' <summary>Medical Instruments   </summary>
        MedicalInstruments = &H40
        '''' <summary>Reserved  </summary>
        'Reserved = &H41 - 7.0F
        ''' <summary>USB Device Class Definition for Monitor Devices (Monitor Page 0)</summary>
        UsbMonitor = &H80
        ''' <summary>USB Device Class Definition for Monitor Devices (Monitor Page 1)</summary>
        UsbEnumeratedValues = &H81
        ''' <summary>USB Device Class Definition for Monitor Devices (Monitor Page 2)</summary>
        VesaVirtualControls = &H82
        ''' <summary>USB Device Class Definition for Monitor Devices (Monitor Page 3). This page is reserved</summary>
        MonitorPage3 = &H83
        ''' <summary>USB Device Class Definition for Power Devices (Power Page 0)</summary>
        PowerDevice = &H84
        ''' <summary>USB Device Class Definition for Power Devices (Power Page 1)</summary>
        BatterySystem = &H85
        ''' <summary>USB Device Class Definition for Power Devices (Power Page 2). This page is reserved for future use.</summary>
        PowerPage2 = &H86
        ''' <summary>USB Device Class Definition for Power Devices (Power Page 3). This page is rewserved for future use.</summary>
        PowerPage3 = &H87
        '''' <summary>Reserved  </summary>
        'Reserved  = &h88-8B
        ''' <summary>Bar Code Scanner page (Point Of Sale 0)</summary>
        BarCodeScanners = &H8C
        ''' <summary>Scale page  (Point Of Sale 1, weighting devices) </summary>
        Scale = &H8D
        ''' <summary>Magnetic Stripe Reading (MSR)Devices  (Point Of Sale 2)</summary>
        MSRDevices = &H8E
        ''' <summary>Reserved Point of Sale pages   (Point Of Sale 3). This page is reserved.</summary>
        PointOfScaleReserved = &H8F
        ''' <summary>USB Device Class Definition for Image Class Devices.</summary>
        ''' <remarks>Usage page enumeration for this usage page is not available, because I've found it nowhere.</remarks>
        CameraControl = &H90
        ''' <summary>OAAF Definitions for arcade and coinop related Devices  </summary>
        Arcade = &H91
        '''' <summary> Reserved</summary>
        'Reserved = &H92 - FEFF
        ''' <summary>Minimal value for Vendor-defined</summary>
        VendorDefinedMin = &HFF00
        ''' <summary>Maximal value for Vendor-defined</summary>
        VendorDefinedMax = &HFFFF
    End Enum
    ''' <summary>Control-related usage types</summary>
    ''' <remarks>Value of type <see cref="UsageTypes"/> is usually single value. On rare occasions multiple flags are specified (OR-ed) indicating that multiple usages are possible.</remarks>
    <Flags()> _
    Public Enum UsageTypes
        ''' <summary>Linear control (LC; control related)</summary>
        LinearControl = 1
        ''' <summary>On-Off control (OOC; control related)</summary>
        OnOffControl = 2
        ''' <summary>Momentary control (MC; control related)</summary>
        MomentaryControl = 4
        ''' <summary>One shot control (OSC; control related)</summary>
        OneShotControl = 8
        ''' <summary>Re-trigger control (RTC; control related)</summary>
        ReTriggerControl = 16
        ''' <summary>Selector (Sel; data related)</summary>
        Selector = 32
        ''' <summary>Static value (SV; data related)</summary>
        StaticValue = 64
        ''' <summary>Stafic Flag (SF; data related)</summary>
        StaticFlag = 128
        ''' <summary>Dynamic value (DV; data related)</summary>
        DynamicValue = 256
        ''' <summary>Dynamic flag (DF; data related)</summary>
        DynamicFlag = 512
        ''' <summary>Named array (NAry; collection related)</summary>
        NamedArray = 1024
        ''' <summary>Application collection (CA; collection related)</summary>
        ApplicationCollection = 2048
        ''' <summary>Physical collection (CL; collection related)</summary>
        LogicalCollection = 4096
        ''' <summary>Physical collection (CP; collection related)</summary>
        PhysicalCollection = 8192
        ''' <summary>Usage switch (US; collection related)</summary>
        UsageSwitch = 16384
        ''' <summary>Usage modifier (UM; collection related)</summary>
        UsageModifier = 32768
        ''' <summary>Buffered bytes</summary>
        BufferedBytes = 65536
        ''' <summary>Item</summary>
        Item = 131072
    End Enum
    ''' <summary>Applied to constants in usage pages enumerations to define usage types of HID defice usages</summary>
    <EditorBrowsable(EditorBrowsableState.Advanced)> _
    <AttributeUsage(AttributeTargets.Field, AllowMultiple:=False, Inherited:=False)> _
    Public Class UsageTypeAttribute : Inherits Attribute
        ''' <summary>CTor from usage type</summary>
        ''' <param name="UsageType">OR-ed <see cref="UsageTypes"/> to initialize this instance with</param>
        Public Sub New(ByVal UsageType As UsageTypes)
            _UsageType = UsageType
        End Sub
        ''' <summary>Contains value of the <see cref="UsageType"/> property</summary>
        Private _UsageType As UsageTypes
        ''' <summary>Gets usage types applied by this attribute</summary>
        Public ReadOnly Property UsageType() As UsageTypes
            Get
                Return UsageType
            End Get
        End Property
    End Class
    ''' <summary>When applied to enumeration defines <see cref="UsageTypeAttribute"/> for values that are not defined in enumeration</summary>
    <EditorBrowsable(EditorBrowsableState.Advanced)> _
    <AttributeUsage(AttributeTargets.Enum, AllowMultiple:=False, Inherited:=False)> _
    Public Class UndefinedMembersUsageType : Inherits UsageTypeAttribute
        ''' <summary>CTor from usage type</summary>
        ''' <param name="UsageType">OR-ed <see cref="UsageTypes"/> to initialize this instance with</param>
        Public Sub New(ByVal UsageType As UsageTypes)
            MyBase.New(UsageType)
        End Sub
    End Class

    ''' <summary>Contains HID usage code s for <see cref="UsagePages.GenericDesktopControls"/> page</summary>
    ''' <remarks>Values not defined in this enumeration are reserved</remarks>
    Public Enum Usages_GenericDesktopControls
        ''' <summary>Undefined</summary>
        Undefined = &H0
        ''' <summary>Pointer</summary>
        <UsageType(UsageTypes.PhysicalCollection)> Pointer = &H1
        ''' <summary>Mouse</summary>
        <UsageType(UsageTypes.ApplicationCollection)> Mouse = &H2
        ''' <summary>Joystick</summary>
        <UsageType(UsageTypes.ApplicationCollection)> Joystick = &H4
        ''' <summary>Gamepad</summary>
        <UsageType(UsageTypes.ApplicationCollection)> Gamepad = &H5
        ''' <summary>Keyboard</summary>
        <UsageType(UsageTypes.ApplicationCollection)> Keyboard = &H6
        ''' <summary>Keypad</summary>
        <UsageType(UsageTypes.ApplicationCollection)> Keypad = &H7
        ''' <summary>Multi-axis controller</summary>
        <UsageType(UsageTypes.ApplicationCollection)> MultiAxisController = &H8
        ''' <summary>Tablet PC System Controls</summary>
        <UsageType(UsageTypes.ApplicationCollection)> TabletPCSystemControls = &H9
        ''' <summary>X</summary>
        <UsageType(UsageTypes.DynamicValue)> X = &H30
        ''' <summary>Y</summary>
        <UsageType(UsageTypes.DynamicValue)> Y = &H31
        ''' <summary>Z</summary>
        <UsageType(UsageTypes.DynamicValue)> Z = &H32
        ''' <summary>Rx</summary>
        <UsageType(UsageTypes.DynamicValue)> Rx = &H33
        ''' <summary>Ry</summary>
        <UsageType(UsageTypes.DynamicValue)> Ry = &H34
        ''' <summary>Rz</summary>
        <UsageType(UsageTypes.DynamicValue)> Rz = &H35
        ''' <summary>Slider</summary>
        <UsageType(UsageTypes.DynamicFlag)> Slider = &H36
        ''' <summary>Dial</summary>
        <UsageType(UsageTypes.DynamicValue)> Dial = &H37
        ''' <summary>Wheel</summary>
        <UsageType(UsageTypes.DynamicValue)> Wheel = &H38
        ''' <summary>Hat switch</summary>
        <UsageType(UsageTypes.DynamicValue)> HatSwitch = &H39
        ''' <summary>Counted Buffer</summary>
        <UsageType(UsageTypes.LogicalCollection)> CountedBuffer = &H3A
        ''' <summary>Byte Count</summary>
        <UsageType(UsageTypes.DynamicValue)> ByteCount = &H3B
        ''' <summary>Motion Wakeup</summary>
        <UsageType(UsageTypes.OneShotControl)> MotionWakeup = &H3C
        ''' <summary>Start</summary>
        <UsageType(UsageTypes.OnOffControl)> Start = &H3D
        ''' <summary>Select</summary>
        <UsageType(UsageTypes.OnOffControl)> [Select] = &H3E
        ''' <summary>Vx</summary>
        <UsageType(UsageTypes.DynamicValue)> Vx = &H40
        ''' <summary>Vy</summary>
        <UsageType(UsageTypes.DynamicValue)> Vy = &H41
        ''' <summary>Vz</summary>
        <UsageType(UsageTypes.DynamicValue)> Vz = &H42
        ''' <summary>Vbrx</summary>
        <UsageType(UsageTypes.DynamicValue)> Vbrx = &H43
        ''' <summary>Vbry</summary>
        <UsageType(UsageTypes.DynamicValue)> Vbry = &H44
        ''' <summary>Vbrz</summary>
        <UsageType(UsageTypes.DynamicValue)> Vbrz = &H45
        ''' <summary>Vno</summary>
        <UsageType(UsageTypes.DynamicValue)> Vno = &H46
        ''' <summary>Feature Notification</summary>
        <UsageType(UsageTypes.DynamicValue Or UsageTypes.DynamicFlag)> FeatureNotification = &H47
        ''' <summary>Resolution Multiplier</summary>
        <UsageType(UsageTypes.DynamicValue)> ResolutionMultiplier = &H48
        ''' <summary>System Control</summary>
        <UsageType(UsageTypes.ApplicationCollection)> SystemControl = &H80
        ''' <summary>System Power Down</summary>
        <UsageType(UsageTypes.OneShotControl)> SystemPowerDown = &H81
        ''' <summary>System Sleep</summary>
        <UsageType(UsageTypes.OneShotControl)> SystemSleep = &H82
        ''' <summary>System Wake Up</summary>
        <UsageType(UsageTypes.OneShotControl)> SystemWakeUp = &H83
        ''' <summary>System Context Menu</summary>
        <UsageType(UsageTypes.OneShotControl)> SystemContextMenu = &H84
        ''' <summary>System Main Menu</summary>
        <UsageType(UsageTypes.OneShotControl)> SystemMainMenu = &H85
        ''' <summary>System App Menu</summary>
        <UsageType(UsageTypes.OneShotControl)> SystemAppMenu = &H86
        ''' <summary>System Menu Help</summary>
        <UsageType(UsageTypes.OneShotControl)> SystemMenuHelp = &H87
        ''' <summary>System Menu Exit</summary>
        <UsageType(UsageTypes.OneShotControl)> SystemMenuExit = &H88
        ''' <summary>System Menu Select</summary>
        <UsageType(UsageTypes.OneShotControl)> SystemMenuSelect = &H89
        ''' <summary>System Menu Right</summary>
        <UsageType(UsageTypes.ReTriggerControl)> SystemMenuRight = &H8A
        ''' <summary>System Menu Left</summary>
        <UsageType(UsageTypes.ReTriggerControl)> SystemMenuLeft = &H8B
        ''' <summary>System Menu Up</summary>
        <UsageType(UsageTypes.ReTriggerControl)> SystemMenuUp = &H8C
        ''' <summary>System Menu Down</summary>
        <UsageType(UsageTypes.ReTriggerControl)> SystemMenuDown = &H8D
        ''' <summary>System Cold Restart</summary>
        <UsageType(UsageTypes.OneShotControl)> SystemColdRestart = &H8E
        ''' <summary>System Warm Restart</summary>
        <UsageType(UsageTypes.OneShotControl)> SystemWarmRestart = &H8F
        ''' <summary>D-pad Up</summary>
        <UsageType(UsageTypes.OnOffControl)> DPadUp = &H90
        ''' <summary>D-pad Down</summary>
        <UsageType(UsageTypes.OnOffControl)> DPadDown = &H91
        ''' <summary>D-pad Right</summary>
        <UsageType(UsageTypes.OnOffControl)> DPadRight = &H92
        ''' <summary>D-pad Left</summary>
        <UsageType(UsageTypes.OnOffControl)> DPadLeft = &H93
        ''' <summary>System Dock</summary>
        <UsageType(UsageTypes.OneShotControl)> SystemDock = &HA0
        ''' <summary>System Undock</summary>
        <UsageType(UsageTypes.OneShotControl)> SystemUndock = &HA1
        ''' <summary>System Setup</summary>
        <UsageType(UsageTypes.OneShotControl)> SystemSetup = &HA2
        ''' <summary>System Break</summary>
        <UsageType(UsageTypes.OneShotControl)> SystemBreak = &HA3
        ''' <summary>System Debugger Break</summary>
        <UsageType(UsageTypes.OneShotControl)> SystemDebuggerBreak = &HA4
        ''' <summary>Application Break</summary>
        <UsageType(UsageTypes.OneShotControl)> ApplicationBreak = &HA5
        ''' <summary>Application Debugger Break</summary>
        <UsageType(UsageTypes.OneShotControl)> ApplicationDebuggerBreak = &HA6
        ''' <summary>System Speaker Mute</summary>
        <UsageType(UsageTypes.OneShotControl)> SystemSpeakerMute = &HA7
        ''' <summary>System Hibernate</summary>
        <UsageType(UsageTypes.OneShotControl)> SystemHibernate = &HA8
        ''' <summary>System Display Invert</summary>
        <UsageType(UsageTypes.OneShotControl)> SystemDisplayInvert = &HB0
        ''' <summary>System Display Internal</summary>
        <UsageType(UsageTypes.OneShotControl)> SystemDisplayInternal = &HB1
        ''' <summary>System Display External</summary>
        <UsageType(UsageTypes.OneShotControl)> SystemDisplayExternal = &HB2
        ''' <summary>System Display Both</summary>
        <UsageType(UsageTypes.OneShotControl)> SystemDisplayBoth = &HB3
        ''' <summary>System Display Dual</summary>
        <UsageType(UsageTypes.OneShotControl)> SystemDisplayDual = &HB4
        ''' <summary>System Display Toggle Int/Ext</summary>
        <UsageType(UsageTypes.OneShotControl)> SystemDisplayToggleIntExt = &HB5
        ''' <summary>System Display Swap Primary/Secondary</summary>
        <UsageType(UsageTypes.OneShotControl)> SystemDisplaySwapPrimarySecondary = &HB6
        ''' <summary>System Display LCD Autoscale</summary>
        <UsageType(UsageTypes.OneShotControl)> SystemDisplayLCDAutoscale = &HB7
    End Enum

    ''' <summary>Contains HID usage code s for <see cref="UsagePages.SimulationControls"/> page</summary>
    ''' <remarks>Values not defined in this enumeration are reserved</remarks>
    Public Enum Usages_SimulationControls
        ''' <summary>Undefined</summary>
        Undefined = 0
        ''' <summary>Flight Simulation Device</summary>
        <UsageType(UsageTypes.ApplicationCollection)> Flight = &H1
        ''' <summary>Automobile Simulation Device</summary>
        <UsageType(UsageTypes.ApplicationCollection)> Automobile = &H2
        ''' <summary>Tank Simulation Device</summary>
        <UsageType(UsageTypes.ApplicationCollection)> Tank = &H3
        ''' <summary>Spaceship Simulation Device</summary>
        <UsageType(UsageTypes.ApplicationCollection)> Spaceship = &H4
        ''' <summary>Submarine Simulation Device</summary>
        <UsageType(UsageTypes.ApplicationCollection)> SubmarineS = &H5
        ''' <summary>Sailing Simulation Device</summary>
        <UsageType(UsageTypes.ApplicationCollection)> Sailing = &H6
        ''' <summary>Motorcycle Simulation Device</summary>
        <UsageType(UsageTypes.ApplicationCollection)> Motorcycle = &H7
        ''' <summary>Sports Simulation Device</summary>
        <UsageType(UsageTypes.ApplicationCollection)> Sports = &H8
        ''' <summary>Airplane Simulation Device</summary>
        <UsageType(UsageTypes.ApplicationCollection)> Airplane = &H9
        ''' <summary>Helicopter Simulation Device</summary>
        <UsageType(UsageTypes.ApplicationCollection)> Helicopter = &HA
        ''' <summary>Magic Carpet</summary>
        <UsageType(UsageTypes.ApplicationCollection)> MagicCarpet = &HB
        ''' <summary>Bicycle Simulation Device</summary>
        <UsageType(UsageTypes.ApplicationCollection)> Bicycle = &HC
        ''' <summary>Flight Control Stick</summary>
        <UsageType(UsageTypes.ApplicationCollection)> FlightControlStick = &H20
        ''' <summary>Flight Stick</summary>
        <UsageType(UsageTypes.ApplicationCollection)> FlightStick = &H21
        ''' <summary>Cyclic Control</summary>
        <UsageType(UsageTypes.PhysicalCollection)> CyclicControl = &H22
        ''' <summary>Cyclic Trim</summary>
        <UsageType(UsageTypes.PhysicalCollection)> CyclicTrim = &H23
        ''' <summary>Flight Yoke</summary>
        <UsageType(UsageTypes.ApplicationCollection)> FlightYoke = &H24
        ''' <summary>Track Control</summary>
        <UsageType(UsageTypes.PhysicalCollection)> TrackControl = &H25
        ''' <summary>Aileron</summary>
        <UsageType(UsageTypes.DynamicValue)> Aileron = &HB0
        ''' <summary>Aileron Trim</summary>
        <UsageType(UsageTypes.DynamicValue)> AileronTrim = &HB1
        ''' <summary>Anti-Torque Control</summary>
        <UsageType(UsageTypes.DynamicValue)> AntiTorqueControl = &HB2
        ''' <summary>Autopilot Enable</summary>
        <UsageType(UsageTypes.OnOffControl)> AutopilotEnable = &HB3
        ''' <summary>Chaff Release</summary>
        <UsageType(UsageTypes.OneShotControl)> ChaffRelease = &HB4
        ''' <summary>Collective Control</summary>
        <UsageType(UsageTypes.DynamicValue)> CollectiveControl = &HB5
        ''' <summary>Dive Brake</summary>
        <UsageType(UsageTypes.DynamicValue)> DiveBrake = &HB6
        ''' <summary>Electronic Countermeasures</summary>
        <UsageType(UsageTypes.OnOffControl)> ElectronicCountermeasures = &HB7
        ''' <summary>Elevator</summary>
        <UsageType(UsageTypes.DynamicValue)> Elevator = &HB8
        ''' <summary>Elevator Trim</summary>
        <UsageType(UsageTypes.DynamicValue)> ElevatorTrim = &HB9
        ''' <summary>Rudder</summary>
        <UsageType(UsageTypes.DynamicValue)> Rudder = &HBA
        ''' <summary>Throttle</summary>
        <UsageType(UsageTypes.DynamicValue)> Throttle = &HBB
        ''' <summary>Flight Communications</summary>
        <UsageType(UsageTypes.OnOffControl)> FlightCommunications = &HBC
        ''' <summary>Flare Release</summary>
        <UsageType(UsageTypes.OneShotControl)> FlareRelease = &HBD
        ''' <summary>Landing Gear</summary>
        <UsageType(UsageTypes.OnOffControl)> LandingGear = &HBE
        ''' <summary>Toe Brake</summary>
        <UsageType(UsageTypes.DynamicValue)> ToeBrake = &HBF
        ''' <summary>Trigger</summary>
        <UsageType(UsageTypes.MomentaryControl)> Trigger = &HC0
        ''' <summary>Weapons Arm</summary>
        <UsageType(UsageTypes.OnOffControl)> WeaponsArm = &HC1
        ''' <summary>Weapons Select</summary>
        <UsageType(UsageTypes.OneShotControl)> WeaponsSelect = &HC2
        ''' <summary>Wing Flaps</summary>
        <UsageType(UsageTypes.DynamicValue)> WingFlaps = &HC3
        ''' <summary>Accelerator</summary>
        <UsageType(UsageTypes.DynamicValue)> Accelerator = &HC4
        ''' <summary>Brake</summary>
        <UsageType(UsageTypes.DynamicValue)> Brake = &HC5
        ''' <summary>Clutch</summary>
        <UsageType(UsageTypes.DynamicValue)> Clutch = &HC6
        ''' <summary>Shifter</summary>
        <UsageType(UsageTypes.DynamicValue)> Shifter = &HC7
        ''' <summary>Steering</summary>
        <UsageType(UsageTypes.DynamicValue)> Steering = &HC8
        ''' <summary>Turret Direction</summary>
        <UsageType(UsageTypes.DynamicValue)> TurretDirection = &HC9
        ''' <summary>Barrel Elevation</summary>
        <UsageType(UsageTypes.DynamicValue)> BarrelElevation = &HCA
        ''' <summary>Dive Plane</summary>
        <UsageType(UsageTypes.DynamicValue)> DivePlane = &HCB
        ''' <summary>Ballast</summary>
        <UsageType(UsageTypes.DynamicValue)> Ballast = &HCC
        ''' <summary>Bicycle Crank</summary>
        <UsageType(UsageTypes.DynamicValue)> BicycleCrank = &HCD
        ''' <summary>Handle Bars</summary>
        <UsageType(UsageTypes.DynamicValue)> HandleBars = &HCE
        ''' <summary>Front Brake</summary>
        <UsageType(UsageTypes.DynamicValue)> FrontBrake = &HCF
        ''' <summary>Rear Brake</summary>
        <UsageType(UsageTypes.DynamicValue)> RearBrake = &HD0
    End Enum

    ''' <summary>Contains HID usage code s for <see cref="UsagePages.VRControls"/> page</summary>
    ''' <remarks>Values not defined in this enumeration are reserved</remarks>
    Public Enum Usages_VRControls
        ''' <summary>Unidentified</summary>
        Unidentified = 0
        ''' <summary>Belt</summary>
        <UsageType(UsageTypes.ApplicationCollection)> Belt = &H1
        ''' <summary>Body Suit</summary>
        <UsageType(UsageTypes.ApplicationCollection)> BodySuit = &H2
        ''' <summary>Flexor</summary>
        <UsageType(UsageTypes.PhysicalCollection)> Flexor = &H3
        ''' <summary>Glove</summary>
        <UsageType(UsageTypes.ApplicationCollection)> Glove = &H4
        ''' <summary>Head Tracker</summary>
        <UsageType(UsageTypes.PhysicalCollection)> HeadTracker = &H5
        ''' <summary>Head Mounted Display</summary>
        <UsageType(UsageTypes.ApplicationCollection)> HeadMountedDisplay = &H6
        ''' <summary>Hand Tracker</summary>
        <UsageType(UsageTypes.ApplicationCollection)> HandTracker = &H7
        ''' <summary>Oculometer</summary>
        <UsageType(UsageTypes.ApplicationCollection)> Oculometer = &H8
        ''' <summary>Vest</summary>
        <UsageType(UsageTypes.ApplicationCollection)> Vest = &H9
        ''' <summary>Animatronic Device</summary>
        <UsageType(UsageTypes.ApplicationCollection)> AnimatronicDevice = &HA
        ''' <summary>Stereo Enable</summary>
        <UsageType(UsageTypes.OnOffControl)> StereoEnable = &H20
        ''' <summary>Display Enable</summary>
        <UsageType(UsageTypes.OnOffControl)> DisplayEnable = &H21
    End Enum

    ''' <summary>Contains HID usage code s for <see cref="UsagePages.SportControls"/> page</summary>
    ''' <remarks>Values not defined in this enumeration are reserved</remarks>
    Public Enum Usages_SportControls
        ''' <summary>Unidentified</summary>
        Unidentified = 0
        ''' <summary>Baseball Bat</summary>
        <UsageType(UsageTypes.ApplicationCollection)> BaseballBat = &H1
        ''' <summary>Golf Club</summary>
        <UsageType(UsageTypes.ApplicationCollection)> GolfClub = &H2
        ''' <summary>Rowing Machine</summary>
        <UsageType(UsageTypes.ApplicationCollection)> RowingMachine = &H3
        ''' <summary>Treadmill</summary>
        <UsageType(UsageTypes.ApplicationCollection)> Treadmill = &H4
        ''' <summary>Oar</summary>
        <UsageType(UsageTypes.DynamicValue)> Oar = &H30
        ''' <summary>Slope</summary>
        <UsageType(UsageTypes.DynamicValue)> Slope = &H31
        ''' <summary>Rate</summary>
        <UsageType(UsageTypes.DynamicValue)> Rate = &H32
        ''' <summary>Stick Speed</summary>
        <UsageType(UsageTypes.DynamicValue)> StickSpeed = &H33
        ''' <summary>Stick Face Angle</summary>
        <UsageType(UsageTypes.DynamicValue)> StickFaceAngle = &H34
        ''' <summary>Stick Heel/Toe</summary>
        <UsageType(UsageTypes.DynamicValue)> StickHeelToe = &H35
        ''' <summary>Stick Follow Through</summary>
        <UsageType(UsageTypes.DynamicValue)> StickFollowThrough = &H36
        ''' <summary>Stick Tempo</summary>
        <UsageType(UsageTypes.DynamicValue)> StickTempo = &H37
        ''' <summary>Stick Type</summary>
        <UsageType(UsageTypes.NamedArray)> StickType = &H38
        ''' <summary>Stick Height</summary>
        <UsageType(UsageTypes.DynamicValue)> StickHeight = &H39
        ''' <summary>Putter</summary>
        <UsageType(UsageTypes.Selector)> Putter = &H50
        ''' <summary>1 Iron</summary>
        <UsageType(UsageTypes.Selector)> Iron1 = &H51
        ''' <summary>2 Iron</summary>
        <UsageType(UsageTypes.Selector)> Iron2 = &H52
        ''' <summary>3 Iron</summary>
        <UsageType(UsageTypes.Selector)> Iron3 = &H53
        ''' <summary>4 Iron</summary>
        <UsageType(UsageTypes.Selector)> Iron4 = &H54
        ''' <summary>5 Iron</summary>
        <UsageType(UsageTypes.Selector)> Iron5 = &H55
        ''' <summary>6 Iron</summary>
        <UsageType(UsageTypes.Selector)> Iron6 = &H56
        ''' <summary>7 Iron</summary>
        <UsageType(UsageTypes.Selector)> Iron7 = &H57
        ''' <summary>8 Iron</summary>
        <UsageType(UsageTypes.Selector)> Iron8 = &H58
        ''' <summary>9 Iron</summary>
        <UsageType(UsageTypes.Selector)> Iron9 = &H59
        ''' <summary>10 Iron</summary>
        <UsageType(UsageTypes.Selector)> Iron10 = &H5A
        ''' <summary>11 Iron</summary>
        <UsageType(UsageTypes.Selector)> Iron11 = &H5B
        ''' <summary>Sand Wedge</summary>
        <UsageType(UsageTypes.Selector)> SandWedge = &H5C
        ''' <summary>Loft Wedge</summary>
        <UsageType(UsageTypes.Selector)> LoftWedge = &H5D
        ''' <summary>Power Wedge</summary>
        <UsageType(UsageTypes.Selector)> PowerWedge = &H5E
        ''' <summary>1 Wood</summary>
        <UsageType(UsageTypes.Selector)> Wood1 = &H5F
        ''' <summary>3 Wood</summary>
        <UsageType(UsageTypes.Selector)> Wood3 = &H60
        ''' <summary>5 Wood</summary>
        <UsageType(UsageTypes.Selector)> Wood5 = &H61
        ''' <summary>7 Wood</summary>
        <UsageType(UsageTypes.Selector)> Wood7 = &H62
        ''' <summary>9 Wood</summary>
        <UsageType(UsageTypes.Selector)> Wood9 = &H63
    End Enum

    ''' <summary>Contains HID usage code s for <see cref="UsagePages.GameControls"/> page</summary>
    ''' <remarks>Values not defined in this enumeration are reserved</remarks>
    Public Enum Usages_GameControls
        ''' <summary>Unidentified</summary>
        Unidentified = 0
        ''' <summary>3D Game Controller</summary>
        <UsageType(UsageTypes.ApplicationCollection)> GameController3D = &H1
        ''' <summary>Pinball Device</summary>
        <UsageType(UsageTypes.ApplicationCollection)> Pinball = &H2
        ''' <summary>Gun Device</summary>
        <UsageType(UsageTypes.ApplicationCollection)> Gun = &H3
        ''' <summary>Point of View</summary>
        <UsageType(UsageTypes.PhysicalCollection)> PointOfView = &H20
        ''' <summary>Turn Right/Left</summary>
        <UsageType(UsageTypes.DynamicValue)> TurnRightLeft = &H21
        ''' <summary>Pitch Forward/Backward</summary>
        <UsageType(UsageTypes.DynamicValue)> PitchForwardBackward = &H22
        ''' <summary>Roll Right/Left</summary>
        <UsageType(UsageTypes.DynamicValue)> RollRightLeft = &H23
        ''' <summary>Move Right/Left</summary>
        <UsageType(UsageTypes.DynamicValue)> MoveRightLeft = &H24
        ''' <summary>Move Forward/Backward</summary>
        <UsageType(UsageTypes.DynamicValue)> MoveForwardBackward = &H25
        ''' <summary>Move Up/Down</summary>
        <UsageType(UsageTypes.DynamicValue)> MoveUpDown = &H26
        ''' <summary>Lean Right/Left</summary>
        <UsageType(UsageTypes.DynamicValue)> LeanRightLeft = &H27
        ''' <summary>Lean Forward/Backward</summary>
        <UsageType(UsageTypes.DynamicValue)> LeanForwardBackward = &H28
        ''' <summary>Height of POV</summary>
        <UsageType(UsageTypes.DynamicValue)> HeightOfPOV = &H29
        ''' <summary>Flipper</summary>
        <UsageType(UsageTypes.MomentaryControl)> Flipper = &H2A
        ''' <summary>Secondary Flipper</summary>
        <UsageType(UsageTypes.MomentaryControl)> SecondaryFlipper = &H2B
        ''' <summary>Bump</summary>
        <UsageType(UsageTypes.MomentaryControl)> Bump = &H2C
        ''' <summary>New Game</summary>
        <UsageType(UsageTypes.OneShotControl)> NewGame = &H2D
        ''' <summary>Shoot Ball</summary>
        <UsageType(UsageTypes.OneShotControl)> ShootBall = &H2E
        ''' <summary>Player</summary>
        <UsageType(UsageTypes.OneShotControl)> Player = &H2F
        ''' <summary>Gun Bolt</summary>
        <UsageType(UsageTypes.OnOffControl)> GunBolt = &H30
        ''' <summary>Gun Clip</summary>
        <UsageType(UsageTypes.OnOffControl)> GunClip = &H31
        ''' <summary>Gun Selector</summary>
        <UsageType(UsageTypes.NamedArray)> GunSelector = &H32
        ''' <summary>Gun Single Shot</summary>
        <UsageType(UsageTypes.Selector)> GunSingleShot = &H33
        ''' <summary>Gun Burst</summary>
        <UsageType(UsageTypes.Selector)> GunBurst = &H34
        ''' <summary>Gun Automatic</summary>
        <UsageType(UsageTypes.Selector)> GunAutomatic = &H35
        ''' <summary>Gun Safety</summary>
        <UsageType(UsageTypes.OnOffControl)> GunSafety = &H36
        ''' <summary>Gamepad Fire/Jump</summary>
        <UsageType(UsageTypes.LogicalCollection)> GamepadFireJump = &H37
        ''' <summary>Gamepad Trigger</summary>
        <UsageType(UsageTypes.LogicalCollection)> GamepadTrigger = &H39
    End Enum

    ''' <summary>Contains HID usage code s for <see cref="UsagePages.GenericDeviceControls "/> page</summary>
    ''' <remarks>Values not defined in this enumeration are reserved</remarks>
    Public Enum Usages_GenericDeviceControls
        ''' <summary>Unidentified</summary>
        Unidentified = 0
        ''' <summary>Battery Strength</summary>
        <UsageType(UsageTypes.DynamicValue)> BatteryStrength = &H20
        ''' <summary>Wireless Channel</summary>
        <UsageType(UsageTypes.DynamicValue)> WirelessChannel = &H21
        ''' <summary>Wireless ID</summary>
        <UsageType(UsageTypes.DynamicValue)> WirelessID = &H22
        ''' <summary>Discover Wireless Control</summary>                
        <UsageType(UsageTypes.OneShotControl)> DiscoverWirelessControl = &H23
        ''' <summary>Security Code Character Entered</summary>
        <UsageType(UsageTypes.OneShotControl)> SecurityCodeCharacterEntered = &H24
        ''' <summary>Security Code Character Erased</summary>
        <UsageType(UsageTypes.OneShotControl)> SecurityCodeCharacterErased = &H25
        ''' <summary>Security Code Cleared</summary>
        <UsageType(UsageTypes.OneShotControl)> SecurityCodeCleared = &H26
    End Enum

    ''' <summary>Contains HID usage code s for <see cref="UsagePages.KeyboardKeypad"/> page</summary>
    ''' <remarks>Values not defined in this enumeration are reserved.
    ''' <para>All keys but <see cref="Usages_KeyboardKeypad.LeftControl"/> and <see cref="Usages_KeyboardKeypad.RightGUI"/> has usage type <see cref="UsageTypes.Selector"/>.</para></remarks>
    Public Enum Usages_KeyboardKeypad
        ''' <summary>Reserved (no event indicated)</summary>
        <UsageType(UsageTypes.Selector)> ReservedNoEventIndicated = &H0
        ''' <summary>Keyboard ErrorRollOver</summary>
        <UsageType(UsageTypes.Selector)> ErrorRollOver = &H1
        ''' <summary>Keyboard POSTFail</summary>
        <UsageType(UsageTypes.Selector)> POSTFail = &H2
        ''' <summary>Keyboard ErrorUndefined</summary>
        <UsageType(UsageTypes.Selector)> ErrorUndefined = &H3
#Region "Letters"
        ''' <summary>Keyboard a and A</summary>
        <UsageType(UsageTypes.Selector)> A = &H4
        ''' <summary>Keyboard b and B</summary>
        <UsageType(UsageTypes.Selector)> B = &H5
        ''' <summary>Keyboard c and C</summary>
        <UsageType(UsageTypes.Selector)> C = &H6
        ''' <summary>Keyboard d and D</summary>
        <UsageType(UsageTypes.Selector)> D = &H7
        ''' <summary>Keyboard e and E</summary>
        <UsageType(UsageTypes.Selector)> E = &H8
        ''' <summary>Keyboard f and F</summary>
        <UsageType(UsageTypes.Selector)> F = &H9
        ''' <summary>Keyboard g and G</summary>
        <UsageType(UsageTypes.Selector)> G = &HA
        ''' <summary>Keyboard h and H</summary>
        <UsageType(UsageTypes.Selector)> H = &HB
        ''' <summary>Keyboard i and I</summary>
        <UsageType(UsageTypes.Selector)> I = &HC
        ''' <summary>Keyboard j and J</summary>
        <UsageType(UsageTypes.Selector)> J = &HD
        ''' <summary>Keyboard k and K</summary>
        <UsageType(UsageTypes.Selector)> K = &HE
        ''' <summary>Keyboard l and L</summary>
        <UsageType(UsageTypes.Selector)> L = &HF
        ''' <summary>Keyboard m and M</summary>
        <UsageType(UsageTypes.Selector)> M = &H10
        ''' <summary>Keyboard n and N</summary>
        <UsageType(UsageTypes.Selector)> N = &H11
        ''' <summary>Keyboard o and O</summary>
        <UsageType(UsageTypes.Selector)> O = &H12
        ''' <summary>Keyboard p and P</summary>
        <UsageType(UsageTypes.Selector)> P = &H13
        ''' <summary>Keyboard r and R</summary>
        <UsageType(UsageTypes.Selector)> R = &H15
        ''' <summary>Keyboard s and S</summary>
        <UsageType(UsageTypes.Selector)> S = &H16
        ''' <summary>Keyboard t and T</summary>
        <UsageType(UsageTypes.Selector)> T = &H17
        ''' <summary>Keyboard u and U</summary>
        <UsageType(UsageTypes.Selector)> U = &H18
        ''' <summary>Keyboard v and V</summary>
        <UsageType(UsageTypes.Selector)> V = &H19
        ''' <summary>Keyboard w and W</summary>
        <UsageType(UsageTypes.Selector)> W = &H1A
        ''' <summary>Keyboard x and X</summary>
        <UsageType(UsageTypes.Selector)> X = &H1B
        ''' <summary>Keyboard y and Y</summary>
        <UsageType(UsageTypes.Selector)> Y = &H1C
        ''' <summary>Keyboard z and Z</summary>
        <UsageType(UsageTypes.Selector)> Z = &H1D
#End Region
#Region "Shift numbers"
        ''' <summary>Keyboard 1 and !</summary>
        <UsageType(UsageTypes.Selector)> D1 = &H1E
        ''' <summary>Keyboard 2 and @</summary>
        <UsageType(UsageTypes.Selector)> D2 = &H1F
        ''' <summary>Keyboard 3 and #</summary>
        <UsageType(UsageTypes.Selector)> D3 = &H20
        ''' <summary>Keyboard 4 and $</summary>
        <UsageType(UsageTypes.Selector)> D4 = &H21
        ''' <summary>Keyboard 5 and %</summary>
        <UsageType(UsageTypes.Selector)> D5 = &H22
        ''' <summary>Keyboard 6 and ^</summary>
        <UsageType(UsageTypes.Selector)> D6 = &H23
        ''' <summary>Keyboard 7 and &amp;</summary>
        <UsageType(UsageTypes.Selector)> D7 = &H24
        ''' <summary>Keyboard 8 and *</summary>
        <UsageType(UsageTypes.Selector)> D8 = &H25
        ''' <summary>Keyboard 9 and (</summary>
        <UsageType(UsageTypes.Selector)> D9 = &H26
        ''' <summary>Keyboard 0 and )</summary>
        <UsageType(UsageTypes.Selector)> D0 = &H27
#End Region
        ''' <summary>Keyboard Return (ENTER)</summary>
        <UsageType(UsageTypes.Selector)> Enter = &H28
        ''' <summary>Keyboard ESCAPE</summary>
        <UsageType(UsageTypes.Selector)> Escape = &H29
        ''' <summary>Keyboard DELETE (Backspace)</summary>
        <UsageType(UsageTypes.Selector)> DeleteBackspace = &H2A
        ''' <summary>Keyboard Tab</summary>
        <UsageType(UsageTypes.Selector)> Tab = &H2B
#Region "Interpunction and diacritic"
        ''' <summary>Keyboard Spacebar</summary>
        <UsageType(UsageTypes.Selector)> Spacebar = &H2C
        ''' <summary>Keyboard - and (underscore)</summary>
        <UsageType(UsageTypes.Selector)> Minus = &H2D
        ''' <summary>Keyboard = and +</summary>
        <UsageType(UsageTypes.Selector)> Equals = &H2E
        ''' <summary>Keyboard [ and {</summary>
        <UsageType(UsageTypes.Selector)> OpenBrace = &H2F
        ''' <summary>Keyboard ] and }</summary>
        <UsageType(UsageTypes.Selector)> CloseBrace = &H30
        ''' <summary>Keyboard \ and |</summary>
        <UsageType(UsageTypes.Selector)> Backslash = &H31
        ''' <summary>Keyboard Non-US # and ~</summary>
        <UsageType(UsageTypes.Selector)> Hash = &H32
        ''' <summary>Keyboard ; and :</summary>
        <UsageType(UsageTypes.Selector)> Semicolon = &H33
        ''' <summary>Keyboard ‘ and “</summary>
        <UsageType(UsageTypes.Selector)> GraveAccentAndDoubleGraveAccent = &H34
        ''' <summary>Keyboard Grave Accent and Tilde</summary>
        <UsageType(UsageTypes.Selector)> GraveAccentAndTilde = &H35
        ''' <summary>Keyboard , and &lt;</summary>
        <UsageType(UsageTypes.Selector)> Comma = &H36
        ''' <summary>Keyboard . and ></summary>
        <UsageType(UsageTypes.Selector)> Dot = &H37
        ''' <summary>Keyboard / and ?</summary>
        <UsageType(UsageTypes.Selector)> Slash = &H38
#End Region
        ''' <summary>Keyboard Caps Lock</summary>
        <UsageType(UsageTypes.Selector)> CapsLock = &H39
#Region "F1-12"
        ''' <summary>Keyboard F1</summary>
        <UsageType(UsageTypes.Selector)> F1 = &H3A
        ''' <summary>Keyboard F2</summary>
        <UsageType(UsageTypes.Selector)> F2 = &H3B
        ''' <summary>Keyboard F3</summary>
        <UsageType(UsageTypes.Selector)> F3 = &H3C
        ''' <summary>Keyboard F4</summary>
        <UsageType(UsageTypes.Selector)> F4 = &H3D
        ''' <summary>Keyboard F5</summary>
        <UsageType(UsageTypes.Selector)> F5 = &H3E
        ''' <summary>Keyboard F6</summary>
        <UsageType(UsageTypes.Selector)> F6 = &H3F
        ''' <summary>Keyboard F7</summary>
        <UsageType(UsageTypes.Selector)> F7 = &H40
        ''' <summary>Keyboard F8</summary>
        <UsageType(UsageTypes.Selector)> F8 = &H41
        ''' <summary>Keyboard F9</summary>
        <UsageType(UsageTypes.Selector)> F9 = &H42
        ''' <summary>Keyboard F10</summary>
        <UsageType(UsageTypes.Selector)> F10 = &H43
        ''' <summary>Keyboard F11</summary>
        <UsageType(UsageTypes.Selector)> F11 = &H44
        ''' <summary>Keyboard F12</summary>
        <UsageType(UsageTypes.Selector)> F12 = &H45
#End Region
        ''' <summary>Keyboard PrintScreen</summary>
        <UsageType(UsageTypes.Selector)> PrintScreen = &H46
        ''' <summary>Keyboard Scroll Lock</summary>
        <UsageType(UsageTypes.Selector)> ScrollLock = &H47
        ''' <summary>Keyboard Pause</summary>
        <UsageType(UsageTypes.Selector)> Pause = &H48
        ''' <summary>Keyboard Insert</summary>
        <UsageType(UsageTypes.Selector)> Insert = &H49
        ''' <summary>Keyboard Home</summary>
        <UsageType(UsageTypes.Selector)> Home = &H4A
        ''' <summary>Keyboard PageUp</summary>
        <UsageType(UsageTypes.Selector)> PageUp = &H4B
        ''' <summary>Keyboard Delete Forward</summary>
        <UsageType(UsageTypes.Selector)> DeleteForward = &H4C
        ''' <summary>Keyboard End</summary>
        <UsageType(UsageTypes.Selector)> [End] = &H4D
        ''' <summary>Keyboard PageDown</summary>
        <UsageType(UsageTypes.Selector)> PageDown = &H4E
        ''' <summary>Keyboard RightArrow</summary>
        <UsageType(UsageTypes.Selector)> RightArrow = &H4F
        ''' <summary>Keyboard LeftArrow</summary>
        <UsageType(UsageTypes.Selector)> LeftArrow = &H50
        ''' <summary>Keyboard DownArrow</summary>
        <UsageType(UsageTypes.Selector)> DownArrow = &H51
        ''' <summary>Keyboard UpArrow</summary>
        <UsageType(UsageTypes.Selector)> UpArrow = &H52
        ''' <summary>Keypad Num Lock and Clear</summary>
        <UsageType(UsageTypes.Selector)> KeypadNumLockAndClear = &H53
        ''' <summary>Keypad /</summary>
        <UsageType(UsageTypes.Selector)> KeypadSlash = &H54
        ''' <summary>Keypad *</summary>
        <UsageType(UsageTypes.Selector)> KeypadAsterisk = &H55
        ''' <summary>Keypad -</summary>
        <UsageType(UsageTypes.Selector)> KeypadMinus = &H56
        ''' <summary>Keypad +</summary>
        <UsageType(UsageTypes.Selector)> KeypadPlus = &H57
        ''' <summary>Keypad ENTER</summary>
        <UsageType(UsageTypes.Selector)> KeypadENTER = &H58
        ''' <summary>Keypad 1 and End</summary>
#Region "Numbers"
        <UsageType(UsageTypes.Selector)> Keypad1 = &H59
        ''' <summary>Keypad 2 and Down Arrow</summary>
        <UsageType(UsageTypes.Selector)> Keypad2 = &H5A
        ''' <summary>Keypad 3 and PageDn</summary>
        <UsageType(UsageTypes.Selector)> Keypad3 = &H5B
        ''' <summary>Keypad 4 and Left Arrow</summary>
        <UsageType(UsageTypes.Selector)> Keypad4 = &H5C
        ''' <summary>Keypad 5</summary>
        <UsageType(UsageTypes.Selector)> Keypad5 = &H5D
        ''' <summary>Keypad 6 and Right Arrow</summary>
        <UsageType(UsageTypes.Selector)> Keypad6 = &H5E
        ''' <summary>Keypad 7 and Home</summary>
        <UsageType(UsageTypes.Selector)> Keypad7 = &H5F
        ''' <summary>Keypad 8 and Up Arrow</summary>
        <UsageType(UsageTypes.Selector)> Keypad8 = &H60
        ''' <summary>Keypad 9 and PageUp</summary>
        <UsageType(UsageTypes.Selector)> Keypad9 = &H61
        ''' <summary>Keypad 0 and Insert</summary>
        <UsageType(UsageTypes.Selector)> Keypad0 = &H62
#End Region
        ''' <summary>Keypad . and Delete</summary>
        <UsageType(UsageTypes.Selector)> KeypadDot = &H63
        ''' <summary>Keyboard Non-US \ and |</summary>
        <UsageType(UsageTypes.Selector)> Backslash2 = &H64
        ''' <summary>Keyboard Application</summary>
        <UsageType(UsageTypes.Selector)> Application = &H65
        ''' <summary>Keyboard Power</summary>
        <UsageType(UsageTypes.Selector)> Power = &H66
        ''' <summary>Keypad =</summary>
        <UsageType(UsageTypes.Selector)> KeypadEquals = &H67
#Region "F13-F24"
        ''' <summary>Keyboard F13</summary>
        <UsageType(UsageTypes.Selector)> F13 = &H68
        ''' <summary>Keyboard F14</summary>
        <UsageType(UsageTypes.Selector)> F14 = &H69
        ''' <summary>Keyboard F15</summary>
        <UsageType(UsageTypes.Selector)> F15 = &H6A
        ''' <summary>Keyboard F16</summary>
        <UsageType(UsageTypes.Selector)> F16 = &H6B
        ''' <summary>Keyboard F17</summary>
        <UsageType(UsageTypes.Selector)> F17 = &H6C
        ''' <summary>Keyboard F18</summary>
        <UsageType(UsageTypes.Selector)> F18 = &H6D
        ''' <summary>Keyboard F19</summary>
        <UsageType(UsageTypes.Selector)> F19 = &H6E
        ''' <summary>Keyboard F20</summary>
        <UsageType(UsageTypes.Selector)> F20 = &H6F
        ''' <summary>Keyboard F21</summary>
        <UsageType(UsageTypes.Selector)> F21 = &H70
        ''' <summary>Keyboard F22</summary>
        <UsageType(UsageTypes.Selector)> F22 = &H71
        ''' <summary>Keyboard F23</summary>
        <UsageType(UsageTypes.Selector)> F23 = &H72
        ''' <summary>Keyboard F24</summary>
        <UsageType(UsageTypes.Selector)> F24 = &H73
#End Region
        ''' <summary>Keyboard Execute</summary>
        <UsageType(UsageTypes.Selector)> Execute = &H74
        ''' <summary>Keyboard Help</summary>
        <UsageType(UsageTypes.Selector)> Help = &H75
        ''' <summary>Keyboard Menu</summary>
        <UsageType(UsageTypes.Selector)> Menu = &H76
        ''' <summary>Keyboard Select</summary>
        <UsageType(UsageTypes.Selector)> [Select] = &H77
        ''' <summary>Keyboard Stop</summary>
        <UsageType(UsageTypes.Selector)> [Stop] = &H78
        ''' <summary>Keyboard Again</summary>
        <UsageType(UsageTypes.Selector)> Again = &H79
        ''' <summary>Keyboard Undo</summary>
        <UsageType(UsageTypes.Selector)> Undo = &H7A
        ''' <summary>Keyboard Cut</summary>
        <UsageType(UsageTypes.Selector)> Cut = &H7B
        ''' <summary>Keyboard Copy</summary>
        <UsageType(UsageTypes.Selector)> Copy = &H7C
        ''' <summary>Keyboard Paste</summary>
        <UsageType(UsageTypes.Selector)> Paste = &H7D
        ''' <summary>Keyboard Find</summary>
        <UsageType(UsageTypes.Selector)> Find = &H7E
        ''' <summary>Keyboard Mute</summary>
        <UsageType(UsageTypes.Selector)> Mute = &H7F
        ''' <summary>Keyboard Volume Up</summary>
        <UsageType(UsageTypes.Selector)> VolumeUp = &H80
        ''' <summary>Keyboard Volume Down</summary>
        <UsageType(UsageTypes.Selector)> VolumeDown = &H81
        ''' <summary>Keyboard Locking Caps Lock</summary>
        <UsageType(UsageTypes.Selector)> LockingCapsLock = &H82
        ''' <summary>Keyboard Locking Num Lock</summary>
        <UsageType(UsageTypes.Selector)> LockingNumLock = &H83
        ''' <summary>Keyboard Locking Scroll Lock</summary>
        <UsageType(UsageTypes.Selector)> LockingScrollLock = &H84
        ''' <summary>Keypad Comma</summary>
        <UsageType(UsageTypes.Selector)> KeypadComma = &H85
        ''' <summary>Keypad Equal Sign</summary>
        <UsageType(UsageTypes.Selector)> KeypadEqualSign = &H86
        ''' <summary>Keyboard International1</summary>
        <UsageType(UsageTypes.Selector)> International1 = &H87
        ''' <summary>Keyboard International2</summary>
        <UsageType(UsageTypes.Selector)> International2 = &H88
        ''' <summary>Keyboard International3</summary>
        <UsageType(UsageTypes.Selector)> International3 = &H89
        ''' <summary>Keyboard International4</summary>
        <UsageType(UsageTypes.Selector)> International4 = &H8A
        ''' <summary>Keyboard International5</summary>
        <UsageType(UsageTypes.Selector)> International5 = &H8B
        ''' <summary>Keyboard International6</summary>
        <UsageType(UsageTypes.Selector)> International6 = &H8C
        ''' <summary>Keyboard International7</summary>
        <UsageType(UsageTypes.Selector)> International7 = &H8D
        ''' <summary>Keyboard International8</summary>
        <UsageType(UsageTypes.Selector)> International8 = &H8E
        ''' <summary>Keyboard International9</summary>
        <UsageType(UsageTypes.Selector)> International9 = &H8F
        ''' <summary>Keyboard LANG1</summary>
        <UsageType(UsageTypes.Selector)> LANG1 = &H90
        ''' <summary>Keyboard LANG2</summary>
        <UsageType(UsageTypes.Selector)> LANG2 = &H91
        ''' <summary>Keyboard LANG3</summary>
        <UsageType(UsageTypes.Selector)> LANG3 = &H92
        ''' <summary>Keyboard LANG4</summary>
        <UsageType(UsageTypes.Selector)> LANG4 = &H93
        ''' <summary>Keyboard LANG5</summary>
        <UsageType(UsageTypes.Selector)> LANG5 = &H94
        ''' <summary>Keyboard LANG6</summary>
        <UsageType(UsageTypes.Selector)> LANG6 = &H95
        ''' <summary>Keyboard LANG7</summary>
        <UsageType(UsageTypes.Selector)> LANG7 = &H96
        ''' <summary>Keyboard LANG8</summary>
        <UsageType(UsageTypes.Selector)> LANG8 = &H97
        ''' <summary>Keyboard LANG9</summary>
        <UsageType(UsageTypes.Selector)> LANG9 = &H98
        ''' <summary>Keyboard Alternate Erase</summary>
        <UsageType(UsageTypes.Selector)> AlternateErase = &H99
        ''' <summary>Keyboard SysReq/Attention</summary>
        <UsageType(UsageTypes.Selector)> SysReqAttention = &H9A
        ''' <summary>Keyboard Cancel</summary>
        <UsageType(UsageTypes.Selector)> Cancel = &H9B
        ''' <summary>Keyboard Clear</summary>
        <UsageType(UsageTypes.Selector)> Clear = &H9C
        ''' <summary>Keyboard Prior</summary>
        <UsageType(UsageTypes.Selector)> Prior = &H9D
        ''' <summary>Keyboard Return</summary>
        <UsageType(UsageTypes.Selector)> [Return] = &H9E
        ''' <summary>Keyboard Separator</summary>
        <UsageType(UsageTypes.Selector)> Separator = &H9F
        ''' <summary>Keyboard Out</summary>
        <UsageType(UsageTypes.Selector)> Out = &HA0
        ''' <summary>Keyboard Oper</summary>
        <UsageType(UsageTypes.Selector)> Oper = &HA1
        ''' <summary>Keyboard Clear/Again</summary>
        <UsageType(UsageTypes.Selector)> ClearAgain = &HA2
        ''' <summary>Keyboard CrSel/Props</summary>
        <UsageType(UsageTypes.Selector)> CrSelProps = &HA3
        ''' <summary>Keyboard ExSel</summary>
        <UsageType(UsageTypes.Selector)> ExSel = &HA4
        ''' <summary>Keypad 00</summary>
        <UsageType(UsageTypes.Selector)> Keypad00 = &HB0
        ''' <summary>Keypad 000</summary>
        <UsageType(UsageTypes.Selector)> Keypad000 = &HB1
        ''' <summary>Thousands Separator</summary>
        <UsageType(UsageTypes.Selector)> ThousandsSeparator = &HB2
        ''' <summary>Decimal Separator </summary>
        <UsageType(UsageTypes.Selector)> DecimalSeparator = &HB3
        ''' <summary>Currency Unit</summary>
        <UsageType(UsageTypes.Selector)> CurrencyUnit = &HB4
        ''' <summary>Currency Sub-unit</summary>
        <UsageType(UsageTypes.Selector)> CurrencySubUnit = &HB5
        ''' <summary>Keypad (</summary>
        <UsageType(UsageTypes.Selector)> KeypadOpenBrace = &HB6
        ''' <summary>Keypad )</summary>
        <UsageType(UsageTypes.Selector)> KeypadCloseBrace = &HB7
        ''' <summary>Keypad {</summary>
        <UsageType(UsageTypes.Selector)> KeypadOpenBracket = &HB8
        ''' <summary>Keypad }</summary>
        <UsageType(UsageTypes.Selector)> KeypadCloseBracket = &HB9
        ''' <summary>Keypad Tab</summary>
        <UsageType(UsageTypes.Selector)> KeypadTab = &HBA
        ''' <summary>Keypad Backspace</summary>
        <UsageType(UsageTypes.Selector)> KeypadBackspace = &HBB
        ''' <summary>Keypad A</summary>
        <UsageType(UsageTypes.Selector)> KeypadA = &HBC
        ''' <summary>Keypad B</summary>
        <UsageType(UsageTypes.Selector)> KeypadB = &HBD
        ''' <summary>Keypad C</summary>
        <UsageType(UsageTypes.Selector)> KeypadC = &HBE
        ''' <summary>Keypad D</summary>
        <UsageType(UsageTypes.Selector)> KeypadD = &HBF
        ''' <summary>Keypad E</summary>
        <UsageType(UsageTypes.Selector)> KeypadE = &HC0
        ''' <summary>Keypad F</summary>
        <UsageType(UsageTypes.Selector)> KeypadF = &HC1
        ''' <summary>Keypad XOR</summary>
        <UsageType(UsageTypes.Selector)> KeypadXOR = &HC2
        ''' <summary>Keypad ^</summary>
        <UsageType(UsageTypes.Selector)> KeypadCaret = &HC3
        ''' <summary>Keypad %</summary>
        <UsageType(UsageTypes.Selector)> KeypadPercent = &HC4
        ''' <summary>Keypad &lt;</summary>
        <UsageType(UsageTypes.Selector)> KeypadLessThan = &HC5
        ''' <summary>Keypad ></summary>
        <UsageType(UsageTypes.Selector)> KeypadGreateThan = &HC6
        ''' <summary>Keypad amp;</summary>
        <UsageType(UsageTypes.Selector)> KeypadAndpresand = &HC7
        ''' <summary>Keypad &amp;&amp;</summary>
        <UsageType(UsageTypes.Selector)> KeypadAndpresands = &HC8
        ''' <summary>Keypad |</summary>
        <UsageType(UsageTypes.Selector)> KeypadPipe = &HC9
        ''' <summary>Keypad ||</summary>
        <UsageType(UsageTypes.Selector)> KeypadPipes = &HCA
        ''' <summary>Keypad :</summary>
        <UsageType(UsageTypes.Selector)> KeypadColon = &HCB
        ''' <summary>Keypad #</summary>
        <UsageType(UsageTypes.Selector)> KeypadHash = &HCC
        ''' <summary>Keypad Space</summary>
        <UsageType(UsageTypes.Selector)> KeypadSpace = &HCD
        ''' <summary>Keypad @</summary>
        <UsageType(UsageTypes.Selector)> KeypadAt = &HCE
        ''' <summary>Keypad !</summary>
        <UsageType(UsageTypes.Selector)> KeypadExclamation = &HCF
        ''' <summary>Keypad Memory Store</summary>
        <UsageType(UsageTypes.Selector)> KeypadMemoryStore = &HD0
        ''' <summary>Keypad Memory Recall</summary>
        <UsageType(UsageTypes.Selector)> KeypadMemoryRecall = &HD1
        ''' <summary>Keypad Memory Clear</summary>
        <UsageType(UsageTypes.Selector)> KeypadMemoryClear = &HD2
        ''' <summary>Keypad Memory Add</summary>
        <UsageType(UsageTypes.Selector)> KeypadMemoryAdd = &HD3
        ''' <summary>Keypad Memory Subtract</summary>
        <UsageType(UsageTypes.Selector)> KeypadMemorySubtract = &HD4
        ''' <summary>Keypad Memory Multiply</summary>
        <UsageType(UsageTypes.Selector)> KeypadMemoryMultiply = &HD5
        ''' <summary>Keypad Memory Divide</summary>
        <UsageType(UsageTypes.Selector)> KeypadMemoryDivide = &HD6
        ''' <summary>Keypad +/-</summary>
        <UsageType(UsageTypes.Selector)> KeypadPlusMinus = &HD7
        ''' <summary>Keypad Clear</summary>
        <UsageType(UsageTypes.Selector)> KeypadClear = &HD8
        ''' <summary>Keypad Clear Entry</summary>
        <UsageType(UsageTypes.Selector)> KeypadClearEntry = &HD9
        ''' <summary>Keypad Binary</summary>
        <UsageType(UsageTypes.Selector)> KeypadBinary = &HDA
        ''' <summary>Keypad Octal</summary>
        <UsageType(UsageTypes.Selector)> KeypadOctal = &HDB
        ''' <summary>Keypad Decimal</summary>
        <UsageType(UsageTypes.Selector)> KeypadDecimal = &HDC
        ''' <summary>Keyboard LeftControl</summary>
        <UsageType(UsageTypes.DynamicValue)> LeftControl = &HE0
        ''' <summary>Keyboard LeftShift</summary>
        <UsageType(UsageTypes.Selector)> LeftShift = &HE1
        ''' <summary>Keyboard LeftAlt</summary>
        <UsageType(UsageTypes.Selector)> LeftAlt = &HE2
        ''' <summary>Keyboard Left GUI</summary>
        <UsageType(UsageTypes.Selector)> LeftGUI = &HE3
        ''' <summary>Keyboard RightControl</summary>
        <UsageType(UsageTypes.Selector)> RightControl = &HE4
        ''' <summary>Keyboard RightShift</summary>
        <UsageType(UsageTypes.Selector)> RightShift = &HE5
        ''' <summary>Keyboard RightAlt</summary>
        <UsageType(UsageTypes.Selector)> RightAlt = &HE6
        ''' <summary>Keyboard Right GUI</summary>
        <UsageType(UsageTypes.DynamicValue)> RightGUI = &HE7
    End Enum

    ''' <summary>Contains HID usage code s for <see cref="UsagePages.LEDs "/> page</summary>
    ''' <remarks>Values not defined in this enumeration are reserved</remarks>
    Public Enum Usages_LEDs
        ''' <summary>Unidentified</summary>
        Unidentified = 0
        ''' <summary>Num Lock</summary>
        <UsageType(UsageTypes.OnOffControl)> NumLock = &H1
        ''' <summary>Caps Lock</summary>
        <UsageType(UsageTypes.OnOffControl)> CapsLock = &H2
        ''' <summary>Scroll Lock</summary>
        <UsageType(UsageTypes.OnOffControl)> ScrollLock = &H3
        ''' <summary>Compose</summary>
        <UsageType(UsageTypes.OnOffControl)> Compose = &H4
        ''' <summary>Kana</summary>
        <UsageType(UsageTypes.OnOffControl)> Kana = &H5
        ''' <summary>Power</summary>
        <UsageType(UsageTypes.OnOffControl)> Power = &H6
        ''' <summary>Shift</summary>
        <UsageType(UsageTypes.OnOffControl)> Shift = &H7
        ''' <summary>Do Not Disturb</summary>
        <UsageType(UsageTypes.OnOffControl)> DoNotDisturb = &H8
        ''' <summary>Mute</summary>
        <UsageType(UsageTypes.OnOffControl)> Mute = &H9
        ''' <summary>Tone Enable</summary>
        <UsageType(UsageTypes.OnOffControl)> ToneEnable = &HA
        ''' <summary>High Cut Filter</summary>
        <UsageType(UsageTypes.OnOffControl)> HighCutFilter = &HB
        ''' <summary>Low Cut Filter</summary>
        <UsageType(UsageTypes.OnOffControl)> LowCutFilter = &HC
        ''' <summary>Equalizer Enable</summary>
        <UsageType(UsageTypes.OnOffControl)> EqualizerEnable = &HD
        ''' <summary>Sound Field On</summary>
        <UsageType(UsageTypes.OnOffControl)> SoundFieldOn = &HE
        ''' <summary>Surround On</summary>
        <UsageType(UsageTypes.OnOffControl)> SurroundOn = &HF
        ''' <summary>Repeat</summary>
        <UsageType(UsageTypes.OnOffControl)> Repeat = &H10
        ''' <summary>Stereo</summary>
        <UsageType(UsageTypes.OnOffControl)> Stereo = &H11
        ''' <summary>Sampling Rate Detect</summary>
        <UsageType(UsageTypes.OnOffControl)> SamplingRateDetect = &H12
        ''' <summary>Spinning</summary>
        <UsageType(UsageTypes.OnOffControl)> Spinning = &H13
        ''' <summary>CAV</summary>
        <UsageType(UsageTypes.OnOffControl)> CAV = &H14
        ''' <summary>CLV</summary>
        <UsageType(UsageTypes.OnOffControl)> CLV = &H15
        ''' <summary>Recording Format Detect</summary>
        <UsageType(UsageTypes.OnOffControl)> RecordingFormatDetect = &H16
        ''' <summary>Off-Hook</summary>
        <UsageType(UsageTypes.OnOffControl)> OffHook = &H17
        ''' <summary>Ring</summary>
        <UsageType(UsageTypes.OnOffControl)> Ring = &H18
        ''' <summary>Message Waiting</summary>
        <UsageType(UsageTypes.OnOffControl)> MessageWaiting = &H19
        ''' <summary>Data Mode</summary>
        <UsageType(UsageTypes.OnOffControl)> DataMode = &H1A
        ''' <summary>Battery Operation</summary>
        <UsageType(UsageTypes.OnOffControl)> BatteryOperation = &H1B
        ''' <summary>Battery OK</summary>
        <UsageType(UsageTypes.OnOffControl)> BatteryOK = &H1C
        ''' <summary>Battery Low</summary>
        <UsageType(UsageTypes.OnOffControl)> BatteryLow = &H1D
        ''' <summary>Speaker</summary>
        <UsageType(UsageTypes.OnOffControl)> Speaker = &H1E
        ''' <summary>Head Set</summary>
        <UsageType(UsageTypes.OnOffControl)> HeadSet = &H1F
        ''' <summary>Hold</summary>
        <UsageType(UsageTypes.OnOffControl)> Hold = &H20
        ''' <summary>Microphone</summary>
        <UsageType(UsageTypes.OnOffControl)> Microphone = &H21
        ''' <summary>Coverage</summary>
        <UsageType(UsageTypes.OnOffControl)> Coverage = &H22
        ''' <summary>Night Mode</summary>
        <UsageType(UsageTypes.OnOffControl)> NightMode = &H23
        ''' <summary>Send Calls</summary>
        <UsageType(UsageTypes.OnOffControl)> SendCalls = &H24
        ''' <summary>Call Pickup</summary>
        <UsageType(UsageTypes.OnOffControl)> CallPickup = &H25
        ''' <summary>Conference</summary>
        <UsageType(UsageTypes.OnOffControl)> Conference = &H26
        ''' <summary>Stand-by</summary>
        <UsageType(UsageTypes.OnOffControl)> StandBy = &H27
        ''' <summary>Camera On</summary>
        <UsageType(UsageTypes.OnOffControl)> CameraOn = &H28
        ''' <summary>Camera Off</summary>
        <UsageType(UsageTypes.OnOffControl)> CameraOff = &H29
        ''' <summary>On-Line</summary>
        <UsageType(UsageTypes.OnOffControl)> OnLine = &H2A
        ''' <summary>Off-Line</summary>
        <UsageType(UsageTypes.OnOffControl)> OffLine = &H2B
        ''' <summary>Busy</summary>
        <UsageType(UsageTypes.OnOffControl)> Busy = &H2C
        ''' <summary>Ready</summary>
        <UsageType(UsageTypes.OnOffControl)> Ready = &H2D
        ''' <summary>Paper-Out</summary>
        <UsageType(UsageTypes.OnOffControl)> PaperOut = &H2E
        ''' <summary>Paper-Jam</summary>
        <UsageType(UsageTypes.OnOffControl)> PaperJam = &H2F
        ''' <summary>Remote</summary>
        <UsageType(UsageTypes.OnOffControl)> Remote = &H30
        ''' <summary>Forward</summary>
        <UsageType(UsageTypes.OnOffControl)> Forward = &H31
        ''' <summary>Reverse</summary>
        <UsageType(UsageTypes.OnOffControl)> Reverse = &H32
        ''' <summary>Stop</summary>
        <UsageType(UsageTypes.OnOffControl)> [Stop] = &H33
        ''' <summary>Rewind</summary>
        <UsageType(UsageTypes.OnOffControl)> Rewind = &H34
        ''' <summary>Fast Forward</summary>
        <UsageType(UsageTypes.OnOffControl)> FastForward = &H35
        ''' <summary>Play</summary>
        <UsageType(UsageTypes.OnOffControl)> Play = &H36
        ''' <summary>Pause</summary>
        <UsageType(UsageTypes.OnOffControl)> Pause = &H37
        ''' <summary>Record</summary>
        <UsageType(UsageTypes.OnOffControl)> Record = &H38
        ''' <summary>Error</summary>
        <UsageType(UsageTypes.OnOffControl)> [Error] = &H39
        ''' <summary>Usage Selected Indicator</summary>
        <UsageType(UsageTypes.UsageSwitch)> UsageSelectedIndicator = &H3A
        ''' <summary>Usage In Use Indicator</summary>
        <UsageType(UsageTypes.UsageSwitch)> UsageInUseIndicator = &H3B
        ''' <summary>Usage Multi Mode Indicator</summary>
        <UsageType(UsageTypes.UsageModifier)> UsageMultiModeIndicator = &H3C
        ''' <summary>Indicator On</summary>
        <UsageType(UsageTypes.Selector)> IndicatorOn = &H3D
        ''' <summary>Indicator Flash</summary>
        <UsageType(UsageTypes.Selector)> IndicatorFlash = &H3E
        ''' <summary>Indicator Slow Blink</summary>
        <UsageType(UsageTypes.Selector)> IndicatorSlowBlink = &H3F
        ''' <summary>Indicator Fast Blink</summary>
        <UsageType(UsageTypes.Selector)> IndicatorFastBlink = &H40
        ''' <summary>Indicator Off</summary>
        <UsageType(UsageTypes.Selector)> IndicatorOff = &H41
        ''' <summary>Flash On Time</summary>
        <UsageType(UsageTypes.DynamicValue)> FlashOnTime = &H42
        ''' <summary>Slow Blink On Time</summary>
        <UsageType(UsageTypes.DynamicValue)> SlowBlinkOnTime = &H43
        ''' <summary>Slow Blink Off Time</summary>
        <UsageType(UsageTypes.DynamicValue)> SlowBlinkOffTime = &H44
        ''' <summary>Fast Blink On Time</summary>
        <UsageType(UsageTypes.DynamicValue)> FastBlinkOnTime = &H45
        ''' <summary>Fast Blink Off Time</summary>
        <UsageType(UsageTypes.DynamicValue)> FastBlinkOffTime = &H46
        ''' <summary>Usage Indicator Color</summary>
        <UsageType(UsageTypes.UsageModifier)> UsageIndicatorColor = &H47
        ''' <summary>Indicator Red</summary>
        <UsageType(UsageTypes.Selector)> IndicatorRed = &H48
        ''' <summary>Indicator Green</summary>
        <UsageType(UsageTypes.Selector)> IndicatorGreen = &H49
        ''' <summary>Indicator Amber</summary>
        <UsageType(UsageTypes.Selector)> IndicatorAmber = &H4A
        ''' <summary>Generic Indicator</summary>
        <UsageType(UsageTypes.OnOffControl)> GenericIndicator = &H4B
        ''' <summary>System Suspend</summary>
        <UsageType(UsageTypes.OnOffControl)> SystemSuspend = &H4C
        ''' <summary>External Power Connected</summary>
        <UsageType(UsageTypes.OnOffControl)> ExternalPowerConnected = &H4D
    End Enum

    ''' <summary>Contains HID usage code s for <see cref="UsagePages.Button "/> page</summary>
    ''' <remarks>For this enumeration all values between <see cref="Usages_Button.Button1"/> and <see cref="Usages_Button.Button65535"/> can be used.
    ''' <para>Buttons can be either <see cref="UsageTypes.Selector"/>, <see cref="UsageTypes.OnOffControl"/>, <see cref="UsageTypes.MomentaryControl"/> or <see cref="UsageTypes.OneShotControl"/> depending on the context of their declaration</para></remarks>
    <UndefinedMembersUsageType(UsageTypes.Selector Or UsageTypes.OnOffControl Or UsageTypes.MomentaryControl Or UsageTypes.OneShotControl)> _
    <ValuesFromRangeAreValid(Usages_Button.Button4 + 1, Usages_Button.Button65535 - 1)> _
    Public Enum Usages_Button
        ''' <summary>No button pressed</summary>
        <UsageType(UsageTypes.Selector Or UsageTypes.OnOffControl Or UsageTypes.MomentaryControl Or UsageTypes.OneShotControl)> _
        NoButtonPressed = 0
        ''' <summary>Button 1 (primary/trigger)</summary>
        <UsageType(UsageTypes.Selector Or UsageTypes.OnOffControl Or UsageTypes.MomentaryControl Or UsageTypes.OneShotControl)> _
        Button1
        ''' <summary>Button 2 (secondary)</summary>
        <UsageType(UsageTypes.Selector Or UsageTypes.OnOffControl Or UsageTypes.MomentaryControl Or UsageTypes.OneShotControl)> _
        Button2
        ''' <summary>Button 3 (tertiary)</summary>
        <UsageType(UsageTypes.Selector Or UsageTypes.OnOffControl Or UsageTypes.MomentaryControl Or UsageTypes.OneShotControl)> _
        Button3
        ''' <summary>Button 4</summary>
        <UsageType(UsageTypes.Selector Or UsageTypes.OnOffControl Or UsageTypes.MomentaryControl Or UsageTypes.OneShotControl)> _
        Button4
        ''' <summary>Button 65535</summary>
        <UsageType(UsageTypes.Selector Or UsageTypes.OnOffControl Or UsageTypes.MomentaryControl Or UsageTypes.OneShotControl)> _
        Button65535
    End Enum
    ''' <summary>Contains HID usage code s for <see cref="UsagePages.Ordinal"/> page</summary>
    ''' <remarks>For this enumeration all values between <see cref="Usages_Ordinal.Instance1"/> and <see cref="Usages_Ordinal.Instance65535"/> can be used.
    ''' <para>Memmbers are always of <see cref="UsageTypes.UsageModifier"/> type.</para></remarks>
    <ValuesFromRangeAreValid(Usages_Ordinal.Instance4 + 1, Usages_Ordinal.Instance65535 - 1)> _
    <UndefinedMembersUsageType(UsageTypes.UsageModifier)> _
    Public Enum Usages_Ordinal
        ''' <summary>Reserved</summary>
        Reserved = 0
        ''' <summary>Instance 1</summary>
        <UsageType(UsageTypes.UsageModifier)> Instance1 = 1
        ''' <summary>Instance 2</summary>
        <UsageType(UsageTypes.UsageModifier)> instance2 = 2
        ''' <summary>Instance 3</summary>
        <UsageType(UsageTypes.UsageModifier)> Instance3 = 3
        ''' <summary>Isnatnce 4</summary>
        <UsageType(UsageTypes.UsageModifier)> Instance4 = 4
        ''' <summary>Instance 65535</summary>
        <UsageType(UsageTypes.UsageModifier)> Instance65535 = 65535
    End Enum

    ''' <summary>Contains HID usage code s for <see cref="UsagePages.Telephony"/> page</summary>
    ''' <remarks>Values not defined in this enumeration are reserved</remarks>
    Public Enum Usages_Telephony
        ''' <summary>Unidentified</summary>
        Unassigned = 0
        ''' <summary>Phone</summary>
        <UsageType(UsageTypes.ApplicationCollection)> Phone = &H1
        ''' <summary>Answering Machine</summary>
        <UsageType(UsageTypes.ApplicationCollection)> AnsweringMachine = &H2
        ''' <summary>Message Controls</summary>
        <UsageType(UsageTypes.LogicalCollection)> MessageControls = &H3
        ''' <summary>Handset</summary>
        <UsageType(UsageTypes.LogicalCollection)> Handset = &H4
        ''' <summary>Headset</summary>
        <UsageType(UsageTypes.LogicalCollection)> Headset = &H5
        ''' <summary>Telephony Key Pad</summary>
        <UsageType(UsageTypes.NamedArray)> TelephonyKeyPad = &H6
        ''' <summary>Programmable Button</summary>
        <UsageType(UsageTypes.NamedArray)> ProgrammableButton = &H7
        ''' <summary>Hook Switch</summary>
        <UsageType(UsageTypes.OnOffControl)> HookSwitch = &H20
        ''' <summary>Flash</summary>
        <UsageType(UsageTypes.MomentaryControl)> Flash = &H21
        ''' <summary>Feature</summary>
        <UsageType(UsageTypes.OneShotControl)> Feature = &H22
        ''' <summary>Hold</summary>
        <UsageType(UsageTypes.OnOffControl)> Hold = &H23
        ''' <summary>Redial</summary>
        <UsageType(UsageTypes.OneShotControl)> Redial = &H24
        ''' <summary>Transfer</summary>
        <UsageType(UsageTypes.OneShotControl)> Transfer = &H25
        ''' <summary>Drop</summary>
        <UsageType(UsageTypes.OneShotControl)> Drop = &H26
        ''' <summary>Park</summary>
        <UsageType(UsageTypes.OnOffControl)> Park = &H27
        ''' <summary>Forward Calls</summary>
        <UsageType(UsageTypes.OnOffControl)> ForwardCalls = &H28
        ''' <summary>Alternate Function</summary>
        <UsageType(UsageTypes.MomentaryControl)> AlternateFunction = &H29
        ''' <summary>Line</summary>
        <UsageType(UsageTypes.OneShotControl Or UsageTypes.NamedArray)> Line = &H2A
        ''' <summary>Speaker Phone</summary>
        <UsageType(UsageTypes.OnOffControl)> SpeakerPhone = &H2B
        ''' <summary>Conference</summary>
        <UsageType(UsageTypes.OnOffControl)> Conference = &H2C
        ''' <summary>Ring Enable</summary>
        <UsageType(UsageTypes.OnOffControl)> RingEnable = &H2D
        ''' <summary>Ring Select</summary>
        <UsageType(UsageTypes.OneShotControl)> RingSelect = &H2E
        ''' <summary>Phone Mute</summary>
        <UsageType(UsageTypes.OnOffControl)> PhoneMute = &H2F
        ''' <summary>Caller ID</summary>
        <UsageType(UsageTypes.MomentaryControl)> CallerID = &H30
        ''' <summary>Send</summary>
        <UsageType(UsageTypes.OnOffControl)> Send = &H31
        ''' <summary>Speed Dial</summary>
        <UsageType(UsageTypes.OneShotControl)> SpeedDial = &H50
        ''' <summary>Store Number</summary>
        <UsageType(UsageTypes.OneShotControl)> StoreNumber = &H51
        ''' <summary>Recall Number</summary>
        <UsageType(UsageTypes.OneShotControl)> RecallNumber = &H52
        ''' <summary>Phone Directory</summary>
        <UsageType(UsageTypes.OnOffControl)> PhoneDirectory = &H53
        ''' <summary>Voice Mail</summary>
        <UsageType(UsageTypes.OnOffControl)> VoiceMail = &H70
        ''' <summary>Screen Calls</summary>
        <UsageType(UsageTypes.OnOffControl)> ScreenCalls = &H71
        ''' <summary>Do Not Disturb</summary>
        <UsageType(UsageTypes.OnOffControl)> DoNotDisturb = &H72
        ''' <summary>Message</summary>
        <UsageType(UsageTypes.OneShotControl)> Message = &H73
        ''' <summary>Answer On/Off</summary>
        <UsageType(UsageTypes.OnOffControl)> AnswerOnOff = &H74
        ''' <summary>Inside Dial Tone</summary>
        <UsageType(UsageTypes.MomentaryControl)> InsideDialTone = &H90
        ''' <summary>Outside Dial Tone</summary>
        <UsageType(UsageTypes.MomentaryControl)> OutsideDialTone = &H91
        ''' <summary>Inside Ring Tone</summary>
        <UsageType(UsageTypes.MomentaryControl)> InsideRingTone = &H92
        ''' <summary>Outside Ring Tone</summary>
        <UsageType(UsageTypes.MomentaryControl)> OutsideRingTone = &H93
        ''' <summary>Priority Ring Tone</summary>
        <UsageType(UsageTypes.MomentaryControl)> PriorityRingTone = &H94
        ''' <summary>Inside Ringback</summary>
        <UsageType(UsageTypes.MomentaryControl)> InsideRingback = &H95
        ''' <summary>Priority Ringback</summary>
        <UsageType(UsageTypes.MomentaryControl)> PriorityRingback = &H96
        ''' <summary>Line Busy Tone</summary>
        <UsageType(UsageTypes.MomentaryControl)> LineBusyTone = &H97
        ''' <summary>Reorder Tone</summary>
        <UsageType(UsageTypes.MomentaryControl)> ReorderTone = &H98
        ''' <summary>Call Waiting Tone</summary>
        <UsageType(UsageTypes.MomentaryControl)> CallWaitingTone = &H99
        ''' <summary>Confirmation Tone 1</summary>
        <UsageType(UsageTypes.MomentaryControl)> ConfirmationTone1 = &H9A
        ''' <summary>Confirmation Tone 1</summary>
        <UsageType(UsageTypes.MomentaryControl)> ConfirmationTone2 = &H9B
        ''' <summary>Tones Off</summary>
        <UsageType(UsageTypes.OnOffControl)> TonesOff = &H9C
        ''' <summary>Outside Ringback</summary>
        <UsageType(UsageTypes.MomentaryControl)> OutsideRingback = &H9D
        ''' <summary>Ringer</summary>
        <UsageType(UsageTypes.OnOffControl)> Ringer = &H9E
        ''' <summary>Phone Key 0</summary>
        <UsageType(UsageTypes.Selector)> Key0 = &HB0
        ''' <summary>Phone Key 1</summary>
        <UsageType(UsageTypes.Selector)> Key1 = &HB1
        ''' <summary>Phone Key 2</summary>
        <UsageType(UsageTypes.Selector)> Key2 = &HB2
        ''' <summary>Phone Key 3</summary>
        <UsageType(UsageTypes.Selector)> Key3 = &HB3
        ''' <summary>Phone Key 4</summary>
        <UsageType(UsageTypes.Selector)> Key4 = &HB4
        ''' <summary>Phone Key 5</summary>
        <UsageType(UsageTypes.Selector)> Key5 = &HB5
        ''' <summary>Phone Key 6</summary>
        <UsageType(UsageTypes.Selector)> Key6 = &HB6
        ''' <summary>Phone Key 7</summary>
        <UsageType(UsageTypes.Selector)> Key7 = &HB7
        ''' <summary>Phone Key 8</summary>
        <UsageType(UsageTypes.Selector)> Key8 = &HB8
        ''' <summary>Phone Key 9</summary>
        <UsageType(UsageTypes.Selector)> Key9 = &HB9
        ''' <summary>Phone Key Star</summary>
        <UsageType(UsageTypes.Selector)> KeyStar = &HBA
        ''' <summary>Phone Key Pound</summary>
        <UsageType(UsageTypes.Selector)> KeyPound = &HBB
        ''' <summary>Phone Key A</summary>
        <UsageType(UsageTypes.Selector)> KeyA = &HBC
        ''' <summary>Phone Key B</summary>
        <UsageType(UsageTypes.Selector)> KeyB = &HBD
        ''' <summary>Phone Key C</summary>
        <UsageType(UsageTypes.Selector)> KeyC = &HBE
        ''' <summary>Phone Key D</summary>
        <UsageType(UsageTypes.Selector)> KeyD = &HBF
    End Enum

    ''' <summary>Contains HID usage code s for <see cref="UsagePages.Consumer"/> page</summary>
    ''' <remarks>Values not defined in this enumeration are reserved</remarks>
    Public Enum Usages_Consumer
        ''' <summary>Unidentified</summary>
        Unassigned = 0
        ''' <summary>Consumer Control</summary>
        <UsageType(UsageTypes.ApplicationCollection)> ConsumerControl = &H1
        ''' <summary>Numeric Key Pad</summary>
        <UsageType(UsageTypes.NamedArray)> NumericKeyPad = &H2
        ''' <summary>Programmable Buttons</summary>
        <UsageType(UsageTypes.NamedArray)> ProgrammableButtons = &H3
        ''' <summary>Microphone</summary>
        <UsageType(UsageTypes.ApplicationCollection)> Microphone = &H4
        ''' <summary>Headphone</summary>
        <UsageType(UsageTypes.ApplicationCollection)> Headphone = &H5
        ''' <summary>Graphic Equalizer</summary>
        <UsageType(UsageTypes.ApplicationCollection)> GraphicEqualizer = &H6
        ''' <summary>+10</summary>
        <UsageType(UsageTypes.OneShotControl)> Plus10 = &H20
        ''' <summary>+100</summary>
        <UsageType(UsageTypes.OneShotControl)> Plus100 = &H21
        ''' <summary>AM/PM</summary>
        <UsageType(UsageTypes.OneShotControl)> AMPM = &H22
        ''' <summary>Power</summary>
        <UsageType(UsageTypes.OnOffControl)> Power = &H30
        ''' <summary>Reset</summary>
        <UsageType(UsageTypes.OneShotControl)> Reset = &H31
        ''' <summary>Sleep</summary>
        <UsageType(UsageTypes.OneShotControl)> Sleep = &H32
        ''' <summary>Sleep After</summary>
        <UsageType(UsageTypes.OneShotControl)> SleepAfter = &H33
        ''' <summary>Sleep Mode</summary>
        <UsageType(UsageTypes.ReTriggerControl)> SleepMode = &H34
        ''' <summary>Illumination</summary>
        <UsageType(UsageTypes.OnOffControl)> Illumination = &H35
        ''' <summary>Function Buttons</summary>
        <UsageType(UsageTypes.NamedArray)> FunctionButtons = &H36
        ''' <summary>Menu</summary>
        <UsageType(UsageTypes.OnOffControl)> Menu = &H40
        ''' <summary>Menu Pick</summary>
        <UsageType(UsageTypes.OneShotControl)> MenuPick = &H41
        ''' <summary>Menu Up</summary>
        <UsageType(UsageTypes.OneShotControl)> MenuUp = &H42
        ''' <summary>Menu Down</summary>
        <UsageType(UsageTypes.OneShotControl)> MenuDown = &H43
        ''' <summary>Menu Left</summary>
        <UsageType(UsageTypes.OneShotControl)> MenuLeft = &H44
        ''' <summary>Menu Right</summary>
        <UsageType(UsageTypes.OneShotControl)> MenuRight = &H45
        ''' <summary>Menu Escape</summary>
        <UsageType(UsageTypes.OneShotControl)> MenuEscape = &H46
        ''' <summary>Menu Value Increase</summary>
        <UsageType(UsageTypes.OneShotControl)> MenuValueIncrease = &H47
        ''' <summary>Menu Value Decrease</summary>
        <UsageType(UsageTypes.OneShotControl)> MenuValueDecrease = &H48
        ''' <summary>Data On Screen</summary>
        <UsageType(UsageTypes.OnOffControl)> DataOnScreen = &H60
        ''' <summary>Closed Caption</summary>
        <UsageType(UsageTypes.OnOffControl)> ClosedCaption = &H61
        ''' <summary>Closed Caption Select</summary>
        <UsageType(UsageTypes.OneShotControl)> ClosedCaptionSelect = &H62
        ''' <summary>VCR/TV</summary>
        <UsageType(UsageTypes.OnOffControl)> VCRTV = &H63
        ''' <summary>Broadcast Mode</summary>
        <UsageType(UsageTypes.OneShotControl)> BroadcastMode = &H64
        ''' <summary>Snapshot</summary>
        <UsageType(UsageTypes.OneShotControl)> Snapshot = &H65
        ''' <summary>Still</summary>
        <UsageType(UsageTypes.OneShotControl)> Still = &H66
        ''' <summary>Selection</summary>
        <UsageType(UsageTypes.NamedArray)> Selection = &H80
        ''' <summary>Assign Selection</summary>
        <UsageType(UsageTypes.OneShotControl)> AssignSelection = &H81
        ''' <summary>Mode Step</summary>
        <UsageType(UsageTypes.OneShotControl)> ModeStep = &H82
        ''' <summary>Recall Last</summary>
        <UsageType(UsageTypes.OneShotControl)> RecallLast = &H83
        ''' <summary>Enter Channel</summary>
        <UsageType(UsageTypes.OneShotControl)> EnterChannel = &H84
        ''' <summary>Order Movie</summary>
        <UsageType(UsageTypes.OneShotControl)> OrderMovie = &H85
        ''' <summary>Channel</summary>
        <UsageType(UsageTypes.LinearControl)> Channel = &H86
        ''' <summary>Media Selection</summary>
        <UsageType(UsageTypes.NamedArray)> MediaSelection = &H87
        ''' <summary>Media Select Computer</summary>
        <UsageType(UsageTypes.Selector)> MediaSelectComputer = &H88
        ''' <summary>Media Select TV</summary>
        <UsageType(UsageTypes.Selector)> MediaSelectTV = &H89
        ''' <summary>Media Select WWW</summary>
        <UsageType(UsageTypes.Selector)> MediaSelectWWW = &H8A
        ''' <summary>Media Select DVD</summary>
        <UsageType(UsageTypes.Selector)> MediaSelectDVD = &H8B
        ''' <summary>Media Select Telephone</summary>
        <UsageType(UsageTypes.Selector)> MediaSelectTelephone = &H8C
        ''' <summary>Media Select Program Guide</summary>
        <UsageType(UsageTypes.Selector)> MediaSelectProgramGuide = &H8D
        ''' <summary>Media Select Video Phone</summary>
        <UsageType(UsageTypes.Selector)> MediaSelectVideoPhone = &H8E
        ''' <summary>Media Select Games</summary>
        <UsageType(UsageTypes.Selector)> MediaSelectGames = &H8F
        ''' <summary>Media Select Messages</summary>
        <UsageType(UsageTypes.Selector)> MediaSelectMessages = &H90
        ''' <summary>Media Select CD</summary>
        <UsageType(UsageTypes.Selector)> MediaSelectCD = &H91
        ''' <summary>Media Select VCR</summary>
        <UsageType(UsageTypes.Selector)> MediaSelectVCR = &H92
        ''' <summary>Media Select Tuner</summary>
        <UsageType(UsageTypes.Selector)> MediaSelectTuner = &H93
        ''' <summary>Quit</summary>
        <UsageType(UsageTypes.OneShotControl)> Quit = &H94
        ''' <summary>Help</summary>
        <UsageType(UsageTypes.OnOffControl)> Help = &H95
        ''' <summary>Media Select Tape</summary>
        <UsageType(UsageTypes.Selector)> MediaSelectTape = &H96
        ''' <summary>Media Select Cable</summary>
        <UsageType(UsageTypes.Selector)> MediaSelectCable = &H97
        ''' <summary>Media Select Satellite</summary>
        <UsageType(UsageTypes.Selector)> MediaSelectSatellite = &H98
        ''' <summary>Media Select Security</summary>
        <UsageType(UsageTypes.Selector)> MediaSelectSecurity = &H99
        ''' <summary>Media Select Home</summary>
        <UsageType(UsageTypes.Selector)> MediaSelectHome = &H9A
        ''' <summary>Media Select Call</summary>
        <UsageType(UsageTypes.Selector)> MediaSelectCall = &H9B
        ''' <summary>Channel Increment</summary>
        <UsageType(UsageTypes.OneShotControl)> ChannelIncrement = &H9C
        ''' <summary>Channel Decrement</summary>
        <UsageType(UsageTypes.OneShotControl)> ChannelDecrement = &H9D
        ''' <summary>Media Select SAP</summary>
        <UsageType(UsageTypes.Selector)> MediaSelectSAP = &H9E
        ''' <summary>VCR Plus</summary>
        <UsageType(UsageTypes.OneShotControl)> VCRPlus = &HA0
        ''' <summary>Once</summary>
        <UsageType(UsageTypes.OneShotControl)> Once = &HA1
        ''' <summary>Daily</summary>
        <UsageType(UsageTypes.OneShotControl)> Daily = &HA2
        ''' <summary>Weekly</summary>
        <UsageType(UsageTypes.OneShotControl)> Weekly = &HA3
        ''' <summary>Monthly</summary>
        <UsageType(UsageTypes.OneShotControl)> Monthly = &HA4
        ''' <summary>Play</summary>
        <UsageType(UsageTypes.OnOffControl)> Play = &HB0
        ''' <summary>Pause</summary>
        <UsageType(UsageTypes.OnOffControl)> Pause = &HB1
        ''' <summary>Record</summary>
        <UsageType(UsageTypes.OnOffControl)> Record = &HB2
        ''' <summary>Fast Forward</summary>
        <UsageType(UsageTypes.OnOffControl)> FastForward = &HB3
        ''' <summary>Rewind</summary>
        <UsageType(UsageTypes.OnOffControl)> Rewind = &HB4
        ''' <summary>Scan Next Track</summary>
        <UsageType(UsageTypes.OneShotControl)> ScanNextTrack = &HB5
        ''' <summary>Scan Previous Track</summary>
        <UsageType(UsageTypes.OneShotControl)> ScanPreviousTrack = &HB6
        ''' <summary>Stop</summary>
        <UsageType(UsageTypes.OneShotControl)> [Stop] = &HB7
        ''' <summary>Eject</summary>
        <UsageType(UsageTypes.OneShotControl)> Eject = &HB8
        ''' <summary>Random Play</summary>
        <UsageType(UsageTypes.OnOffControl)> RandomPlay = &HB9
        ''' <summary>Select Disc</summary>
        <UsageType(UsageTypes.NamedArray)> SelectDisc = &HBA
        ''' <summary>Enter Disc</summary>
        <UsageType(UsageTypes.MomentaryControl)> EnterDisc = &HBB
        ''' <summary>Repeat</summary>
        <UsageType(UsageTypes.OneShotControl)> Repeat = &HBC
        ''' <summary>Tracking</summary>
        <UsageType(UsageTypes.LinearControl)> Tracking = &HBD
        ''' <summary>Track Normal</summary>
        <UsageType(UsageTypes.OneShotControl)> TrackNormal = &HBE
        ''' <summary>Slow Tracking</summary>
        <UsageType(UsageTypes.LinearControl)> Slowracking = &HBF
        ''' <summary>Frame Forward</summary>
        <UsageType(UsageTypes.ReTriggerControl)> FrameForward = &HC0
        ''' <summary>Frame Back</summary>
        <UsageType(UsageTypes.ReTriggerControl)> FrameBack = &HC1
        ''' <summary>Mark</summary>
        <UsageType(UsageTypes.OneShotControl)> Mark = &HC2
        ''' <summary>Clear Mark</summary>
        <UsageType(UsageTypes.OneShotControl)> ClearMark = &HC3
        ''' <summary>Repeat From Mark</summary>
        <UsageType(UsageTypes.OnOffControl)> RepeatFromMark = &HC4
        ''' <summary>Return To Mark</summary>
        <UsageType(UsageTypes.OneShotControl)> ReturnToMark = &HC5
        ''' <summary>Search Mark Forward</summary>
        <UsageType(UsageTypes.OneShotControl)> SearchMarkForward = &HC6
        ''' <summary>Search Mark Backwards</summary>
        <UsageType(UsageTypes.OneShotControl)> SearchMarkBackwards = &HC7
        ''' <summary>Counter Reset</summary>
        <UsageType(UsageTypes.OneShotControl)> CounterReset = &HC8
        ''' <summary>Show Counter</summary>
        <UsageType(UsageTypes.OneShotControl)> ShowCounter = &HC9
        ''' <summary>Tracking Increment</summary>
        <UsageType(UsageTypes.ReTriggerControl)> TrackingIncrement = &HCA
        ''' <summary>Tracking Decrement</summary>
        <UsageType(UsageTypes.ReTriggerControl)> TrackingDecrement = &HCB
        ''' <summary>Stop/Eject</summary>
        <UsageType(UsageTypes.OneShotControl)> StopEject = &HCC
        ''' <summary>Play/Pause</summary>
        <UsageType(UsageTypes.OneShotControl)> PlayPause = &HCD
        ''' <summary>Play/Skip</summary>
        <UsageType(UsageTypes.OneShotControl)> PlaySkip = &HCE
        ''' <summary>Volume</summary>
        <UsageType(UsageTypes.LinearControl)> Volume = &HE0
        ''' <summary>Balance</summary>
        <UsageType(UsageTypes.LinearControl)> Balance = &HE1
        ''' <summary>Mute</summary>
        <UsageType(UsageTypes.OnOffControl)> Mute = &HE2
        ''' <summary>Bass</summary>
        <UsageType(UsageTypes.LinearControl)> Bass = &HE3
        ''' <summary>Treble</summary>
        <UsageType(UsageTypes.LinearControl)> Treble = &HE4
        ''' <summary>Bass Boost</summary>
        <UsageType(UsageTypes.OnOffControl)> BassBoost = &HE5
        ''' <summary>Surround Mode</summary>
        <UsageType(UsageTypes.OneShotControl)> SurroundMode = &HE6
        ''' <summary>Loudness</summary>
        <UsageType(UsageTypes.OnOffControl)> Loudness = &HE7
        ''' <summary>MPX</summary>
        <UsageType(UsageTypes.OnOffControl)> MPX = &HE8
        ''' <summary>Volume Increment</summary>
        <UsageType(UsageTypes.ReTriggerControl)> VolumeIncrement = &HE9
        ''' <summary>Volume Decrement</summary>
        <UsageType(UsageTypes.ReTriggerControl)> VolumeDecrement = &HEA
        ''' <summary>Speed Select</summary>
        <UsageType(UsageTypes.OneShotControl)> SpeedSelect = &HF0
        ''' <summary>Playback Speed</summary>
        <UsageType(UsageTypes.NamedArray)> PlaybackSpeed = &HF1
        ''' <summary>Standard Play</summary>
        <UsageType(UsageTypes.Selector)> StandardPlay = &HF2
        ''' <summary>Long Play</summary>
        <UsageType(UsageTypes.Selector)> LongPlay = &HF3
        ''' <summary>Extended Play</summary>
        <UsageType(UsageTypes.Selector)> ExtendedPlay = &HF4
        ''' <summary>Slow</summary>
        <UsageType(UsageTypes.OneShotControl)> Slow = &HF5
        ''' <summary>Fan Enable</summary>
        <UsageType(UsageTypes.OnOffControl)> FanEnable = &H100
        ''' <summary>Fan Speed</summary>
        <UsageType(UsageTypes.LinearControl)> FanSpeed = &H101
        ''' <summary>Light Enable</summary>
        <UsageType(UsageTypes.OnOffControl)> LightEnable = &H102
        ''' <summary>Light Illumination Level</summary>
        <UsageType(UsageTypes.LinearControl)> LightIlluminationLevel = &H103
        ''' <summary>Climate Control Enable</summary>
        <UsageType(UsageTypes.OnOffControl)> ClimateControlEnable = &H104
        ''' <summary>Room Temperature</summary>
        <UsageType(UsageTypes.LinearControl)> RoomTemperature = &H105
        ''' <summary>Security Enable</summary>
        <UsageType(UsageTypes.OnOffControl)> SecurityEnable = &H106
        ''' <summary>Fire Alarm</summary>
        <UsageType(UsageTypes.OneShotControl)> FireAlarm = &H107
        ''' <summary>Police Alarm</summary>
        <UsageType(UsageTypes.OneShotControl)> PoliceAlarm = &H108
        ''' <summary>Proximity</summary>
        <UsageType(UsageTypes.LinearControl)> Proximity = &H109
        ''' <summary>Motion</summary>
        <UsageType(UsageTypes.OneShotControl)> Motion = &H10A
        ''' <summary>Duress Alarm</summary>
        <UsageType(UsageTypes.OneShotControl)> DuressAlarm = &H10B
        ''' <summary>Holdup Alarm</summary>
        <UsageType(UsageTypes.OneShotControl)> HoldupAlarm = &H10C
        ''' <summary>Medical Alarm</summary>
        <UsageType(UsageTypes.OneShotControl)> MedicalAlarm = &H10D
        ''' <summary>Balance Right</summary>
        <UsageType(UsageTypes.ReTriggerControl)> BalanceRight = &H150
        ''' <summary>Balance Left</summary>
        <UsageType(UsageTypes.ReTriggerControl)> BalanceLeft = &H151
        ''' <summary>Bass Increment</summary>
        <UsageType(UsageTypes.ReTriggerControl)> BassIncrement = &H152
        ''' <summary>Bass Decrement</summary>
        <UsageType(UsageTypes.ReTriggerControl)> BassDecrement = &H153
        ''' <summary>Treble Increment</summary>
        <UsageType(UsageTypes.ReTriggerControl)> TrebleIncrement = &H154
        ''' <summary>Treble Decrement</summary>
        <UsageType(UsageTypes.ReTriggerControl)> TrebleDecrement = &H155
        ''' <summary>Speaker System</summary>
        <UsageType(UsageTypes.LogicalCollection)> SpeakerSystem = &H160
        ''' <summary>Channel Left</summary>
        <UsageType(UsageTypes.LogicalCollection)> ChannelLeft = &H161
        ''' <summary>Channel Right</summary>
        <UsageType(UsageTypes.LogicalCollection)> ChannelRight = &H162
        ''' <summary>Channel Center</summary>
        <UsageType(UsageTypes.LogicalCollection)> ChannelCenter = &H163
        ''' <summary>Channel Front</summary>
        <UsageType(UsageTypes.LogicalCollection)> ChannelFront = &H164
        ''' <summary>Channel Center Front</summary>
        <UsageType(UsageTypes.LogicalCollection)> ChannelCenterFront = &H165
        ''' <summary>Channel Side</summary>
        <UsageType(UsageTypes.LogicalCollection)> ChannelSide = &H166
        ''' <summary>Channel Surround</summary>
        <UsageType(UsageTypes.LogicalCollection)> ChannelSurround = &H167
        ''' <summary>Channel Low Frequency Enhancement</summary>
        <UsageType(UsageTypes.LogicalCollection)> ChannelLowFrequencyEnhancement = &H168
        ''' <summary>Channel Top</summary>
        <UsageType(UsageTypes.LogicalCollection)> ChannelTop = &H169
        ''' <summary>Channel Unknown</summary>
        <UsageType(UsageTypes.LogicalCollection)> ChannelUnknown = &H16A
        ''' <summary>Sub-channel</summary>
        <UsageType(UsageTypes.LinearControl)> SubChannel = &H170
        ''' <summary>Sub-channel Increment</summary>
        <UsageType(UsageTypes.OneShotControl)> SubChannelIncrement = &H171
        ''' <summary>Sub-channel Decrement</summary>
        <UsageType(UsageTypes.OneShotControl)> SubChannelDecrement = &H172
        ''' <summary>Alternate Audio Increment</summary>
        <UsageType(UsageTypes.OneShotControl)> AlternateAudioIncrement = &H173
        ''' <summary>Alternate Audio Decrement</summary>
        <UsageType(UsageTypes.OneShotControl)> AlternateAudioDecrement = &H174
        ''' <summary>Application Launch Buttons</summary>
        <UsageType(UsageTypes.NamedArray)> ApplicationLaunchButtons = &H180
        ''' <summary>Application launch (AL) Launch Button Configuration Tool</summary>
        <UsageType(UsageTypes.Selector)> ALLaunchButtonConfigurationTools = &H181
        ''' <summary>Application launch (AL) Programmable Button Configuration</summary>
        <UsageType(UsageTypes.Selector)> ALProgrammableButtonConfiguration = &H182
        ''' <summary>Application launch (AL) Consumer Control Configuration</summary>
        <UsageType(UsageTypes.Selector)> ALConsumerControlConfiguration = &H183
        ''' <summary>Application launch (AL) Word Processor</summary>
        <UsageType(UsageTypes.Selector)> ALWordProcessor = &H184
        ''' <summary>Application launch (AL) Text Editor</summary>
        <UsageType(UsageTypes.Selector)> ALTextEditor = &H185
        ''' <summary>Application launch (AL) Spreadsheet</summary>
        <UsageType(UsageTypes.Selector)> ALSpreadsheet = &H186
        ''' <summary>Application launch (AL) Graphics Editor</summary>
        <UsageType(UsageTypes.Selector)> ALGraphicsEditor = &H187
        ''' <summary>Application launch (AL) Presentation App</summary>
        <UsageType(UsageTypes.Selector)> ALPresentationApp = &H188
        ''' <summary>Application launch (AL) Database App</summary>
        <UsageType(UsageTypes.Selector)> ALDatabaseApp = &H189
        ''' <summary>Application launch (AL) Email Reader</summary>
        <UsageType(UsageTypes.Selector)> ALEmailReader = &H18A
        ''' <summary>Application launch (AL) Newsreader</summary>
        <UsageType(UsageTypes.Selector)> ALNewsreader = &H18B
        ''' <summary>Application launch (AL) Voicemail</summary>
        <UsageType(UsageTypes.Selector)> ALVoicemail = &H18C
        ''' <summary>Application launch (AL) Contacts/Address Book</summary>
        <UsageType(UsageTypes.Selector)> ALContactsAddressBook = &H18D
        ''' <summary>Application launch (AL) Calendar/Schedule</summary>
        <UsageType(UsageTypes.Selector)> ALCalendarSchedule = &H18E
        ''' <summary>Application launch (AL) Task/Project Manager</summary>
        <UsageType(UsageTypes.Selector)> ALTaskProjectManager = &H18F
        ''' <summary>Application launch (AL) Log/Journal/Timecard</summary>
        <UsageType(UsageTypes.Selector)> ALLogJournalTimecard = &H190
        ''' <summary>Application launch (AL) Checkbook/Finance</summary>
        <UsageType(UsageTypes.Selector)> ALCheckbookFinance = &H191
        ''' <summary>Application launch (AL) Calculator</summary>
        <UsageType(UsageTypes.Selector)> ALCalculator = &H192
        ''' <summary>Application launch (AL) A/V Capture/Playback</summary>
        <UsageType(UsageTypes.Selector)> ALAVCapturePlayback = &H193
        ''' <summary>Application launch (AL) Local Machine Browser</summary>
        <UsageType(UsageTypes.Selector)> ALLocalMachineBrowser = &H194
        ''' <summary>Application launch (AL) LAN/WAN Browser</summary>
        <UsageType(UsageTypes.Selector)> ALLANWANBrowser = &H195
        ''' <summary>Application launch (AL) Internet Browser</summary>
        <UsageType(UsageTypes.Selector)> ALInternetBrowser = &H196
        ''' <summary>Application launch (AL) Remote Networking/ISP Connect</summary>
        <UsageType(UsageTypes.Selector)> ALRemoteNetworkingISPConnect = &H197
        ''' <summary>Application launch (AL) Network Conference</summary>
        <UsageType(UsageTypes.Selector)> ALNetworkConference = &H198
        ''' <summary>Application launch (AL) Network Chat</summary>
        <UsageType(UsageTypes.Selector)> ALNetworkChat = &H199
        ''' <summary>Application launch (AL) Telephony/Dialer</summary>
        <UsageType(UsageTypes.Selector)> ALTelephonyDialer = &H19A
        ''' <summary>Application launch (AL) Logon</summary>
        <UsageType(UsageTypes.Selector)> ALLogon = &H19B
        ''' <summary>Application launch (AL) Logoff</summary>
        <UsageType(UsageTypes.Selector)> ALLogoff = &H19C
        ''' <summary>Application launch (AL) Logon/Logoff</summary>
        <UsageType(UsageTypes.Selector)> ALLogonLogoff = &H19D
        ''' <summary>Application launch (AL) Terminal Lock/Screensaver</summary>
        <UsageType(UsageTypes.Selector)> ALTerminalLockScreensaver = &H19E
        ''' <summary>Application launch (AL) Control Panel</summary>
        <UsageType(UsageTypes.Selector)> ALControlPanel = &H19F
        ''' <summary>Application launch (AL) Command Line Processor/Run</summary>
        <UsageType(UsageTypes.Selector)> ALCommandLineProcessorRun = &H1A0
        ''' <summary>Application launch (AL) Process/Task Manager</summary>
        <UsageType(UsageTypes.Selector)> ALProcessTaskManager = &H1A1
        ''' <summary>Application launch (AL) Select Task/Application</summary>
        <UsageType(UsageTypes.Selector)> ALSelectTaskApplication = &H1A2
        ''' <summary>Application launch (AL) Next Task/Application</summary>
        <UsageType(UsageTypes.Selector)> ALNextTaskApplication = &H1A3
        ''' <summary>Application launch (AL) Previous Task/Application</summary>
        <UsageType(UsageTypes.Selector)> ALPreviousTaskApplication = &H1A4
        ''' <summary>Application launch (AL) Preemptive Halt Task/Application</summary>
        <UsageType(UsageTypes.Selector)> ALPreemptiveHaltTaskApplication = &H1A5
        ''' <summary>Application launch (AL) Integrated Help Center</summary>
        <UsageType(UsageTypes.Selector)> ALIntegratedHelpCenter = &H1A6
        ''' <summary>Application launch (AL) Documents</summary>
        <UsageType(UsageTypes.Selector)> ALDocuments = &H1A7
        ''' <summary>Application launch (AL) Thesaurus</summary>
        <UsageType(UsageTypes.Selector)> ALThesaurus = &H1A8
        ''' <summary>Application launch (AL) Dictionary</summary>
        <UsageType(UsageTypes.Selector)> ALDictionary = &H1A9
        ''' <summary>Application launch (AL) Desktop</summary>
        <UsageType(UsageTypes.Selector)> ALDesktop = &H1AA
        ''' <summary>Application launch (AL) Spell Check</summary>
        <UsageType(UsageTypes.Selector)> ALSpellCheck = &H1AB
        ''' <summary>Application launch (AL) Grammar Check</summary>
        <UsageType(UsageTypes.Selector)> ALGrammarCheck = &H1AC
        ''' <summary>Application launch (AL) Wireless Status</summary>
        <UsageType(UsageTypes.Selector)> ALWirelessStatus = &H1AD
        ''' <summary>Application launch (AL) Keyboard Layout</summary>
        <UsageType(UsageTypes.Selector)> ALKeyboardLayout = &H1AE
        ''' <summary>Application launch (AL) Virus Protection</summary>
        <UsageType(UsageTypes.Selector)> ALVirusProtection = &H1AF
        ''' <summary>Application launch (AL) Encryption</summary>
        <UsageType(UsageTypes.Selector)> ALEncryption = &H1B0
        ''' <summary>Application launch (AL) Screen Saver</summary>
        <UsageType(UsageTypes.Selector)> ALScreenSaver = &H1B1
        ''' <summary>Application launch (AL) Alarms</summary>
        <UsageType(UsageTypes.Selector)> ALAlarms = &H1B2
        ''' <summary>Application launch (AL) Clock</summary>
        <UsageType(UsageTypes.Selector)> ALClock = &H1B3
        ''' <summary>Application launch (AL) File Browser</summary>
        <UsageType(UsageTypes.Selector)> ALFileBrowser = &H1B4
        ''' <summary>Application launch (AL) Power Status</summary>
        <UsageType(UsageTypes.Selector)> ALPowerStatus = &H1B5
        ''' <summary>Application launch (AL) Image Browser</summary>
        <UsageType(UsageTypes.Selector)> ALImageBrowser = &H1B6
        ''' <summary>Application launch (AL) Audio Browser</summary>
        <UsageType(UsageTypes.Selector)> ALAudioBrowser = &H1B7
        ''' <summary>Application launch (AL) Movie Browser</summary>
        <UsageType(UsageTypes.Selector)> ALMovieBrowser = &H1B8
        ''' <summary>Application launch (AL) Digital Rights Manager</summary>
        <UsageType(UsageTypes.Selector)> ALDigitalRightsManager = &H1B9
        ''' <summary>Application launch (AL) Digital Wallet</summary>
        <UsageType(UsageTypes.Selector)> ALDigitalWallet = &H1BA
        ''' <summary>Application launch (AL) Instant Messaging</summary>
        <UsageType(UsageTypes.Selector)> ALInstantMessaging = &H1BC
        ''' <summary>Application launch (AL) OEM Features/Tips/Tutorial Browser</summary>
        <UsageType(UsageTypes.Selector)> ALOEMFeaturesTipsTutorialBrowser = &H1BD
        ''' <summary>Application launch (AL) OEM Help</summary>
        <UsageType(UsageTypes.Selector)> ALOEMHelp = &H1BE
        ''' <summary>Application launch (AL) Online Community</summary>
        <UsageType(UsageTypes.Selector)> ALOnlineCommunity = &H1BF
        ''' <summary>Application launch (AL) Entertainment Conetent Browser</summary>
        <UsageType(UsageTypes.Selector)> ALEntertainmentContentBrowser = &H1C0
        ''' <summary>Application launch (AL) Online Shopping Browser</summary>
        <UsageType(UsageTypes.Selector)> ALOnlineShoppingBrowser = &H1C1
        ''' <summary>Application launch (AL) SmartCard Information/Help</summary>
        <UsageType(UsageTypes.Selector)> ALSmartCardInformationHelp = &H1C2
        ''' <summary>Application launch (AL) Market Monitor/Finance Browser</summary>
        <UsageType(UsageTypes.Selector)> ALMarketMonitorFinanceBrowser = &H1C3
        ''' <summary>Application launch (AL) Customized Corporate News Browser</summary>
        <UsageType(UsageTypes.Selector)> ALCustomizedCorporateNewsBrowser = &H1C4
        ''' <summary>Application launch (AL) Online Activity Browser</summary>
        <UsageType(UsageTypes.Selector)> ALOnlineActivityBrowser = &H1C5
        ''' <summary>Application launch (AL) Research/Search Browser</summary>
        <UsageType(UsageTypes.Selector)> ALResearchSearchBrowser = &H1C6
        ''' <summary>Application launch (AL) Audio Player</summary>
        <UsageType(UsageTypes.Selector)> ALAudioPlayer = &H1C7
        ''' <summary>Generic GUI Application Controls</summary>
        <UsageType(UsageTypes.NamedArray)> GenericGUIApplicationControls = &H200
        ''' <summary> Application Control (AC) New</summary>
        <UsageType(UsageTypes.Selector)> ACNew = &H201
        ''' <summary> Application Control (AC) Open</summary>
        <UsageType(UsageTypes.Selector)> ACOpen = &H202
        ''' <summary> Application Control (AC) Close</summary>
        <UsageType(UsageTypes.Selector)> ACClose = &H203
        ''' <summary> Application Control (AC) Exit</summary>
        <UsageType(UsageTypes.Selector)> ACExit = &H204
        ''' <summary> Application Control (AC) Maximize</summary>
        <UsageType(UsageTypes.Selector)> ACMaximize = &H205
        ''' <summary> Application Control (AC) Minimize</summary>
        <UsageType(UsageTypes.Selector)> ACMinimize = &H206
        ''' <summary> Application Control (AC) Save</summary>
        <UsageType(UsageTypes.Selector)> ACSave = &H207
        ''' <summary> Application Control (AC) Print</summary>
        <UsageType(UsageTypes.Selector)> ACPrint = &H208
        ''' <summary> Application Control (AC) Properties</summary>
        <UsageType(UsageTypes.Selector)> ACProperties = &H209
        ''' <summary> Application Control (AC) Undo</summary>
        <UsageType(UsageTypes.Selector)> ACUndo = &H21A
        ''' <summary> Application Control (AC) Copy</summary>
        <UsageType(UsageTypes.Selector)> ACCopy = &H21B
        ''' <summary> Application Control (AC) Cut</summary>
        <UsageType(UsageTypes.Selector)> ACCut = &H21C
        ''' <summary> Application Control (AC) Paste</summary>
        <UsageType(UsageTypes.Selector)> ACPaste = &H21D
        ''' <summary> Application Control (AC) Select All</summary>
        <UsageType(UsageTypes.Selector)> ACSelectAll = &H21E
        ''' <summary> Application Control (AC) Find</summary>
        <UsageType(UsageTypes.Selector)> ACFind = &H21F
        ''' <summary> Application Control (AC) Find and Replace</summary>
        <UsageType(UsageTypes.Selector)> ACFindAndReplace = &H220
        ''' <summary> Application Control (AC) Search</summary>
        <UsageType(UsageTypes.Selector)> ACSearch = &H221
        ''' <summary> Application Control (AC) Go To</summary>
        <UsageType(UsageTypes.Selector)> ACGoTo = &H222
        ''' <summary> Application Control (AC) Home</summary>
        <UsageType(UsageTypes.Selector)> ACHome = &H223
        ''' <summary> Application Control (AC) Back</summary>
        <UsageType(UsageTypes.Selector)> ACBack = &H224
        ''' <summary> Application Control (AC) Forward</summary>
        <UsageType(UsageTypes.Selector)> ACForward = &H225
        ''' <summary> Application Control (AC) Stop</summary>
        <UsageType(UsageTypes.Selector)> ACStop = &H226
        ''' <summary> Application Control (AC) Refresh</summary>
        <UsageType(UsageTypes.Selector)> ACRefresh = &H227
        ''' <summary> Application Control (AC) Previous Link</summary>
        <UsageType(UsageTypes.Selector)> ACPreviousLink = &H228
        ''' <summary> Application Control (AC) Next Link</summary>
        <UsageType(UsageTypes.Selector)> ACNextLink = &H229
        ''' <summary> Application Control (AC) Bookmarks</summary>
        <UsageType(UsageTypes.Selector)> ACBookmarks = &H22A
        ''' <summary> Application Control (AC) History</summary>
        <UsageType(UsageTypes.Selector)> ACHistory = &H22B
        ''' <summary> Application Control (AC) Subscriptions</summary>
        <UsageType(UsageTypes.Selector)> ACSubscriptions = &H22C
        ''' <summary> Application Control (AC) Zoom In</summary>
        <UsageType(UsageTypes.Selector)> ACZoomIn = &H22D
        ''' <summary> Application Control (AC) Zoom Out</summary>
        <UsageType(UsageTypes.Selector)> ACZoomOut = &H22E
        ''' <summary> Application Control (AC) Zoom</summary>
        <UsageType(UsageTypes.LinearControl)> ACZoom = &H22F
        ''' <summary> Application Control (AC) Full Screen View</summary>
        <UsageType(UsageTypes.Selector)> ACFullScreenView = &H230
        ''' <summary> Application Control (AC) Normal View</summary>
        <UsageType(UsageTypes.Selector)> ACNormalView = &H231
        ''' <summary> Application Control (AC) View Toggle</summary>
        <UsageType(UsageTypes.Selector)> ACViewToggle = &H232
        ''' <summary> Application Control (AC) Scroll Up</summary>
        <UsageType(UsageTypes.Selector)> ACScrollUp = &H233
        ''' <summary> Application Control (AC) Scroll Down</summary>
        <UsageType(UsageTypes.Selector)> ACScrollDown = &H234
        ''' <summary> Application Control (AC) Scroll</summary>
        <UsageType(UsageTypes.LinearControl)> ACScroll = &H235
        ''' <summary> Application Control (AC) Pan Left</summary>
        <UsageType(UsageTypes.Selector)> ACPanLeft = &H236
        ''' <summary> Application Control (AC) Pan Right</summary>
        <UsageType(UsageTypes.Selector)> ACPanRight = &H237
        ''' <summary> Application Control (AC) Pan</summary>
        <UsageType(UsageTypes.LinearControl)> ACPan = &H238
        ''' <summary> Application Control (AC) New Window</summary>
        <UsageType(UsageTypes.Selector)> ACNewWindow = &H239
        ''' <summary> Application Control (AC) Tile Horizontally</summary>
        <UsageType(UsageTypes.Selector)> ACTileHorizontally = &H23A
        ''' <summary> Application Control (AC) Tile Vertically</summary>
        <UsageType(UsageTypes.Selector)> ACTileVertically = &H23B
        ''' <summary> Application Control (AC) Format</summary>
        <UsageType(UsageTypes.Selector)> ACFormat = &H23C
        ''' <summary> Application Control (AC) Edit</summary>
        <UsageType(UsageTypes.Selector)> ACEdit = &H23D
        ''' <summary> Application Control (AC) Bold</summary>
        <UsageType(UsageTypes.Selector)> ACBold = &H23E
        ''' <summary> Application Control (AC) Italics</summary>
        <UsageType(UsageTypes.Selector)> ACItalics = &H23F
        ''' <summary> Application Control (AC) Underline</summary>
        <UsageType(UsageTypes.Selector)> ACUnderline = &H240
        ''' <summary> Application Control (AC) Strikethrough</summary>
        <UsageType(UsageTypes.Selector)> ACStrikethrough = &H241
        ''' <summary> Application Control (AC) Subscript</summary>
        <UsageType(UsageTypes.Selector)> ACSubscript = &H242
        ''' <summary> Application Control (AC) Superscript</summary>
        <UsageType(UsageTypes.Selector)> ACSuperscript = &H243
        ''' <summary> Application Control (AC) All Caps</summary>
        <UsageType(UsageTypes.Selector)> ACAllCaps = &H244
        ''' <summary> Application Control (AC) Rotate</summary>
        <UsageType(UsageTypes.Selector)> ACRotate = &H245
        ''' <summary> Application Control (AC) Resize</summary>
        <UsageType(UsageTypes.Selector)> ACResize = &H246
        ''' <summary> Application Control (AC) Flip horizontal</summary>
        <UsageType(UsageTypes.Selector)> ACFlipHorizontal = &H247
        ''' <summary> Application Control (AC) Flip Vertical</summary>
        <UsageType(UsageTypes.Selector)> ACFlipVertical = &H248
        ''' <summary> Application Control (AC) Mirror Horizontal</summary>
        <UsageType(UsageTypes.Selector)> ACMirrorHorizontal = &H249
        ''' <summary> Application Control (AC) Mirror Vertical</summary>
        <UsageType(UsageTypes.Selector)> ACMirrorVertical = &H24A
        ''' <summary> Application Control (AC) Font Select</summary>
        <UsageType(UsageTypes.Selector)> ACFontSelect = &H24B
        ''' <summary> Application Control (AC) Font Color</summary>
        <UsageType(UsageTypes.Selector)> ACFontColor = &H24C
        ''' <summary> Application Control (AC) Font Size</summary>
        <UsageType(UsageTypes.Selector)> ACFontSize = &H24D
        ''' <summary> Application Control (AC) Justify Left</summary>
        <UsageType(UsageTypes.Selector)> ACJustifyLeft = &H24E
        ''' <summary> Application Control (AC) Justify Center H</summary>
        <UsageType(UsageTypes.Selector)> ACJustifyCenterH = &H24F
        ''' <summary> Application Control (AC) Justify Right</summary>
        <UsageType(UsageTypes.Selector)> ACJustifyRight = &H250
        ''' <summary> Application Control (AC) Justify Block H</summary>
        <UsageType(UsageTypes.Selector)> ACJustifyBlockH = &H251
        ''' <summary> Application Control (AC) Justify Top</summary>
        <UsageType(UsageTypes.Selector)> ACJustifyTop = &H252
        ''' <summary> Application Control (AC) Justify Center V</summary>
        <UsageType(UsageTypes.Selector)> ACJustifyCenterV = &H253
        ''' <summary> Application Control (AC) Justify Bottom</summary>
        <UsageType(UsageTypes.Selector)> ACJustifyBottom = &H254
        ''' <summary> Application Control (AC) Justify Block V</summary>
        <UsageType(UsageTypes.Selector)> ACJustifyBlockV = &H255
        ''' <summary> Application Control (AC) Indent Decrease</summary>
        <UsageType(UsageTypes.Selector)> ACIndentDecrease = &H256
        ''' <summary> Application Control (AC) Indent Increase</summary>
        <UsageType(UsageTypes.Selector)> ACIndentIncrease = &H257
        ''' <summary> Application Control (AC) Numbered List</summary>
        <UsageType(UsageTypes.Selector)> ACNumberedList = &H258
        ''' <summary> Application Control (AC) Restart Numbering</summary>
        <UsageType(UsageTypes.Selector)> ACRestartNumbering = &H259
        ''' <summary> Application Control (AC) Bulleted List</summary>
        <UsageType(UsageTypes.Selector)> ACBulletedList = &H25A
        ''' <summary> Application Control (AC) Promote</summary>
        <UsageType(UsageTypes.Selector)> ACPromote = &H25B
        ''' <summary> Application Control (AC) Demote</summary>
        <UsageType(UsageTypes.Selector)> ACDemote = &H25C
        ''' <summary> Application Control (AC) Yes</summary>
        <UsageType(UsageTypes.Selector)> ACYes = &H25D
        ''' <summary> Application Control (AC) No</summary>
        <UsageType(UsageTypes.Selector)> ACNo = &H25E
        ''' <summary> Application Control (AC) Cancel</summary>
        <UsageType(UsageTypes.Selector)> ACCancel = &H25F
        ''' <summary> Application Control (AC) Catalog</summary>
        <UsageType(UsageTypes.Selector)> ACCatalog = &H260
        ''' <summary> Application Control (AC) Buy/Checkout</summary>
        <UsageType(UsageTypes.Selector)> ACBuyCheckout = &H261
        ''' <summary> Application Control (AC) Add to Cart</summary>
        <UsageType(UsageTypes.Selector)> ACAddtoCart = &H262
        ''' <summary> Application Control (AC) Expand</summary>
        <UsageType(UsageTypes.Selector)> ACExpand = &H263
        ''' <summary> Application Control (AC) Expand All</summary>
        <UsageType(UsageTypes.Selector)> ACExpandAll = &H264
        ''' <summary> Application Control (AC) Collapse</summary>
        <UsageType(UsageTypes.Selector)> ACCollapse = &H265
        ''' <summary> Application Control (AC) Collapse All</summary>
        <UsageType(UsageTypes.Selector)> ACCollapseAll = &H266
        ''' <summary> Application Control (AC) Print Preview</summary>
        <UsageType(UsageTypes.Selector)> ACPrintPreview = &H267
        ''' <summary> Application Control (AC) Paste Special</summary>
        <UsageType(UsageTypes.Selector)> ACPasteSpecial = &H268
        ''' <summary> Application Control (AC) Insert Mode</summary>
        <UsageType(UsageTypes.Selector)> ACInsertMode = &H269
        ''' <summary> Application Control (AC) Delete</summary>
        <UsageType(UsageTypes.Selector)> ACDelete = &H26A
        ''' <summary> Application Control (AC) Lock</summary>
        <UsageType(UsageTypes.Selector)> ACLock = &H26B
        ''' <summary> Application Control (AC) Unlock</summary>
        <UsageType(UsageTypes.Selector)> ACUnlock = &H26C
        ''' <summary> Application Control (AC) Protect</summary>
        <UsageType(UsageTypes.Selector)> ACProtect = &H26D
        ''' <summary> Application Control (AC) Unprotect</summary>
        <UsageType(UsageTypes.Selector)> ACUnprotect = &H26E
        ''' <summary> Application Control (AC) Attach Comment</summary>
        <UsageType(UsageTypes.Selector)> ACAttachComment = &H26F
        ''' <summary> Application Control (AC) Delete Comment</summary>
        <UsageType(UsageTypes.Selector)> ACDeleteComment = &H270
        ''' <summary> Application Control (AC) View Comment</summary>
        <UsageType(UsageTypes.Selector)> ACViewComment = &H271
        ''' <summary> Application Control (AC) Select Word</summary>
        <UsageType(UsageTypes.Selector)> ACSelectWord = &H272
        ''' <summary> Application Control (AC) Select Sentence</summary>
        <UsageType(UsageTypes.Selector)> ACSelectSentence = &H273
        ''' <summary> Application Control (AC) Select Paragraph</summary>
        <UsageType(UsageTypes.Selector)> ACSelectParagraph = &H274
        ''' <summary> Application Control (AC) Select Column</summary>
        <UsageType(UsageTypes.Selector)> ACSelectColumn = &H275
        ''' <summary> Application Control (AC) Select Row</summary>
        <UsageType(UsageTypes.Selector)> ACSelectRow = &H276
        ''' <summary> Application Control (AC) Select Table</summary>
        <UsageType(UsageTypes.Selector)> ACSelectTable = &H277
        ''' <summary> Application Control (AC) Select Object</summary>
        <UsageType(UsageTypes.Selector)> ACSelectObject = &H278
        ''' <summary> Application Control (AC) Redo/Repeat</summary>
        <UsageType(UsageTypes.Selector)> ACRedoRepeat = &H279
        ''' <summary> Application Control (AC) Sort</summary>
        <UsageType(UsageTypes.Selector)> ACSort = &H27A
        ''' <summary> Application Control (AC) Sort Ascending</summary>
        <UsageType(UsageTypes.Selector)> ACSortAscending = &H27B
        ''' <summary> Application Control (AC) Sort Descending</summary>
        <UsageType(UsageTypes.Selector)> ACSortDescending = &H27C
        ''' <summary> Application Control (AC) Filter</summary>
        <UsageType(UsageTypes.Selector)> ACFilter = &H27D
        ''' <summary> Application Control (AC) Set Clock</summary>
        <UsageType(UsageTypes.Selector)> ACSetClock = &H27E
        ''' <summary> Application Control (AC) View Clock</summary>
        <UsageType(UsageTypes.Selector)> ACViewClock = &H27F
        ''' <summary> Application Control (AC) Select Time Zone</summary>
        <UsageType(UsageTypes.Selector)> ACSelectTimeZone = &H280
        ''' <summary> Application Control (AC) Edit Time Zones</summary>
        <UsageType(UsageTypes.Selector)> ACEditTimeZones = &H281
        ''' <summary> Application Control (AC) Set Alarm</summary>
        <UsageType(UsageTypes.Selector)> ACSetAlarm = &H282
        ''' <summary> Application Control (AC) Clear Alarm</summary>
        <UsageType(UsageTypes.Selector)> ACClearAlarm = &H283
        ''' <summary> Application Control (AC) Snooze Alarm</summary>
        <UsageType(UsageTypes.Selector)> ACSnoozeAlarm = &H284
        ''' <summary> Application Control (AC) Reset Alarm</summary>
        <UsageType(UsageTypes.Selector)> ACResetAlarm = &H285
        ''' <summary> Application Control (AC) Synchronize</summary>
        <UsageType(UsageTypes.Selector)> ACSynchronize = &H286
        ''' <summary> Application Control (AC) Send/Receive</summary>
        <UsageType(UsageTypes.Selector)> ACSendReceive = &H287
        ''' <summary> Application Control (AC) Send To</summary>
        <UsageType(UsageTypes.Selector)> ACSendTo = &H288
        ''' <summary> Application Control (AC) Reply</summary>
        <UsageType(UsageTypes.Selector)> ACReply = &H289
        ''' <summary> Application Control (AC) Reply All</summary>
        <UsageType(UsageTypes.Selector)> ACReplyAll = &H28A
        ''' <summary> Application Control (AC) Forward Msg</summary>
        <UsageType(UsageTypes.Selector)> ACForwardMsg = &H28B
        ''' <summary> Application Control (AC) Send</summary>
        <UsageType(UsageTypes.Selector)> ACSend = &H28C
        ''' <summary> Application Control (AC) Attach File</summary>
        <UsageType(UsageTypes.Selector)> ACAttachFile = &H28D
        ''' <summary> Application Control (AC) Upload</summary>
        <UsageType(UsageTypes.Selector)> ACUpload = &H28E
        ''' <summary> Application Control (AC) Download (Save Target As)</summary>
        <UsageType(UsageTypes.Selector)> ACDownload = &H28F
        ''' <summary> Application Control (AC) Set Borders</summary>
        <UsageType(UsageTypes.Selector)> ACSetBorders = &H290
        ''' <summary> Application Control (AC) Insert Row</summary>
        <UsageType(UsageTypes.Selector)> ACInsertRow = &H291
        ''' <summary> Application Control (AC) Insert Column</summary>
        <UsageType(UsageTypes.Selector)> ACInsertColumn = &H292
        ''' <summary> Application Control (AC) Insert File</summary>
        <UsageType(UsageTypes.Selector)> ACInsertFile = &H293
        ''' <summary> Application Control (AC) Insert Picture</summary>
        <UsageType(UsageTypes.Selector)> ACInsertPicture = &H294
        ''' <summary> Application Control (AC) Insert Object</summary>
        <UsageType(UsageTypes.Selector)> ACInsertObject = &H295
        ''' <summary> Application Control (AC) Insert Symbol</summary>
        <UsageType(UsageTypes.Selector)> ACInsertSymbol = &H296
        ''' <summary> Application Control (AC) Save and Close</summary>
        <UsageType(UsageTypes.Selector)> ACSaveandClose = &H297
        ''' <summary> Application Control (AC) Rename</summary>
        <UsageType(UsageTypes.Selector)> ACRename = &H298
        ''' <summary> Application Control (AC) Merge</summary>
        <UsageType(UsageTypes.Selector)> ACMerge = &H299
        ''' <summary> Application Control (AC) Split</summary>
        <UsageType(UsageTypes.Selector)> ACSplit = &H29A
        ''' <summary> Application Control (AC) Disribute Horizontally</summary>
        <UsageType(UsageTypes.Selector)> ACDisributeHorizontally = &H29B
        ''' <summary> Application Control (AC) Distribute Vertically</summary>
        <UsageType(UsageTypes.Selector)> ACDistributeVertically = &H29C
    End Enum

    ''' <summary>Contains HID usage code s for <see cref="UsagePages.Digitizer"/> page</summary>
    ''' <remarks>Values not defined in this enumeration are reserved</remarks>
    Public Enum Usages_Digitizer
        ''' <summary>Unidentified</summary>
        Undefined = 0
        ''' <summary>Digitizer</summary>
        <UsageType(UsageTypes.ApplicationCollection)> Digitizer = &H1
        ''' <summary>Pen</summary>
        <UsageType(UsageTypes.ApplicationCollection)> Pen = &H2
        ''' <summary>Light Pen</summary>
        <UsageType(UsageTypes.ApplicationCollection)> LightPen = &H3
        ''' <summary>Touch Screen</summary>
        <UsageType(UsageTypes.ApplicationCollection)> TouchScreen = &H4
        ''' <summary>Touch Pad</summary>
        <UsageType(UsageTypes.ApplicationCollection)> TouchPad = &H5
        ''' <summary>White Board</summary>
        <UsageType(UsageTypes.ApplicationCollection)> WhiteBoard = &H6
        ''' <summary>Coordinate Measuring Machine</summary>
        <UsageType(UsageTypes.ApplicationCollection)> CoordinateMeasuringMachine = &H7
        ''' <summary>3D Digitizer</summary>
        <UsageType(UsageTypes.ApplicationCollection)> Digitizer3D = &H8
        ''' <summary>Stereo Plotter</summary>
        <UsageType(UsageTypes.ApplicationCollection)> StereoPlotter = &H9
        ''' <summary>Articulated Arm</summary>
        <UsageType(UsageTypes.ApplicationCollection)> ArticulatedArm = &HA
        ''' <summary>Armature</summary>
        <UsageType(UsageTypes.ApplicationCollection)> Armature = &HB
        ''' <summary>Multiple Point Digitizer</summary>
        <UsageType(UsageTypes.ApplicationCollection)> MultiplePointDigitizer = &HC
        ''' <summary>Free Space Wand</summary>
        <UsageType(UsageTypes.ApplicationCollection)> FreeSpaceWand = &HD
        ''' <summary>Stylus</summary>
        <UsageType(UsageTypes.LogicalCollection)> Stylus = &H20
        ''' <summary>Puck</summary>
        <UsageType(UsageTypes.LogicalCollection)> Puck = &H21
        ''' <summary>Finger</summary>
        <UsageType(UsageTypes.LogicalCollection)> Finger = &H22
        ''' <summary>Tip Pressure</summary>
        <UsageType(UsageTypes.DynamicValue)> TipPressure = &H30
        ''' <summary>Barrel Pressure</summary>
        <UsageType(UsageTypes.DynamicValue)> BarrelPressure = &H31
        ''' <summary>In Range</summary>
        <UsageType(UsageTypes.MomentaryControl)> InRange = &H32
        ''' <summary>Touch</summary>
        <UsageType(UsageTypes.MomentaryControl)> Touch = &H33
        ''' <summary>Untouch</summary>
        <UsageType(UsageTypes.OneShotControl)> Untouch = &H34
        ''' <summary>Tap</summary>
        <UsageType(UsageTypes.OneShotControl)> Tap = &H35
        ''' <summary>Quality</summary>
        <UsageType(UsageTypes.DynamicValue)> Quality = &H36
        ''' <summary>Data Valid</summary>
        <UsageType(UsageTypes.MomentaryControl)> DataValid = &H37
        ''' <summary>Transducer Index</summary>
        <UsageType(UsageTypes.DynamicValue)> TransducerIndex = &H38
        ''' <summary>Tablet Function Keys</summary>
        <UsageType(UsageTypes.LogicalCollection)> TabletFunctionKeys = &H39
        ''' <summary>Program Change Keys</summary>
        <UsageType(UsageTypes.LogicalCollection)> ProgramChangeKeys = &H3A
        ''' <summary>Battery Strength</summary>
        <UsageType(UsageTypes.DynamicValue)> BatteryStrength = &H3B
        ''' <summary>Invert</summary>
        <UsageType(UsageTypes.MomentaryControl)> Invert = &H3C
        ''' <summary>X Tilt</summary>
        <UsageType(UsageTypes.DynamicValue)> XTilt = &H3D
        ''' <summary>Y Tilt</summary>
        <UsageType(UsageTypes.DynamicValue)> YTilt = &H3E
        ''' <summary>Azimuth</summary>
        <UsageType(UsageTypes.DynamicValue)> Azimuth = &H3F
        ''' <summary>Altitude</summary>
        <UsageType(UsageTypes.DynamicValue)> Altitude = &H40
        ''' <summary>Twist</summary>
        <UsageType(UsageTypes.DynamicValue)> Twist = &H41
        ''' <summary>Tip Switch</summary>
        <UsageType(UsageTypes.MomentaryControl)> TipSwitch = &H42
        ''' <summary>Secondary Tip Switch</summary>
        <UsageType(UsageTypes.MomentaryControl)> SecondaryTipSwitch = &H43
        ''' <summary>Barrel Switch</summary>
        <UsageType(UsageTypes.MomentaryControl)> BarrelSwitch = &H44
        ''' <summary>Eraser</summary>
        <UsageType(UsageTypes.MomentaryControl)> Eraser = &H45
        ''' <summary>Tablet Pick</summary>
        <UsageType(UsageTypes.MomentaryControl)> TabletPick = &H46
    End Enum

    ''' <summary>Contains HID usage code s for <see cref="UsagePages.AlphanumericDisplay "/> page</summary>
    ''' <remarks>Values not defined in this enumeration are reserved</remarks>
    Public Enum Usages_AlphanumericDisplay
        ''' <summary>Unidentified</summary>
        Undefined = 0
        ''' <summary>Alphanumeric Display</summary>
        <UsageType(UsageTypes.ApplicationCollection)> AlphanumericDisplay = &H1
        ''' <summary>Bitmapped Display</summary>
        <UsageType(UsageTypes.ApplicationCollection)> BitmappedDisplay = &H2
        ''' <summary>Display Attributes Report</summary>
        <UsageType(UsageTypes.LogicalCollection)> DisplayAttributesReport = &H20
        ''' <summary>ASCII Character Set</summary>
        <UsageType(UsageTypes.StaticFlag)> ASCIICharacterSet = &H21
        ''' <summary>Data Read Back</summary>
        <UsageType(UsageTypes.StaticFlag)> DataReadBack = &H22
        ''' <summary>Font Read Back</summary>
        <UsageType(UsageTypes.StaticFlag)> FontReadBack = &H23
        ''' <summary>Display Control Report</summary>
        <UsageType(UsageTypes.LogicalCollection)> DisplayControlReport = &H24
        ''' <summary>Clear Display</summary>
        <UsageType(UsageTypes.DynamicFlag)> ClearDisplay = &H25
        ''' <summary>Display Enable</summary>
        <UsageType(UsageTypes.DynamicFlag)> DisplayEnable = &H26
        ''' <summary>Screen Saver Delay</summary>
        <UsageType(UsageTypes.DynamicValue Or UsageTypes.StaticValue)> ScreenSaverDelaySV = &H27
        ''' <summary>Screen Saver Enable</summary>
        <UsageType(UsageTypes.DynamicFlag)> ScreenSaverEnable = &H28
        ''' <summary>Vertical Scroll</summary>
        <UsageType(UsageTypes.DynamicFlag Or UsageTypes.StaticFlag)> VerticalScroll = &H29
        ''' <summary>Horizontal Scroll</summary>
        <UsageType(UsageTypes.DynamicFlag Or UsageTypes.StaticFlag)> HorizontalScroll = &H2A
        ''' <summary>Character Report</summary>
        <UsageType(UsageTypes.LogicalCollection)> CharacterReport = &H2B
        ''' <summary>Display Data</summary>
        <UsageType(UsageTypes.DynamicValue)> DisplayData = &H2C
        ''' <summary>Display Status</summary>
        <UsageType(UsageTypes.LogicalCollection)> DisplayStatus = &H2D
        ''' <summary>Stat Not Ready</summary>
        <UsageType(UsageTypes.Selector)> StatNotReady = &H2E
        ''' <summary>Stat Ready</summary>
        <UsageType(UsageTypes.Selector)> StatReady = &H2F
        ''' <summary>Err Not a loadable character</summary>
        <UsageType(UsageTypes.Selector)> ErrNotALoadableCharacter = &H30
        ''' <summary>Err Font data cannot be read</summary>
        <UsageType(UsageTypes.Selector)> ErrFontDataCannotBeRead = &H31
        ''' <summary>Cursor Position Report</summary>
        <UsageType(UsageTypes.LogicalCollection)> CursorPositionReport = &H32
        ''' <summary>Row</summary>
        <UsageType(UsageTypes.DynamicValue)> Row = &H33
        ''' <summary>Column</summary>
        <UsageType(UsageTypes.DynamicValue)> Column = &H34
        ''' <summary>Rows</summary>
        <UsageType(UsageTypes.StaticValue)> Rows = &H35
        ''' <summary>Columns</summary>
        <UsageType(UsageTypes.StaticValue)> Columns = &H36
        ''' <summary>Cursor Pixel Positioning</summary>
        <UsageType(UsageTypes.StaticFlag)> CursorPixelPositioning = &H37
        ''' <summary>Cursor Mode</summary>
        <UsageType(UsageTypes.DynamicFlag)> CursorMode = &H38
        ''' <summary>Cursor Enable</summary>
        <UsageType(UsageTypes.DynamicFlag)> CursorEnable = &H39
        ''' <summary>Cursor Blink</summary>
        <UsageType(UsageTypes.DynamicFlag)> CursorBlink = &H3A
        ''' <summary>Font Report</summary>
        <UsageType(UsageTypes.LogicalCollection)> FontReport = &H3B
        ''' <summary>Font Data Buffered</summary>
        <UsageType(UsageTypes.BufferedBytes)> FontData = &H3C
        ''' <summary>Character Width</summary>
        <UsageType(UsageTypes.StaticValue)> CharacterWidth = &H3D
        ''' <summary>Character Height</summary>
        <UsageType(UsageTypes.StaticValue)> CharacterHeight = &H3E
        ''' <summary>Character Spacing Horizontal</summary>
        <UsageType(UsageTypes.StaticValue)> CharacterSpacingHorizontal = &H3F
        ''' <summary>Character Spacing Vertical</summary>
        <UsageType(UsageTypes.StaticValue)> CharacterSpacingVertical = &H40
        ''' <summary>Unicode Character Set</summary>
        <UsageType(UsageTypes.StaticFlag)> UnicodeCharacterSet = &H41
        ''' <summary>Font 7-Segment</summary>
        <UsageType(UsageTypes.StaticFlag)> Font7Segment = &H42
        ''' <summary>7-Segment Direct Map</summary>
        <UsageType(UsageTypes.StaticFlag)> Segment7DirectMap = &H43
        ''' <summary>Font 14-Segment</summary>
        <UsageType(UsageTypes.StaticFlag)> Font14Segment = &H44
        ''' <summary>14-Segment Direct Map</summary>
        <UsageType(UsageTypes.StaticFlag)> Segment14DirectMap = &H45
        ''' <summary>Display Brightness</summary>
        <UsageType(UsageTypes.DynamicValue)> DisplayBrightness = &H46
        ''' <summary>Display Contrast</summary>
        <UsageType(UsageTypes.DynamicValue)> DisplayContrast = &H47
        ''' <summary>Character Attribute</summary>
        <UsageType(UsageTypes.LogicalCollection)> CharacterAttribute = &H48
        ''' <summary>Attribute Readback</summary>
        <UsageType(UsageTypes.StaticFlag)> AttributeReadback = &H49
        ''' <summary>Attribute Data</summary>
        <UsageType(UsageTypes.DynamicValue)> AttributeData = &H4A
        ''' <summary>Char Attr Enhance</summary>
        <UsageType(UsageTypes.OnOffControl)> CharAttrEnhance = &H4B
        ''' <summary>Char Attr Underline</summary>
        <UsageType(UsageTypes.OnOffControl)> CharAttrUnderline = &H4C
        ''' <summary>Char Attr Blink</summary>
        <UsageType(UsageTypes.OnOffControl)> CharAttrBlink = &H4D
        ''' <summary>Bitmap Size X</summary>
        <UsageType(UsageTypes.StaticValue)> BitmapSizeX = &H80
        ''' <summary>Bitmap Size Y</summary>
        <UsageType(UsageTypes.StaticValue)> BitmapSizeY = &H81
        ''' <summary>Bit Depth Format</summary>
        <UsageType(UsageTypes.StaticValue)> BitDepthFormat = &H83
        ''' <summary>Display Orientation</summary>
        <UsageType(UsageTypes.DynamicValue)> DisplayOrientation = &H84
        ''' <summary>Palette Report</summary>
        <UsageType(UsageTypes.LogicalCollection)> PaletteReport = &H85
        ''' <summary>Palette Data Size</summary>
        <UsageType(UsageTypes.StaticValue)> PaletteDataSize = &H86
        ''' <summary>Palette Data Offset</summary>
        <UsageType(UsageTypes.StaticValue)> PaletteDataOffset = &H87
        ''' <summary>Palette Data</summary>
        <UsageType(UsageTypes.BufferedBytes)> PaletteData = &H88
        ''' <summary>Blit Report</summary>
        <UsageType(UsageTypes.LogicalCollection)> BlitReport = &H8A
        ''' <summary>Blit Rectangle X1</summary>
        <UsageType(UsageTypes.StaticValue)> BlitRectangleX1 = &H8B
        ''' <summary>Blit Rectangle Y1</summary>
        <UsageType(UsageTypes.StaticValue)> BlitRectangleY1 = &H8C
        ''' <summary>Blit Rectangle X2</summary>
        <UsageType(UsageTypes.StaticValue)> BlitRectangleX2 = &H8D
        ''' <summary>Blit Rectangle Y2</summary>
        <UsageType(UsageTypes.StaticValue)> BlitRectangleY2 = &H8E
        ''' <summary>Blit Data</summary>
        <UsageType(UsageTypes.BufferedBytes)> BlitData = &H8F
        ''' <summary>Soft Button</summary>
        <UsageType(UsageTypes.LogicalCollection)> SoftButton = &H90
        ''' <summary>Soft Button ID</summary>
        <UsageType(UsageTypes.StaticValue)> SoftButtonID = &H91
        ''' <summary>Soft Button Side</summary>
        <UsageType(UsageTypes.StaticValue)> SoftButtonSide = &H92
        ''' <summary>Soft Button Offset 1</summary>
        <UsageType(UsageTypes.StaticValue)> SoftButtonOffset1 = &H93
        ''' <summary>Soft Button Offset 2</summary>
        <UsageType(UsageTypes.StaticValue)> SoftButtonOffset2 = &H94
        ''' <summary>Soft Button Report</summary>
        <UsageType(UsageTypes.StaticValue)> SoftButtonReport = &H95
    End Enum

    ''' <summary>Contains HID usage code s for <see cref="UsagePages.MedicalInstruments "/> page</summary>
    ''' <remarks>Values not defined in this enumeration are reserved</remarks>
    Public Enum Usages_MedicalInstruments
        ''' <summary>Unidentified</summary>
        Undefined = 0
        ''' <summary>Medical Ultrasound</summary>
        <UsageType(UsageTypes.ApplicationCollection)> MedicalUltrasound = &H1
        ''' <summary>VCR/Acquisition</summary>
        <UsageType(UsageTypes.OnOffControl)> VCRAcquisition = &H20
        ''' <summary>Freeze/Thaw</summary>
        <UsageType(UsageTypes.OnOffControl)> FreezeThaw = &H21
        ''' <summary>Clip Store</summary>
        <UsageType(UsageTypes.OneShotControl)> ClipStore = &H22
        ''' <summary>Update</summary>
        <UsageType(UsageTypes.OneShotControl)> Update = &H23
        ''' <summary>Next</summary>
        <UsageType(UsageTypes.OneShotControl)> [Next] = &H24
        ''' <summary>Save</summary>
        <UsageType(UsageTypes.OneShotControl)> Save = &H25
        ''' <summary>Print</summary>
        <UsageType(UsageTypes.OneShotControl)> Print = &H26
        ''' <summary>Microphone Enable</summary>
        <UsageType(UsageTypes.OneShotControl)> MicrophoneEnable = &H27
        ''' <summary>Cine</summary>
        <UsageType(UsageTypes.LinearControl)> Cine = &H40
        ''' <summary>Transmit Power</summary>
        <UsageType(UsageTypes.LinearControl)> TransmitPower = &H41
        ''' <summary>Volume</summary>
        <UsageType(UsageTypes.LinearControl)> Volume = &H42
        ''' <summary>Focus</summary>
        <UsageType(UsageTypes.LinearControl)> Focus = &H43
        ''' <summary>Depth</summary>
        <UsageType(UsageTypes.LinearControl)> Depth = &H44
        ''' <summary>Soft Step - Primary</summary>
        <UsageType(UsageTypes.LinearControl)> SoftStepPrimary = &H60
        ''' <summary>Soft Step - Secondary</summary>
        <UsageType(UsageTypes.LinearControl)> SoftStepSecondary = &H61
        ''' <summary>Depth Gain Compensation</summary>
        <UsageType(UsageTypes.LinearControl)> DepthGainCompensation = &H70
        ''' <summary>Zoom Select</summary>
        <UsageType(UsageTypes.OneShotControl)> ZoomSelect = &H80
        ''' <summary>Zoom Adjust</summary>
        <UsageType(UsageTypes.LinearControl)> ZoomAdjust = &H81
        ''' <summary>Spectral Doppler Mode Select</summary>
        <UsageType(UsageTypes.OneShotControl)> SpectralDopplerModeSelect = &H82
        ''' <summary>Spectral Doppler Adjust</summary>
        <UsageType(UsageTypes.LinearControl)> SpectralDopplerAdjust = &H83
        ''' <summary>Color Doppler Mode Select</summary>
        <UsageType(UsageTypes.OneShotControl)> ColorDopplerModeSelect = &H84
        ''' <summary>Color Doppler Adjust</summary>
        <UsageType(UsageTypes.LinearControl)> ColorDopplerAdjust = &H85
        ''' <summary>Motion Mode Select</summary>
        <UsageType(UsageTypes.OneShotControl)> MotionModeSelect = &H86
        ''' <summary>Motion Mode Adjust</summary>
        <UsageType(UsageTypes.LinearControl)> MotionModeAdjust = &H87
        ''' <summary>2-D Mode Select</summary>
        <UsageType(UsageTypes.OneShotControl)> Mode2DSelect = &H88
        ''' <summary>2-D Mode Adjust</summary>
        <UsageType(UsageTypes.LinearControl)> Mode2DAdjust = &H89
        ''' <summary>Soft Control Select</summary>
        <UsageType(UsageTypes.OneShotControl)> SoftControlSelect = &HA0
        ''' <summary>Soft Control Adjust</summary>
        <UsageType(UsageTypes.LinearControl)> SoftControlAdjust = &HA1
    End Enum

    ''' <summary>Contains HID usage code s for <see cref="UsagePages.PIDPage"/> page (Physical Input Devices)</summary>
    ''' <remarks>Values not defined in this enumeration are reserved</remarks>
    Public Enum Usages_PIDPage
        ''' <summary>Undefined</summary>
        Undefined = &H0
        ''' <summary>Physical Interface Device</summary>
        <UsageType(UsageTypes.ApplicationCollection)> PhysicalInterfaceDevice = &H1
        ''' <summary>Normal</summary>
        <UsageType(UsageTypes.DynamicValue)> Normal = &H20
        ''' <summary>Set Effect Report</summary>
        <UsageType(UsageTypes.LogicalCollection)> SetEffectReport = &H21
        ''' <summary>Effect Block Index</summary>
        <UsageType(UsageTypes.DynamicValue)> EffectBlockIndex = &H22
        ''' <summary>Parameter Block Offset</summary>
        <UsageType(UsageTypes.DynamicValue)> ParameterBlockOffset = &H23
        ''' <summary>ROM Flag</summary>
        <UsageType(UsageTypes.DynamicFlag)> ROMFlag = &H24
        ''' <summary>Effect Type</summary>
        <UsageType(UsageTypes.NamedArray)> EffectType = &H25
        ''' <summary> Effect type (ET) Constant Force</summary>
        <UsageType(UsageTypes.Selector)> ETConstantForce = &H26
        ''' <summary> Effect type (ET) Ramp</summary>
        <UsageType(UsageTypes.Selector)> ETRamp = &H27
        ''' <summary> Effect type (ET) Custom Force Data</summary>
        <UsageType(UsageTypes.Selector)> ETCustomForceData = &H28
        ''' <summary> Effect type (ET) Square</summary>
        <UsageType(UsageTypes.Selector)> ETSquare = &H30
        ''' <summary> Effect type (ET) Sine</summary>
        <UsageType(UsageTypes.Selector)> ETSine = &H31
        ''' <summary> Effect type (ET) Triangle</summary>
        <UsageType(UsageTypes.Selector)> ETTriangle = &H32
        ''' <summary> Effect type (ET) Sawtooth Up</summary>
        <UsageType(UsageTypes.Selector)> ETSawtoothUp = &H33
        ''' <summary> Effect type (ET) Sawtooth Down</summary>
        <UsageType(UsageTypes.Selector)> ETSawtoothDown = &H34
        ''' <summary> Effect type (ET) Spring</summary>
        <UsageType(UsageTypes.Selector)> ETSpring = &H40
        ''' <summary> Effect type (ET) Damper</summary>
        <UsageType(UsageTypes.Selector)> ETDamper = &H41
        ''' <summary> Effect type (ET) Inertia</summary>
        <UsageType(UsageTypes.Selector)> ETInertia = &H42
        ''' <summary>Period</summary>
        <UsageType(UsageTypes.DynamicValue)> Period = &H72
        ''' <summary>Set Constant Force Report</summary>
        <UsageType(UsageTypes.LogicalCollection)> SetConstantForceReport = &H73
        ''' <summary>Set Ramp Force Report</summary>
        <UsageType(UsageTypes.LogicalCollection)> SetRampForceReport = &H74
        ''' <summary>Ramp Start</summary>
        <UsageType(UsageTypes.DynamicValue)> RampStart = &H75
        ''' <summary>Ramp End</summary>
        <UsageType(UsageTypes.DynamicValue)> RampEnd = &H76
        ''' <summary>Effect Operation Report</summary>
        <UsageType(UsageTypes.LogicalCollection)> EffectOperationReport = &H77
        ''' <summary>Effect Operation</summary>
        <UsageType(UsageTypes.NamedArray)> EffectOperation = &H78
        ''' <summary>Op Effect Start</summary>
        <UsageType(UsageTypes.Selector)> OpEffectStart = &H79
        ''' <summary>Op Effect Start Solo</summary>
        <UsageType(UsageTypes.Selector)> OpEffectStartSolo = &H7A
        ''' <summary>Op Effect Stop</summary>
        <UsageType(UsageTypes.Selector)> OpEffectStop = &H7B
        ''' <summary>Loop Count</summary>
        <UsageType(UsageTypes.DynamicValue)> LoopCount = &H7C
        ''' <summary>Device Gain Report</summary>
        <UsageType(UsageTypes.LogicalCollection)> DeviceGainReport = &H7D
        ''' <summary>Device Gain</summary>
        <UsageType(UsageTypes.DynamicValue)> DeviceGain = &H7E
        ''' <summary>Physical Interface Devices (PID) Pool Report</summary>
        <UsageType(UsageTypes.LogicalCollection)> PIDPoolReport = &H7F
        ''' <summary>RAM Pool Size</summary>
        <UsageType(UsageTypes.DynamicValue)> RAMPoolSize = &H80
        ''' <summary>ROM Pool Size</summary>
        <UsageType(UsageTypes.StaticValue)> ROMPoolSize = &H81
        ''' <summary>ROM Effect Block Count</summary>
        <UsageType(UsageTypes.StaticValue)> ROMEffectBlockCount = &H82
        ''' <summary>Simultaneous Effects Max</summary>
        <UsageType(UsageTypes.StaticValue)> SimultaneousEffectsMax = &H83
        ''' <summary>Pool Alignment</summary>
        <UsageType(UsageTypes.StaticValue)> PoolAlignment = &H84
        ''' <summary>Physical Interface Devices (PID) Pool Move Report</summary>
        <UsageType(UsageTypes.LogicalCollection)> PIDPoolMoveReport = &H85
        ''' <summary>Move Source</summary>
        <UsageType(UsageTypes.DynamicValue)> MoveSource = &H86
        ''' <summary>Move Destination</summary>
        <UsageType(UsageTypes.DynamicValue)> MoveDestination = &H87
        ''' <summary> Effect type (ET) Friction</summary>
        <UsageType(UsageTypes.Selector)> ETFriction = &H43
        ''' <summary>Duration</summary>
        <UsageType(UsageTypes.DynamicValue)> Duration = &H50
        ''' <summary>Sample Period</summary>
        <UsageType(UsageTypes.DynamicValue)> SamplePeriod = &H51
        ''' <summary>Gain</summary>
        <UsageType(UsageTypes.DynamicValue)> Gain = &H52
        ''' <summary>Trigger Button</summary>
        <UsageType(UsageTypes.DynamicValue)> TriggerButton = &H53
        ''' <summary>Trigger Repeat Interval</summary>
        <UsageType(UsageTypes.DynamicValue)> TriggerRepeatInterval = &H54
        ''' <summary>Axes Enable</summary>
        <UsageType(UsageTypes.UsageSwitch)> AxesEnable = &H55
        ''' <summary>Direction Enable</summary>
        <UsageType(UsageTypes.DynamicFlag)> DirectionEnable = &H56
        ''' <summary>Direction</summary>
        <UsageType(UsageTypes.LogicalCollection)> Direction = &H57
        ''' <summary>Type Specific Block Offset</summary>
        <UsageType(UsageTypes.LogicalCollection)> TypeSpecificBlockOffset = &H58
        ''' <summary>Block Type</summary>
        <UsageType(UsageTypes.NamedArray)> BlockType = &H59
        ''' <summary>Set Envelope Report</summary>
        <UsageType(UsageTypes.LogicalCollection)> SetEnvelopeReport = &H5A
        ''' <summary>Attack Level</summary>
        <UsageType(UsageTypes.DynamicValue)> AttackLevel = &H5B
        ''' <summary>Attack Time</summary>
        <UsageType(UsageTypes.DynamicValue)> AttackTime = &H5C
        ''' <summary>Fade Level</summary>
        <UsageType(UsageTypes.DynamicValue)> FadeLevel = &H5D
        ''' <summary>Fade Time</summary>
        <UsageType(UsageTypes.DynamicValue)> FadeTime = &H5E
        ''' <summary>Set Condition Report</summary>
        <UsageType(UsageTypes.LogicalCollection)> SetConditionReport = &H5F
        ''' <summary>CP Offset</summary>
        <UsageType(UsageTypes.DynamicValue)> CPOffset = &H60
        ''' <summary>Positive Coefficient</summary>
        <UsageType(UsageTypes.DynamicValue)> PositiveCoefficient = &H61
        ''' <summary>Negative Coefficient</summary>
        <UsageType(UsageTypes.DynamicValue)> NegativeCoefficient = &H62
        ''' <summary>Positive Saturation</summary>
        <UsageType(UsageTypes.DynamicValue)> PositiveSaturation = &H63
        ''' <summary>Negative Saturation</summary>
        <UsageType(UsageTypes.DynamicValue)> NegativeSaturation = &H64
        ''' <summary>Dead Band</summary>
        <UsageType(UsageTypes.DynamicValue)> DeadBand = &H65
        ''' <summary>Download Force Sample</summary>
        <UsageType(UsageTypes.LogicalCollection)> DownloadForceSample = &H66
        ''' <summary>Isoch Custom Force Enable</summary>
        IsochCustomForceEnable = &H67
        ''' <summary>Custom Force Data Report</summary>
        <UsageType(UsageTypes.LogicalCollection)> CustomForceDataReport = &H68
        ''' <summary>Custom Force Data</summary>
        <UsageType(UsageTypes.DynamicValue)> CustomForceData = &H69
        ''' <summary>Custom Force Vendor Defined Data</summary>
        <UsageType(UsageTypes.DynamicValue)> CustomForceVendorDefinedData = &H6A
        ''' <summary>Set Custom Force Report</summary>
        <UsageType(UsageTypes.LogicalCollection)> SetCustomForceReport = &H6B
        ''' <summary>Custom Force Data Offset</summary>
        <UsageType(UsageTypes.DynamicValue)> CustomForceDataOffset = &H6C
        ''' <summary>Sample Count</summary>
        <UsageType(UsageTypes.DynamicValue)> SampleCount = &H6D
        ''' <summary>Set Periodic Report</summary>
        <UsageType(UsageTypes.LogicalCollection)> SetPeriodicReport = &H6E
        ''' <summary>Offset</summary>
        <UsageType(UsageTypes.DynamicValue)> Offset = &H6F
        ''' <summary>Magnitude</summary>
        <UsageType(UsageTypes.DynamicValue)> Magnitude = &H70
        ''' <summary>Phase</summary>
        <UsageType(UsageTypes.DynamicValue)> Phase = &H71
        ''' <summary>Move Length</summary>
        <UsageType(UsageTypes.DynamicValue)> MoveLength = &H88
        ''' <summary>Physical Interface Devices (PID) Block Load Report</summary>
        <UsageType(UsageTypes.LogicalCollection)> PIDBlockLoadReport = &H89
        ''' <summary>Block Load Status</summary>
        <UsageType(UsageTypes.NamedArray)> BlockLoadStatus = &H8B
        ''' <summary>Block Load Success</summary>
        <UsageType(UsageTypes.Selector)> BlockLoadSuccess = &H8C
        ''' <summary>Block Load Full</summary>
        <UsageType(UsageTypes.Selector)> BlockLoadFull = &H8D
        ''' <summary>Block Load Error</summary>
        <UsageType(UsageTypes.Selector)> BlockLoadError = &H8E
        ''' <summary>Block Handle</summary>
        <UsageType(UsageTypes.StaticValue)> BlockHandle = &H8F
        ''' <summary>Physical Interface Devices (PID) Block Free Report</summary>
        <UsageType(UsageTypes.LogicalCollection)> PIDBlockFreeReport = &H90
        ''' <summary>Type Specific Block Handle</summary>
        <UsageType(UsageTypes.LogicalCollection)> TypeSpecificBlockHandle = &H91
        ''' <summary>Physical Interface Devices (PID) State Report</summary>
        <UsageType(UsageTypes.LogicalCollection)> PIDStateReport = &H92
        ''' <summary>Physical Interface Devices (PID) Effect State</summary>
        <UsageType(UsageTypes.NamedArray)> PIDEffectState = 93
        ''' <summary>Effect Playing</summary>
        <UsageType(UsageTypes.DynamicFlag)> EffectPlaying = &H94
        ''' <summary>Physical Interface Devices (PID) Device Control Report</summary>
        <UsageType(UsageTypes.LogicalCollection)> PIDDeviceControlReport = &H95
        ''' <summary>Physical Interface Devices (PID) Device Control</summary>
        <UsageType(UsageTypes.NamedArray)> PIDDeviceControl = &H96
        ''' <summary>Device Control (DC) Enable Actuators</summary>
        <UsageType(UsageTypes.Selector)> DCEnableActuators = &H97
        ''' <summary>Device Control (DC) Disable Actuators</summary>
        <UsageType(UsageTypes.Selector)> DCDisableActuators = &H98
        ''' <summary>Device Control (DC) Stop All Effects</summary>
        <UsageType(UsageTypes.Selector)> DCStopAllEffects = &H99
        ''' <summary>Device Control (DC) Device Reset</summary>
        <UsageType(UsageTypes.Selector)> DCDeviceReset = &H9A
        ''' <summary>Device Control (DC) Device Pause</summary>
        <UsageType(UsageTypes.Selector)> DCDevicePause = &H9B
        ''' <summary>Device Control (DC) Device Continue</summary>
        <UsageType(UsageTypes.Selector)> DCDeviceContinue = &H9C
        ''' <summary>Device Paused</summary>
        <UsageType(UsageTypes.DynamicFlag)> DevicePaused = &H9F
        ''' <summary>Actuators Enabled</summary>
        <UsageType(UsageTypes.DynamicFlag)> ActuatorsEnabled = &HA0
        ''' <summary>Safety Switch</summary>
        <UsageType(UsageTypes.DynamicFlag)> SafetySwitch = &HA4
        ''' <summary>Actuator Override Switch</summary>
        <UsageType(UsageTypes.DynamicFlag)> ActuatorOverrideSwitch = &HA5
        ''' <summary>Actuator Power</summary>
        <UsageType(UsageTypes.OnOffControl)> ActuatorPower = &HA6
        ''' <summary>Start Delay</summary>
        <UsageType(UsageTypes.DynamicValue)> StartDelay = &HA7
        ''' <summary>Parameter Block Size</summary>
        <UsageType(UsageTypes.LogicalCollection)> ParameterBlockSize = &HA8
        ''' <summary>Device Managed Pool</summary>
        <UsageType(UsageTypes.StaticFlag)> DeviceManagedPool = &HA9
        ''' <summary>Shared Parameter Blocks</summary>
        <UsageType(UsageTypes.StaticFlag)> SharedParameterBlocks = &HAA
        ''' <summary>Create New Effect Report</summary>
        <UsageType(UsageTypes.LogicalCollection)> CreateNewEffectReport = &HAB
        ''' <summary>RAM Pool Available</summary>
        <UsageType(UsageTypes.DynamicValue)> RAMPoolAvailable = &HAC
    End Enum

    ''' <summary>Contains HID usage code s for <see cref="UsagePages.PowerDevice"/> page</summary>
    ''' <remarks>Values not defined in this enumeration are reserved</remarks>
    Public Enum Usages_PowerDevice
        ''' <summary>Undefined</summary>
        Undefined = 0
        ''' <summary>iName</summary>
        <UsageType(UsageTypes.StaticValue)> iName = &H1
        ''' <summary>PresentStatus</summary>
        <UsageType(UsageTypes.LogicalCollection)> PresentStatus = &H2
        ''' <summary>ChangedStatus</summary>
        <UsageType(UsageTypes.LogicalCollection)> ChangedStatus = &H3
        ''' <summary>UPS</summary>
        <UsageType(UsageTypes.ApplicationCollection)> UPS = &H4
        ''' <summary>PowerSupply</summary>
        <UsageType(UsageTypes.ApplicationCollection)> PowerSupply = &H5
        ''' <summary>BatterySystem</summary>
        <UsageType(UsageTypes.PhysicalCollection)> BatterySystem = &H10
        ''' <summary>BatterySystemID</summary>
        <UsageType(UsageTypes.StaticValue)> BatterySystemID = &H11
        ''' <summary>Battery</summary>
        <UsageType(UsageTypes.PhysicalCollection)> Battery = &H12
        ''' <summary>BatteryID</summary>
        <UsageType(UsageTypes.StaticValue)> BatteryID = &H13
        ''' <summary>Charger</summary>
        <UsageType(UsageTypes.PhysicalCollection)> Charger = &H14
        ''' <summary>ChargerID</summary>
        <UsageType(UsageTypes.StaticValue)> ChargerID = &H15
        ''' <summary>PowerConverter</summary>
        <UsageType(UsageTypes.PhysicalCollection)> PowerConverter = &H16
        ''' <summary>PowerConverterID</summary>
        <UsageType(UsageTypes.StaticValue)> PowerConverterID = &H17
        ''' <summary>OutletSystem</summary>
        <UsageType(UsageTypes.PhysicalCollection)> OutletSystem = &H18
        ''' <summary>OutletSystemID</summary>
        <UsageType(UsageTypes.StaticValue)> OutletSystemID = &H19
        ''' <summary>Input</summary>
        <UsageType(UsageTypes.PhysicalCollection)> Input = &H1A
        ''' <summary>InputID</summary>
        <UsageType(UsageTypes.StaticValue)> InputID = &H1B
        ''' <summary>Output</summary>
        <UsageType(UsageTypes.PhysicalCollection)> Output = &H1C
        ''' <summary>OutputID</summary>
        <UsageType(UsageTypes.StaticValue)> OutputID = &H1D
        ''' <summary>Flow</summary>
        <UsageType(UsageTypes.PhysicalCollection)> Flow = &H1E
        ''' <summary>FlowID</summary>
        <UsageType(UsageTypes.Item)> FlowID = &H1F
        ''' <summary>Outlet</summary>
        <UsageType(UsageTypes.PhysicalCollection)> Outlet = &H20
        ''' <summary>OutletID</summary>
        <UsageType(UsageTypes.StaticValue)> OutletID = &H21
        ''' <summary>Gang</summary>
        <UsageType(UsageTypes.LogicalCollection Or UsageTypes.PhysicalCollection)> Gang = &H22
        ''' <summary>GangID</summary>
        <UsageType(UsageTypes.StaticValue)> GangID = &H23
        ''' <summary>PowerSummary</summary>
        <UsageType(UsageTypes.LogicalCollection Or UsageTypes.PhysicalCollection)> PowerSummary = &H24
        ''' <summary>PowerSummaryID</summary>
        <UsageType(UsageTypes.StaticValue)> PowerSummaryID = &H25
        ''' <summary>Voltage</summary>
        <UsageType(UsageTypes.DynamicValue)> Voltage = &H30
        ''' <summary>Current</summary>
        <UsageType(UsageTypes.DynamicValue)> Current = &H31
        ''' <summary>Frequency</summary>
        <UsageType(UsageTypes.DynamicValue)> Frequency = &H32
        ''' <summary>ApparentPower</summary>
        <UsageType(UsageTypes.DynamicValue)> ApparentPower = &H33
        ''' <summary>ActivePower</summary>
        <UsageType(UsageTypes.DynamicValue)> ActivePower = &H34
        ''' <summary>PercentLoad</summary>
        <UsageType(UsageTypes.DynamicValue)> PercentLoad = &H35
        ''' <summary>Temperature</summary>
        <UsageType(UsageTypes.DynamicValue)> Temperature = &H36
        ''' <summary>Humidity</summary>
        <UsageType(UsageTypes.DynamicValue)> Humidity = &H37
        ''' <summary>BadCount</summary>
        <UsageType(UsageTypes.DynamicValue)> BadCount = &H38
        ''' <summary>ConfigVoltage</summary>
        <UsageType(UsageTypes.StaticValue Or UsageTypes.DynamicValue)> ConfigVoltage = &H40
        ''' <summary>ConfigCurrent</summary>
        <UsageType(UsageTypes.StaticValue Or UsageTypes.DynamicValue)> ConfigCurrent = &H41
        ''' <summary>ConfigFrequency</summary>
        <UsageType(UsageTypes.StaticValue Or UsageTypes.DynamicValue)> ConfigFrequency = &H42
        ''' <summary>ConfigApparentPower</summary>
        <UsageType(UsageTypes.StaticValue Or UsageTypes.DynamicValue)> ConfigApparentPower = &H43
        ''' <summary>ConfigActivePower</summary>
        <UsageType(UsageTypes.StaticValue Or UsageTypes.DynamicValue)> ConfigActivePower = &H44
        ''' <summary>ConfigPercentLoad</summary>
        <UsageType(UsageTypes.StaticValue Or UsageTypes.DynamicValue)> ConfigPercentLoad = &H45
        ''' <summary>ConfigTemperature</summary>
        <UsageType(UsageTypes.StaticValue Or UsageTypes.DynamicValue)> ConfigTemperature = &H46
        ''' <summary>ConfigHumidity</summary>
        <UsageType(UsageTypes.StaticValue Or UsageTypes.DynamicValue)> ConfigHumidity = &H47
        ''' <summary>SwitchOnControl</summary>
        <UsageType(UsageTypes.DynamicValue)> SwitchOnControl = &H50
        ''' <summary>SwitchOffControl</summary>
        <UsageType(UsageTypes.DynamicValue)> SwitchOffControl = &H51
        ''' <summary>ToggleControl</summary>
        <UsageType(UsageTypes.DynamicValue)> ToggleControl = &H52
        ''' <summary>LowVoltageTransfer</summary>
        <UsageType(UsageTypes.DynamicValue)> LowVoltageTransfer = &H53
        ''' <summary>HighVoltageTransfer</summary>
        <UsageType(UsageTypes.DynamicValue)> HighVoltageTransfer = &H54
        ''' <summary>DelayBeforeReboot</summary>
        <UsageType(UsageTypes.DynamicValue)> DelayBeforeReboot = &H55
        ''' <summary>DelayBeforeStartup</summary>
        <UsageType(UsageTypes.DynamicValue)> DelayBeforeStartup = &H56
        ''' <summary>DelayBeforeShutdown</summary>
        <UsageType(UsageTypes.DynamicValue)> DelayBeforeShutdown = &H57
        ''' <summary>Test</summary>
        <UsageType(UsageTypes.DynamicValue)> Test = &H58
        ''' <summary>ModuleReset</summary>
        <UsageType(UsageTypes.DynamicValue)> ModuleReset = &H59
        ''' <summary>AudibleAlarmControl</summary>
        <UsageType(UsageTypes.DynamicValue)> AudibleAlarmControl = &H5A
        ''' <summary>Present</summary>
        <UsageType(UsageTypes.DynamicFlag)> Present = &H60
        ''' <summary>Good</summary>
        <UsageType(UsageTypes.DynamicFlag)> Good = &H61
        ''' <summary>InternalFailure</summary>
        <UsageType(UsageTypes.DynamicFlag)> InternalFailure = &H62
        ''' <summary>VoltageOutOfRange</summary>
        <UsageType(UsageTypes.DynamicFlag)> VoltageOutOfRange = &H63
        ''' <summary>FrequencyOutOfRange</summary>
        <UsageType(UsageTypes.DynamicFlag)> FrequencyOutOfRange = &H64
        ''' <summary>Overload</summary>
        <UsageType(UsageTypes.DynamicFlag)> Overload = &H65
        ''' <summary>OverCharged</summary>
        <UsageType(UsageTypes.DynamicFlag)> OverCharged = &H66
        ''' <summary>OverTemperature</summary>
        <UsageType(UsageTypes.DynamicFlag)> OverTemperature = &H67
        ''' <summary>ShutdownRequested</summary>
        <UsageType(UsageTypes.DynamicFlag)> ShutdownRequested = &H68
        ''' <summary>ShutdownImminent</summary>
        <UsageType(UsageTypes.DynamicFlag)> ShutdownImminent = &H69
        ''' <summary>Reserved</summary>
        <UsageType(UsageTypes.DynamicFlag)> Reserved = &H6A
        ''' <summary>SwitchOn/Off</summary>
        <UsageType(UsageTypes.DynamicFlag)> SwitchOnOff = &H6B
        ''' <summary>Switchable</summary>
        <UsageType(UsageTypes.DynamicFlag)> Switchable = &H6C
        ''' <summary>Used</summary>
        <UsageType(UsageTypes.DynamicFlag)> Used = &H6D
        ''' <summary>Boost</summary>
        <UsageType(UsageTypes.DynamicFlag)> Boost = &H6E
        ''' <summary>Buck</summary>
        <UsageType(UsageTypes.DynamicFlag)> Buck = &H6F
        ''' <summary>Initialized</summary>
        <UsageType(UsageTypes.DynamicFlag)> Initialized = &H70
        ''' <summary>Tested</summary>
        <UsageType(UsageTypes.DynamicFlag)> Tested = &H71
        ''' <summary>AwaitingPower</summary>
        <UsageType(UsageTypes.DynamicFlag)> AwaitingPower = &H72
        ''' <summary>CommunicationLost</summary>
        <UsageType(UsageTypes.DynamicFlag)> CommunicationLost = &H73
        ''' <summary>iManufacturer</summary>
        <UsageType(UsageTypes.StaticValue)> iManufacturer = &HFD
        ''' <summary>iProduct</summary>
        <UsageType(UsageTypes.StaticValue)> iProduct = &HFE
        ''' <summary>iserialNumber</summary>
        <UsageType(UsageTypes.StaticValue)> iserialNumber = &HFF
    End Enum

    ''' <summary>Contains HID usage code s for <see cref="UsagePages.BatterySystem"/> page</summary>
    ''' <remarks>Values not defined in this enumeration are reserved</remarks>
    Public Enum Usages_BatterySystem
        ''' <summary>Undefined</summary>
        Undefined = 0
        ''' <summary>SMBBatteryMode</summary>
        <UsageType(UsageTypes.LogicalCollection)> SMBBatteryMode = &H1
        ''' <summary>SMBBatteryStatus</summary>
        <UsageType(UsageTypes.LogicalCollection)> SMBBatteryStatus = &H2
        ''' <summary>SMBAlarmWarning</summary>
        <UsageType(UsageTypes.LogicalCollection)> SMBAlarmWarning = &H3
        ''' <summary>SMBChargerMode</summary>
        <UsageType(UsageTypes.LogicalCollection)> SMBChargerMode = &H4
        ''' <summary>SMBChargerStatus</summary>
        <UsageType(UsageTypes.LogicalCollection)> SMBChargerStatus = &H5
        ''' <summary>SMBChargerSpecInfo</summary>
        <UsageType(UsageTypes.LogicalCollection)> SMBChargerSpecInfo = &H6
        ''' <summary>SMBSelectorState</summary>
        <UsageType(UsageTypes.LogicalCollection)> SMBSelectorState = &H7
        ''' <summary>SMBSelectorPresets</summary>
        <UsageType(UsageTypes.LogicalCollection)> SMBSelectorPresets = &H8
        ''' <summary>SMBSelectorInfo</summary>
        <UsageType(UsageTypes.LogicalCollection)> SMBSelectorInfo = &H9
        ''' <summary>OptionalMfgFunction1</summary>
        <UsageType(UsageTypes.DynamicValue)> OptionalMfgFunction1 = &H10
        ''' <summary>OptionalMfgFunction2</summary>
        <UsageType(UsageTypes.DynamicValue)> OptionalMfgFunction2 = &H11
        ''' <summary>OptionalMfgFunction3</summary>
        <UsageType(UsageTypes.DynamicValue)> OptionalMfgFunction3 = &H12
        ''' <summary>OptionalMfgFunction4</summary>
        <UsageType(UsageTypes.DynamicValue)> OptionalMfgFunction4 = &H13
        ''' <summary>OptionalMfgFunction5</summary>
        <UsageType(UsageTypes.DynamicValue)> OptionalMfgFunction5 = &H14
        ''' <summary>ConnectionToSMBus</summary>
        <UsageType(UsageTypes.DynamicFlag)> ConnectionToSMBus = &H15
        ''' <summary>OutputConnection</summary>
        <UsageType(UsageTypes.DynamicFlag)> OutputConnection = &H16
        ''' <summary>ChargerConnection</summary>
        <UsageType(UsageTypes.DynamicFlag)> ChargerConnection = &H17
        ''' <summary>BatteryInsertion</summary>
        <UsageType(UsageTypes.DynamicFlag)> BatteryInsertion = &H18
        ''' <summary>Usenext</summary>
        <UsageType(UsageTypes.DynamicFlag)> Usenext = &H19
        ''' <summary>OKToUse</summary>
        <UsageType(UsageTypes.DynamicFlag)> OKToUse = &H1A
        ''' <summary>BatterySupported</summary>
        <UsageType(UsageTypes.DynamicFlag)> BatterySupported = &H1B
        ''' <summary>SelectorRevision</summary>
        <UsageType(UsageTypes.DynamicFlag)> SelectorRevision = &H1C
        ''' <summary>ChargingIndicator</summary>
        <UsageType(UsageTypes.DynamicFlag)> ChargingIndicator = &H1D
        ''' <summary>ManufacturerAccess</summary>
        <UsageType(UsageTypes.DynamicValue)> ManufacturerAccess = &H28
        ''' <summary>RemainingCapacityLimit</summary>
        <UsageType(UsageTypes.DynamicValue)> RemainingCapacityLimit = &H29
        ''' <summary>RemainingTimeLimit</summary>
        <UsageType(UsageTypes.DynamicValue)> RemainingTimeLimit = &H2A
        ''' <summary>AtRate</summary>
        <UsageType(UsageTypes.DynamicValue)> AtRate = &H2B
        ''' <summary>CapacityMode</summary>
        <UsageType(UsageTypes.DynamicValue)> CapacityMode = &H2C
        ''' <summary>BroadcastToCharger</summary>
        <UsageType(UsageTypes.DynamicValue)> BroadcastToCharger = &H2D
        ''' <summary>PrimaryBattery</summary>
        <UsageType(UsageTypes.DynamicValue)> PrimaryBattery = &H2E
        ''' <summary>ChargeController</summary>
        <UsageType(UsageTypes.DynamicValue)> ChargeController = &H2F
        ''' <summary>TerminateCharge</summary>
        <UsageType(UsageTypes.DynamicFlag)> TerminateCharge = &H40
        ''' <summary>TerminateDischarge</summary>
        <UsageType(UsageTypes.DynamicFlag)> TerminateDischarge = &H41
        ''' <summary>BelowRemainingCapacityLimit</summary>
        <UsageType(UsageTypes.DynamicFlag)> BelowRemainingCapacityLimit = &H42
        ''' <summary>RemainingTimeLimitExpired</summary>
        <UsageType(UsageTypes.DynamicFlag)> RemainingTimeLimitExpired = &H43
        ''' <summary>Charging</summary>
        <UsageType(UsageTypes.DynamicFlag)> Charging = &H44
        ''' <summary>Discharging</summary>
        <UsageType(UsageTypes.DynamicValue)> Discharging = &H45
        ''' <summary>FullyCharged</summary>
        <UsageType(UsageTypes.DynamicFlag)> FullyCharged = &H46
        ''' <summary>FullyDischarged</summary>
        <UsageType(UsageTypes.DynamicValue)> FullyDischarged = &H47
        ''' <summary>ConditioningFlag</summary>
        <UsageType(UsageTypes.DynamicValue)> ConditioningFlag = &H48
        ''' <summary>AtRateOK</summary>
        <UsageType(UsageTypes.DynamicValue)> AtRateOK = &H49
        ''' <summary>SMBErrorCode</summary>
        <UsageType(UsageTypes.DynamicFlag)> SMBErrorCode = &H4A
        ''' <summary>NeedReplacement</summary>
        <UsageType(UsageTypes.DynamicFlag)> NeedReplacement = &H4B
        ''' <summary>AtRateTimeToFull</summary>
        <UsageType(UsageTypes.DynamicValue)> AtRateTimeToFull = &H60
        ''' <summary>AtRateTimeToEmpty</summary>
        <UsageType(UsageTypes.DynamicValue)> AtRateTimeToEmpty = &H61
        ''' <summary>AverageCurrent</summary>
        <UsageType(UsageTypes.DynamicValue)> AverageCurrent = &H62
        ''' <summary>Maxerror</summary>
        <UsageType(UsageTypes.DynamicValue)> Maxerror = &H63
        ''' <summary>RelativeStateOfCharge</summary>
        <UsageType(UsageTypes.DynamicValue)> RelativeStateOfCharge = &H64
        ''' <summary>AbsoluteStateOfCharge</summary>
        <UsageType(UsageTypes.DynamicValue)> AbsoluteStateOfCharge = &H65
        ''' <summary>RemainingCapacity</summary>
        <UsageType(UsageTypes.DynamicValue)> RemainingCapacity = &H66
        ''' <summary>FullChargeCapacity</summary>
        <UsageType(UsageTypes.DynamicValue)> FullChargeCapacity = &H67
        ''' <summary>RunTimeToEmpty</summary>
        <UsageType(UsageTypes.DynamicValue)> RunTimeToEmpty = &H68
        ''' <summary>AverageTimeToEmpty</summary>
        <UsageType(UsageTypes.DynamicValue)> AverageTimeToEmpty = &H69
        ''' <summary>AverageTimeToFull</summary>
        <UsageType(UsageTypes.DynamicValue)> AverageTimeToFull = &H6A
        ''' <summary>CycleCount</summary>
        <UsageType(UsageTypes.DynamicValue)> CycleCount = &H6B
        ''' <summary>BattPackModelLevel</summary>
        <UsageType(UsageTypes.StaticValue)> BattPackModelLevel = &H80
        ''' <summary>InternalChargeController</summary>
        <UsageType(UsageTypes.StaticFlag)> InternalChargeController = &H81
        ''' <summary>PrimaryBatterySupport</summary>
        <UsageType(UsageTypes.StaticFlag)> PrimaryBatterySupport = &H82
        ''' <summary>DesignCapacity</summary>
        <UsageType(UsageTypes.StaticValue)> DesignCapacity = &H83
        ''' <summary>SpecificationInfo</summary>
        <UsageType(UsageTypes.StaticValue)> SpecificationInfo = &H84
        ''' <summary>ManufacturerDate</summary>
        <UsageType(UsageTypes.StaticValue)> ManufacturerDate = &H85
        ''' <summary>SerialNumber</summary>
        <UsageType(UsageTypes.StaticValue)> SerialNumber = &H86
        ''' <summary>iManufacturerName</summary>
        <UsageType(UsageTypes.StaticValue)> iManufacturerName = &H87
        ''' <summary>iDevicename</summary>
        <UsageType(UsageTypes.StaticValue)> iDevicename = &H88
        ''' <summary>iDeviceChemistery</summary>
        <UsageType(UsageTypes.StaticValue)> iDeviceChemistery = &H89
        ''' <summary>ManufacturerData</summary>
        <UsageType(UsageTypes.StaticValue)> ManufacturerData = &H8A
        ''' <summary>Rechargable</summary>
        <UsageType(UsageTypes.StaticValue)> Rechargable = &H8B
        ''' <summary>WarningCapacityLimit</summary>
        <UsageType(UsageTypes.StaticValue)> WarningCapacityLimit = &H8C
        ''' <summary>CapacityGranularity1</summary>
        <UsageType(UsageTypes.StaticValue)> CapacityGranularity1 = &H8D
        ''' <summary>CapacityGranularity2</summary>
        <UsageType(UsageTypes.StaticValue)> CapacityGranularity2 = &H8E
        ''' <summary>iOEMInformation</summary>
        <UsageType(UsageTypes.StaticValue)> iOEMInformation = &H8F
        ''' <summary>InhibitCharge</summary>
        <UsageType(UsageTypes.DynamicFlag)> InhibitCharge = &HC0
        ''' <summary>EnablePolling</summary>
        <UsageType(UsageTypes.DynamicFlag)> EnablePolling = &HC1
        ''' <summary>ResetToZero</summary>
        <UsageType(UsageTypes.DynamicFlag)> ResetToZero = &HC2
        ''' <summary>ACPresent</summary>
        <UsageType(UsageTypes.DynamicFlag)> ACPresent = &HD0
        ''' <summary>BatteryPresent</summary>
        <UsageType(UsageTypes.DynamicFlag)> BatteryPresent = &HD1
        ''' <summary>PowerFail</summary>
        <UsageType(UsageTypes.DynamicFlag)> PowerFail = &HD2
        ''' <summary>AlarmInhibited</summary>
        <UsageType(UsageTypes.DynamicFlag)> AlarmInhibited = &HD3
        ''' <summary>ThermistorUnderRange</summary>
        <UsageType(UsageTypes.DynamicFlag)> ThermistorUnderRange = &HD4
        ''' <summary>ThermistorHot</summary>
        <UsageType(UsageTypes.DynamicFlag)> ThermistorHot = &HD5
        ''' <summary>ThermistorCold</summary>
        <UsageType(UsageTypes.DynamicFlag)> ThermistorCold = &HD6
        ''' <summary>ThermistorOverRange</summary>
        <UsageType(UsageTypes.DynamicFlag)> ThermistorOverRange = &HD7
        ''' <summary>VoltageOutOfRange</summary>
        <UsageType(UsageTypes.DynamicFlag)> VoltageOutOfRange = &HD8
        ''' <summary>CurrentOutOfRange</summary>
        <UsageType(UsageTypes.DynamicFlag)> CurrentOutOfRange = &HD9
        ''' <summary>CurrentNotRegulated</summary>
        <UsageType(UsageTypes.DynamicFlag)> CurrentNotRegulated = &HDA
        ''' <summary>VoltageNotRegulated</summary>
        <UsageType(UsageTypes.DynamicFlag)> VoltageNotRegulated = &HDB
        ''' <summary>MasterMode</summary>
        <UsageType(UsageTypes.DynamicFlag)> MasterMode = &HDC
        ''' <summary>ChargerSelectorSupport</summary>
        <UsageType(UsageTypes.StaticFlag)> ChargerSelectorSupport = &HF0
        ''' <summary>ChargerSpec</summary>
        <UsageType(UsageTypes.StaticValue)> ChargerSpec = &HF1
        ''' <summary>Level2</summary>
        <UsageType(UsageTypes.StaticFlag)> Level2 = &HF2
        ''' <summary>Level3</summary>
        <UsageType(UsageTypes.StaticFlag)> Level3 = &HF3
    End Enum
    ''' <summary>Contains HID usage code s for <see cref="UsagePages.UsbMonitor"/> page</summary>
    ''' <remarks>Values not defined in this enumeration are reserved
    ''' <para>Documentation <a href="http://www.usb.org/developers/devclass_docs/usbmon10.pdf">USB Monitor Control Class Specification</a> doe not provide information about usage types for this usage page, so that information is not included in this enumeration.</para></remarks>
    Public Enum Usages_UsbMonitor
        ''' <summary>Monitor Control</summary>
        ''' <remarks>USB Monitor Control HID Device.</remarks>
        MonitorControl = 1
        ''' <summary>EDID Information</summary>
        EdidInformation = 2
        ''' <summary>VDIF Information</summary>
        VdifInformation = 3
        ''' <summary>VESA Version</summary>
        ''' <remarks><para>The version of the VESA Monitor Command Set specification used by this device.</para>
        ''' <para>If this field is set to zero (0), the monitor uses the virtual control usage values defined in this document.</para>
        ''' <para>If this field is non-zero, it is a Binary-Coded Decimal (BCD) value representing the version number of the VESA Monitor Command Set specification used to define the monitor’s virtual control and command usage values.</para></remarks>
        VesaVersion = 4
    End Enum
    ''' <summary>Contains HID usage code s for <see cref="UsagePages.UsbEnumeratedValues"/> page</summary>
    ''' <remarks>This enumeration allows use of values up to 65535
    ''' <para>Documentation <a href="http://www.usb.org/developers/devclass_docs/usbmon10.pdf">USB Monitor Control Class Specification</a> doe not provide information about usage types for this usage page, so that information is not included in this enumeration.</para></remarks>
    <ValuesFromRangeAreValid(0, 65535)> _
    Public Enum Usages_UsbEnumeratedValues
        ''' <summary>Enum 0</summary>
        Enum_0 = 0
        ''' <summary>Enum 1</summary>
        Enum_1 = 1
        ''' <summary>Enum 2</summary>
        Enum_2 = 2
        ''' <summary>Enum 3</summary>
        Enum_3 = 3
    End Enum
    ''' <summary>Contains HID usage code s for <see cref="UsagePages.VesaVirtualControls"/> page</summary>
    ''' <remarks>Documentation <a href="http://www.usb.org/developers/devclass_docs/usbmon10.pdf">USB Monitor Control Class Specification</a> doe not provide information about usage types for this usage page, so that information is not included in this enumeration.</remarks>
    Public Enum Usages_VesaVirtualControls
        ''' <summary>Brightness</summary>
        Brightness = &H10
        ''' <summary>Contrast</summary>
        Contrast = &H12
        ''' <summary>Red Video Gain</summary>
        RedVideoGain = &H16
        ''' <summary>Green Video Gain</summary>
        GreenVideoGain = &H18
        ''' <summary>Blue Video Gain</summary>
        BlueVideoGain = &H1A
        ''' <summary>Focus</summary>
        Focus = &H1C
        ''' <summary>Horizontal Position</summary>
        HorizontalPosition = &H20
        ''' <summary>Horizontal Size</summary>
        HorizontalSize = &H22
        ''' <summary>Horizontal Pincushion</summary>
        HorizontalPincushion = &H24
        ''' <summary>Horizontal Pincushion Balance</summary>
        HorizontalPincushionBalance = &H26
        ''' <summary>Horizontal Misconvergence</summary>
        HorizontalMisconvergence = &H28
        ''' <summary>Horizontal Linearity</summary>
        HorizontalLinearity = &H2A
        ''' <summary>Horizontal Linearity Balance</summary>
        HorizontalLinearityBalance = &H2C
        ''' <summary>Vertical Position</summary>
        VerticalPosition = &H30
        ''' <summary>Vertical Size</summary>
        VerticalSize = &H32
        ''' <summary>Vertical Pincushion</summary>
        VerticalPincushion = &H34
        ''' <summary>Vertical Pincushion Balance</summary>
        VerticalPincushionBalance = &H36
        ''' <summary>Vertical Misconvergence</summary>
        VerticalMisconvergence = &H38
        ''' <summary>Vertical Linearity</summary>
        VerticalLinearity = &H3A
        ''' <summary>Vertical Linearity Balance</summary>
        VerticalLinearityBalance = &H3C
        ''' <summary>Parallelogram Distortion (Key Balance)</summary>
        ParallelgramDistortion = &H40
        ''' <summary>Trapezoidal Distortion (Key)</summary>
        TrapezoidalDistortion = &H42
        ''' <summary>Tilt (Rotation)</summary>
        Tilt = &H44
        ''' <summary>Top Corner Distortion Control</summary>
        TopCornerDistortionControl = &H46
        ''' <summary>Top Corner Distortion Balance</summary>
        TopCornerDistortionBalance = &H48
        ''' <summary>Bottom Corner Distortion Control</summary>
        BottomCornerDistortionControl = &H4A
        ''' <summary>Bottom Corner Distortion Balance</summary>
        BottomCornerDistortionBalance = &H4C
        ''' <summary>Horizontal Moiré</summary>
        HorizontalMoiré = &H56
        ''' <summary>Vertical Moiré</summary>
        VerticalMoiré = &H58
        ''' <summary>Red Video Black Level</summary>
        RedVideoBlackLevel = &H6C
        ''' <summary>Green Video Black Level</summary>
        GreenVideoBlackLevel = &H6E
        ''' <summary>Blue Video Black Level</summary>
        BlueVideoBlackLevel = &H70
        ''' <summary>Input Level Select</summary>
        InputLevelSelect = &H5E
        ''' <summary>Input Source Select</summary>
        InputSourceSelect = &H60
        ''' <summary>On Screen Display</summary>
        OnScreenDisplay = &HCA
        ''' <summary>StereoMode</summary>
        StereoMode = &HD4
        ''' <summary>Auto Size Center</summary>
        AutoSizeCenter = &HA2
        ''' <summary>Polarity Horizontal</summary>
        PolarityHorizontal = &HA4
        Synchronization
        ''' <summary>Polarity Vertical</summary>
        PolarityVertical = &HA6
        ''' <summary>Synchronization Type</summary>
        SynchronizationType = &HA8
        ''' <summary>Screen Orientation</summary>
        ScreenOrientation = &HAA
        ''' <summary>Horizontal Frequency</summary>
        HorizontalFrequency = &HAC
        ''' <summary>Vertical Frequency</summary>
        VerticalFrequency = &HAE
        ''' <summary>Degauss</summary>
        Degauss = &H1
        ''' <summary>Settings</summary>
        Settings = &HB0
    End Enum

    ''' <summary>Contains HID usage code s for <see cref="UsagePages.BarCodeScanners"/> page</summary>
    ''' <remarks>Values not defined in this enumeration are reserved</remarks>
    Public Enum Usages_BarCodeScanners
        ''' <summary>Undefined</summary>
        Undefined = 0
        ''' <summary>Bar Code Badge Reader</summary>
        <UsageType(UsageTypes.ApplicationCollection)> BarCodeBadgeReader = &H1
        ''' <summary>Bar Code Scanner</summary>
        <UsageType(UsageTypes.ApplicationCollection)> BarCodeScanner = &H2
        ''' <summary>Dumb Bar Code Scanner</summary>
        <UsageType(UsageTypes.ApplicationCollection)> DumbBarCodeScanner = &H3
        ''' <summary>Cordless Scanner Base</summary>
        <UsageType(UsageTypes.ApplicationCollection)> CordlessScannerBase = &H4
        ''' <summary>Bar Code Scanner Cradle</summary>
        <UsageType(UsageTypes.ApplicationCollection)> BarCodeScannerCradle = &H5
        ''' <summary>Attribute Report</summary>
        <UsageType(UsageTypes.LogicalCollection)> AttributeReport = &H10
        ''' <summary>Settings Report</summary>
        <UsageType(UsageTypes.LogicalCollection)> SettingsReport = &H11
        ''' <summary>Scanned Data Report</summary>
        <UsageType(UsageTypes.LogicalCollection)> cannedDataReport = &H12
        ''' <summary>Raw Scanned Data Report</summary>
        <UsageType(UsageTypes.LogicalCollection)> RawScannedDataReport = &H13
        ''' <summary>Trigger Report</summary>
        <UsageType(UsageTypes.LogicalCollection)> TriggerReport = &H14
        ''' <summary>Status Report</summary>
        <UsageType(UsageTypes.LogicalCollection)> tatusReport = &H15
        ''' <summary>UPC/EAN Control Report</summary>
        <UsageType(UsageTypes.LogicalCollection)> UPpcEanControlReport = &H16
        ''' <summary>EAN 2/3 Label Control Report</summary>
        <UsageType(UsageTypes.LogicalCollection)> EAN23LabelControlReport = &H17
        ''' <summary>Code 39 Control Report</summary>
        <UsageType(UsageTypes.LogicalCollection)> Code39ControlReport = &H18
        ''' <summary>Interleaved 2 of 5 Control Report</summary>
        <UsageType(UsageTypes.LogicalCollection)> Interleaved2Of5ControlReport = &H19
        ''' <summary>Standard 2 of 5 Control Report</summary>
        <UsageType(UsageTypes.LogicalCollection)> tandard2of5ControlReport = &H1A
        ''' <summary>MSI Plessey Control Report</summary>
        <UsageType(UsageTypes.LogicalCollection)> MSIPlesseyControlReport = &H1B
        ''' <summary>Codabar Control Report</summary>
        <UsageType(UsageTypes.LogicalCollection)> CodabarControlReport = &H1C
        ''' <summary>Code 128 Control Report</summary>
        <UsageType(UsageTypes.LogicalCollection)> Code128ControlReport = &H1D
        ''' <summary>Misc 1D Control Report</summary>
        <UsageType(UsageTypes.LogicalCollection)> Misc1DControlReport = &H1E
        ''' <summary>2D Control Report</summary>
        <UsageType(UsageTypes.LogicalCollection)> ControlReport2D = &H1F
        ''' <summary>Aiming/Pointer Mode</summary>
        <UsageType(UsageTypes.StaticFlag)> AimingPointerMode = &H30
        ''' <summary>Bar Code Present Sensor</summary>
        <UsageType(UsageTypes.StaticFlag)> BarCodePresentSensor = &H31
        ''' <summary>Class 1A Laser</summary>
        <UsageType(UsageTypes.StaticFlag)> Class1ALaser = &H32
        ''' <summary>Class 2 Laser</summary>
        <UsageType(UsageTypes.StaticFlag)> Class2Laser = &H33
        ''' <summary>Heater Present</summary>
        <UsageType(UsageTypes.StaticFlag)> HeaterPresent = &H34
        ''' <summary>Contact Scanner</summary>
        <UsageType(UsageTypes.StaticFlag)> ContactScanner = &H35
        ''' <summary>Electronic Article Surveillance Notification</summary>
        <UsageType(UsageTypes.StaticFlag)> ElectronicArticleSurveillanceNotification = &H36
        ''' <summary>Constant Electronic Article Surveillance</summary>
        <UsageType(UsageTypes.StaticFlag)> ConstantElectronicArticleSurveillance = &H37
        ''' <summary>Error Indication</summary>
        <UsageType(UsageTypes.StaticFlag)> ErrorIndication = &H38
        ''' <summary>Fixed Beeper</summary>
        <UsageType(UsageTypes.StaticFlag)> FixedBeeper = &H39
        ''' <summary>Good Decode Indication</summary>
        <UsageType(UsageTypes.StaticFlag)> GoodDecodeIndication = &H3A
        ''' <summary>Hands Free Scanning</summary>
        <UsageType(UsageTypes.StaticFlag)> HandsFreeScanning = &H3B
        ''' <summary>Intrinsically Safe</summary>
        <UsageType(UsageTypes.StaticFlag)> IntrinsicallySafe = &H3C
        ''' <summary>Klasse Eins Laser</summary>
        <UsageType(UsageTypes.StaticFlag)> KlasseEinsLaser = &H3D
        ''' <summary>Long Range Scanner</summary>
        <UsageType(UsageTypes.StaticFlag)> LongRangeScanner = &H3E
        ''' <summary>Mirror Speed Control</summary>
        <UsageType(UsageTypes.StaticFlag)> MirrorSpeedControl = &H3F
        ''' <summary>Not On File Indication</summary>
        <UsageType(UsageTypes.StaticFlag)> NotOnFileIndication = &H40
        ''' <summary>Programmable Beeper</summary>
        <UsageType(UsageTypes.StaticFlag)> ProgrammableBeeper = &H41
        ''' <summary>Triggerless</summary>
        <UsageType(UsageTypes.StaticFlag)> Triggerless = &H42
        ''' <summary>Wand</summary>
        <UsageType(UsageTypes.StaticFlag)> Wand = &H43
        ''' <summary>Water Resistant</summary>
        <UsageType(UsageTypes.StaticFlag)> WaterResistant = &H44
        ''' <summary>Multi-Range Scanner</summary>
        <UsageType(UsageTypes.StaticFlag)> MultiRangeScanner = &H45
        ''' <summary>Proximity Sensor</summary>
        <UsageType(UsageTypes.StaticFlag)> ProximitySensor = &H46
        ''' <summary>Fragment Decoding</summary>
        <UsageType(UsageTypes.DynamicFlag)> FragmentDecoding = &H4D
        ''' <summary>Scanner Read Confidence</summary>
        <UsageType(UsageTypes.DynamicValue)> ScannerReadConfidence = &H4E
        ''' <summary>Data Prefix</summary>
        <UsageType(UsageTypes.NamedArray)> DataPrefix = &H4F
        ''' <summary>Prefix AIMI</summary>
        <UsageType(UsageTypes.Selector)> PrefixAIMI = &H50
        ''' <summary>Prefix None</summary>
        <UsageType(UsageTypes.Selector)> PrefixNone = &H51
        ''' <summary>Prefix Proprietary</summary>
        <UsageType(UsageTypes.Selector)> PrefixProprietary = &H52
        ''' <summary>Active Time</summary>
        <UsageType(UsageTypes.DynamicValue)> ActiveTime = &H55
        ''' <summary>Aiming Laser Pattern</summary>
        <UsageType(UsageTypes.DynamicFlag)> AimingLaserPattern = &H56
        ''' <summary>Bar Code Present</summary>
        <UsageType(UsageTypes.OnOffControl)> BarCodePresent = &H57
        ''' <summary>Beeper State</summary>
        <UsageType(UsageTypes.OnOffControl)> BeeperState = &H58
        ''' <summary>Laser On Time</summary>
        <UsageType(UsageTypes.DynamicValue)> LaserOnTime = &H59
        ''' <summary>Laser State</summary>
        <UsageType(UsageTypes.OnOffControl)> LaserState = &H5A
        ''' <summary>Lockout Time</summary>
        <UsageType(UsageTypes.DynamicValue)> LockoutTime = &H5B
        ''' <summary>Motor State</summary>
        <UsageType(UsageTypes.OnOffControl)> MotorState = &H5C
        ''' <summary>Motor Timeout</summary>
        <UsageType(UsageTypes.DynamicValue)> MotorTimeout = &H5D
        ''' <summary>Power On Reset Scanner</summary>
        <UsageType(UsageTypes.DynamicFlag)> PowerOnResetScanner = &H5E
        ''' <summary>Prevent Read of Barcodes</summary>
        <UsageType(UsageTypes.DynamicFlag)> PreventReadofBarcodes = &H5F
        ''' <summary>Initiate Barcode Read</summary>
        <UsageType(UsageTypes.DynamicFlag)> InitiateBarcodeRead = &H60
        ''' <summary>Trigger State</summary>
        <UsageType(UsageTypes.OnOffControl)> TriggerState = &H61
        ''' <summary>Trigger Mode</summary>
        <UsageType(UsageTypes.NamedArray)> TriggerMode = &H62
        ''' <summary>Trigger Mode Blinking Laser On</summary>
        <UsageType(UsageTypes.Selector)> TriggerModeBlinkingLaserOn = &H63
        ''' <summary>Trigger Mode Continuous Laser On</summary>
        <UsageType(UsageTypes.Selector)> TriggerModeContinuousLaserOn = &H64
        ''' <summary>Trigger Mode Laser on while Pulled</summary>
        <UsageType(UsageTypes.Selector)> TriggerModeLaseronwhilePulled = &H65
        ''' <summary>Trigger Mode Laser stays on after Trigger release</summary>
        <UsageType(UsageTypes.Selector)> TriggerModeLaserStaysOnaAfterTriggerRelease = &H66
        ''' <summary>Commit Parameters to NVM</summary>
        <UsageType(UsageTypes.DynamicFlag)> CommitParameterstoNVM = &H6D
        ''' <summary>Parameter Scanning</summary>
        <UsageType(UsageTypes.DynamicFlag)> ParameterScanning = &H6E
        ''' <summary>Parameters Changed</summary>
        <UsageType(UsageTypes.OnOffControl)> ParametersChanged = &H6F
        ''' <summary>Set parameter default values</summary>
        <UsageType(UsageTypes.DynamicFlag)> etparameterdefaultvalues = &H70
        ''' <summary>Scanner In Cradle</summary>
        <UsageType(UsageTypes.OnOffControl)> cannerInCradle = &H75
        ''' <summary>Scanner In Range</summary>
        <UsageType(UsageTypes.OnOffControl)> cannerInRange = &H76
        ''' <summary>Aim Duration</summary>
        <UsageType(UsageTypes.DynamicValue)> AimDuration = &H7A
        ''' <summary>Good Read Lamp Duration</summary>
        <UsageType(UsageTypes.DynamicValue)> GoodReadLampDuration = &H7B
        ''' <summary>Good Read Lamp Intensity</summary>
        <UsageType(UsageTypes.DynamicValue)> GoodReadLampIntensity = &H7C
        ''' <summary>Good Read LED</summary>
        <UsageType(UsageTypes.DynamicFlag)> GoodReadLED = &H7D
        ''' <summary>Good Read Tone Frequency</summary>
        <UsageType(UsageTypes.DynamicValue)> GoodReadToneFrequency = &H7E
        ''' <summary>Good Read Tone Length</summary>
        <UsageType(UsageTypes.DynamicValue)> GoodReadToneLength = &H7F
        ''' <summary>Good Read Tone Volume</summary>
        <UsageType(UsageTypes.DynamicValue)> GoodReadToneVolume = &H80
        ''' <summary>Reserved</summary>
        <UsageType(UsageTypes.DynamicValue)> Reserved = &H81
        ''' <summary>No Read Message</summary>
        <UsageType(UsageTypes.DynamicFlag)> NoReadMessage = &H82
        ''' <summary>Not on File Volume</summary>
        <UsageType(UsageTypes.DynamicValue)> NotonFileVolume = &H83
        ''' <summary>Powerup Beep</summary>
        <UsageType(UsageTypes.DynamicFlag)> PowerupBeep = &H84
        ''' <summary>Sound Error Beep</summary>
        <UsageType(UsageTypes.DynamicFlag)> oundErrorBeep = &H85
        ''' <summary>Sound Good Read Beep</summary>
        <UsageType(UsageTypes.DynamicFlag)> oundGoodReadBeep = &H86
        ''' <summary>Sound Not On File Beep</summary>
        <UsageType(UsageTypes.DynamicFlag)> oundNotOnFileBeep = &H87
        ''' <summary>Good Read When to Write</summary>
        <UsageType(UsageTypes.NamedArray)> GoodReadWhentoWrite = &H88
        ''' <summary>GRWTI After Decode</summary>
        <UsageType(UsageTypes.Selector)> GRWTIAfterDecode = &H89
        ''' <summary>GRWTI Beep/Lamp after transmit</summary>
        <UsageType(UsageTypes.Selector)> GRWTIBeepLampAfterTransmit = &H8A
        ''' <summary>GRWTI No Beep/Lamp use at all</summary>
        <UsageType(UsageTypes.Selector)> GRWTINoBeepLampUseAtAll = &H8B
        ''' <summary>Bookland EAN</summary>
        <UsageType(UsageTypes.DynamicFlag)> BooklandEAN = &H91
        ''' <summary>Convert EAN 8 to 13 Type</summary>
        <UsageType(UsageTypes.DynamicFlag)> ConvertEAN8to13Type = &H92
        ''' <summary>Convert UPC A to EAN-13</summary>
        <UsageType(UsageTypes.DynamicFlag)> ConvertUPCAtoEAN13 = &H93
        ''' <summary>Convert UPC-E to A</summary>
        <UsageType(UsageTypes.DynamicFlag)> ConvertUPCEToA = &H94
        ''' <summary>EAN-13</summary>
        <UsageType(UsageTypes.DynamicFlag)> EAN13 = &H95
        ''' <summary>EAN-8</summary>
        <UsageType(UsageTypes.DynamicFlag)> EAN8 = &H96
        ''' <summary>EAN-99 128_Mandatory</summary>
        <UsageType(UsageTypes.DynamicFlag)> EAN99128_Mandatory = &H97
        ''' <summary>EAN-99 P5/128_Optional</summary>
        <UsageType(UsageTypes.DynamicFlag)> EAN99P5128_Optional = &H98
        ''' <summary>UPC/EAN</summary>
        <UsageType(UsageTypes.DynamicFlag)> UPCEAN = &H9A
        ''' <summary>UPC/EAN Coupon Code</summary>
        <UsageType(UsageTypes.DynamicFlag)> UPCEANCouponCode = &H9B
        ''' <summary>UPC/EAN Periodicals</summary>
        <UsageType(UsageTypes.DynamicValue)> UPCEANPeriodicals = &H9C
        ''' <summary>UPC-A</summary>
        <UsageType(UsageTypes.DynamicFlag)> UPCA = &H9D
        ''' <summary>UPC-A with 128 Mandatory</summary>
        <UsageType(UsageTypes.DynamicFlag)> UPCAWith128Mandatory = &H9E
        ''' <summary>UPC-A with 128 Optional</summary>
        <UsageType(UsageTypes.DynamicFlag)> UPCAWth128Optional = &H9F
        ''' <summary>UPC-A with P5 Optional</summary>
        <UsageType(UsageTypes.DynamicFlag)> UPCAWithP5Optional = &HA0
        ''' <summary>UPC-E</summary>
        <UsageType(UsageTypes.DynamicFlag)> UPCE = &HA1
        ''' <summary>UPC-E1</summary>
        <UsageType(UsageTypes.DynamicFlag)> UPCE1 = &HA2
        ''' <summary>Periodical</summary>
        <UsageType(UsageTypes.NamedArray)> Periodical = &HA9
        ''' <summary>Periodical Auto-Discriminate + 2</summary>
        <UsageType(UsageTypes.Selector)> PeriodicalAutoDiscriminate2 = &HAA
        ''' <summary>Periodical Only Decode with + 2</summary>
        <UsageType(UsageTypes.Selector)> PeriodicalOnlyDecodewith2 = &HAB
        ''' <summary>Periodical Ignore + 2</summary>
        <UsageType(UsageTypes.Selector)> PeriodicalIgnore2 = &HAC
        ''' <summary>Periodical Auto-Discriminate + 5</summary>
        <UsageType(UsageTypes.Selector)> PeriodicalAutoDiscriminate5 = &HAD
        ''' <summary>Periodical Only Decode with + 5</summary>
        <UsageType(UsageTypes.Selector)> PeriodicalOnlyDecodewith5 = &HAE
        ''' <summary>Periodical Ignore + 5</summary>
        <UsageType(UsageTypes.Selector)> PeriodicalIgnore5 = &HAF
        ''' <summary>Check</summary>
        <UsageType(UsageTypes.NamedArray)> Check = &HB0
        ''' <summary>Check Disable Price</summary>
        <UsageType(UsageTypes.Selector)> CheckDisablePrice = &HB1
        ''' <summary>Check Enable 4 digit Price</summary>
        <UsageType(UsageTypes.Selector)> CheckEnable4digitPrice = &HB2
        ''' <summary>Check Enable 5 digit Price</summary>
        <UsageType(UsageTypes.Selector)> CheckEnable5digitPrice = &HB3
        ''' <summary>Check Enable European 4 digit Price</summary>
        <UsageType(UsageTypes.Selector)> CheckEnableEuropean4digitPrice = &HB4
        ''' <summary>Check Enable European 5 digit Price</summary>
        <UsageType(UsageTypes.Selector)> CheckEnableEuropean5digitPrice = &HB5
        ''' <summary>EAN Two Label</summary>
        <UsageType(UsageTypes.DynamicFlag)> EANTwoLabel = &HB7
        ''' <summary>EAN Three Label</summary>
        <UsageType(UsageTypes.DynamicFlag)> EANThreeLabel = &HB8
        ''' <summary>EAN 8 Flag Digit 1</summary>
        <UsageType(UsageTypes.DynamicValue)> EAN8FlagDigit1 = &HB9
        ''' <summary>EAN 8 Flag Digit 2</summary>
        <UsageType(UsageTypes.DynamicValue)> EAN8FlagDigit2 = &HBA
        ''' <summary>EAN 8 Flag Digit 3</summary>
        <UsageType(UsageTypes.DynamicValue)> EAN8FlagDigit3 = &HBB
        ''' <summary>EAN 13 Flag Digit 1</summary>
        <UsageType(UsageTypes.DynamicValue)> EAN13FlagDigit1 = &HBC
        ''' <summary>EAN 13 Flag Digit 2</summary>
        <UsageType(UsageTypes.DynamicValue)> EAN13FlagDigit2 = &HBD
        ''' <summary>EAN 13 Flag Digit 3</summary>
        <UsageType(UsageTypes.DynamicValue)> EAN13FlagDigit3 = &HBE
        ''' <summary>Add EAN 2/3 Label Definition</summary>
        <UsageType(UsageTypes.DynamicFlag)> AddEAN23LabelDefinition = &HBF
        ''' <summary>Clear all EAN 2/3 Label Definitions</summary>
        <UsageType(UsageTypes.DynamicFlag)> ClearAllEAN23LabelDefinitions = &HC0
        ''' <summary>Codabar</summary>
        <UsageType(UsageTypes.DynamicFlag)> Codabar = &HC3
        ''' <summary>Code 128</summary>
        <UsageType(UsageTypes.DynamicFlag)> Code128 = &HC4
        ''' <summary>Code 39</summary>
        <UsageType(UsageTypes.DynamicFlag)> Code39 = &HC7
        ''' <summary>Code 93</summary>
        <UsageType(UsageTypes.DynamicFlag)> Code93 = &HC8
        ''' <summary>Full ASCII Conversion</summary>
        <UsageType(UsageTypes.DynamicFlag)> FullASCIIConversion = &HC9
        ''' <summary>Interleaved 2 of 5</summary>
        <UsageType(UsageTypes.DynamicFlag)> Interleaved2Of5 = &HCA
        ''' <summary>Italian Pharmacy Code</summary>
        <UsageType(UsageTypes.DynamicFlag)> ItalianPharmacyCode = &HCB
        ''' <summary>MSI/Plessey</summary>
        <UsageType(UsageTypes.DynamicFlag)> MSIPlessey = &HCC
        ''' <summary>Standard 2 of 5 IATA</summary>
        <UsageType(UsageTypes.DynamicFlag)> StandardOf5IATA = &HCD
        ''' <summary>Standard 2 of 5</summary>
        <UsageType(UsageTypes.DynamicFlag)> Standard2Of5 = &HCE
        ''' <summary>Transmit Start/Stop</summary>
        <UsageType(UsageTypes.DynamicFlag)> TransmitStartStop = &HD3
        ''' <summary>Tri-Optic</summary>
        <UsageType(UsageTypes.DynamicFlag)> TriOptic = &HD4
        ''' <summary>UCC/EAN-128</summary>
        <UsageType(UsageTypes.DynamicFlag)> UCCEAN128 = &HD5
        ''' <summary>Check Digit</summary>
        <UsageType(UsageTypes.NamedArray)> CheckDigit = &HD6
        ''' <summary>Check Digit Disable</summary>
        <UsageType(UsageTypes.Selector)> CheckDigitDisable = &HD7
        ''' <summary>Check Digit Enable Interleaved 2 of 5 OPCC</summary>
        <UsageType(UsageTypes.Selector)> CheckDigitEnableInterleaved2Of5OPCC = &HD8
        ''' <summary>Check Digit Enable Interleaved 2 of 5 USS</summary>
        <UsageType(UsageTypes.Selector)> CheckDigitEnableInterleaved2Of5USS = &HD9
        ''' <summary>Check Digit Enable Standard 2 of 5 OPCC</summary>
        <UsageType(UsageTypes.Selector)> CheckDigitEnableStandard2Of5OPCC = &HDA
        ''' <summary>Check Digit Enable Standard 2 of 5 USS</summary>
        <UsageType(UsageTypes.Selector)> CheckDigitEnableStandard2Of5USS = &HDB
        ''' <summary>Check Digit Enable One MSI Plessey</summary>
        <UsageType(UsageTypes.Selector)> CheckDigitEnableOneMSIPlessey = &HDC
        ''' <summary>Check Digit Enable Two MSI Plessey</summary>
        <UsageType(UsageTypes.Selector)> CheckDigitEnableTwoMSIPlessey = &HDD
        ''' <summary>Check Digit Codabar Enable</summary>
        <UsageType(UsageTypes.Selector)> CheckDigitCodabarEnable = &HDE
        ''' <summary>Check Digit Code 39 Enable</summary>
        <UsageType(UsageTypes.Selector)> CheckDigitCode39Enable = &HDF
        ''' <summary>Transmit Check Digit</summary>
        <UsageType(UsageTypes.NamedArray)> TransmitCheckDigit = &HF0
        ''' <summary>Disable Check Digit Transmit</summary>
        <UsageType(UsageTypes.Selector)> DisableCheckDigitTransmit = &HF1
        ''' <summary>Enable Check Digit Transmit</summary>
        <UsageType(UsageTypes.Selector)> EnableCheckDigitTransmit = &HF2
        ''' <summary>Symbology Identifier 1</summary>
        <UsageType(UsageTypes.DynamicValue)> ymbologyIdentifier1 = &HFB
        ''' <summary>Symbology Identifier 2</summary>
        <UsageType(UsageTypes.DynamicValue)> ymbologyIdentifier2 = &HFC
        ''' <summary>Symbology Identifier 3</summary>
        <UsageType(UsageTypes.DynamicValue)> ymbologyIdentifier3 = &HFD
        ''' <summary>Decoded Data</summary>
        <UsageType(UsageTypes.DynamicValue)> DecodedData = &HFE
        ''' <summary>Decode Data Continued</summary>
        <UsageType(UsageTypes.DynamicFlag)> DecodeDataContinued = &HFF
        ''' <summary>Bar Space Data</summary>
        <UsageType(UsageTypes.DynamicValue)> BarSpaceData = &H100
        ''' <summary>Scanner Data Accuracy</summary>
        <UsageType(UsageTypes.DynamicValue)> ScannerDataAccuracy = &H101
        ''' <summary>Raw Data Polarity</summary>
        <UsageType(UsageTypes.NamedArray)> RawDataPolarity = &H102
        ''' <summary>Polarity Inverted Bar Code</summary>
        <UsageType(UsageTypes.Selector)> PolarityInvertedBarCode = &H103
        ''' <summary>Polarity Normal Bar Code</summary>
        <UsageType(UsageTypes.Selector)> PolarityNormalBarCode = &H104
        ''' <summary>Minimum Length to Decode</summary>
        <UsageType(UsageTypes.DynamicValue)> MinimumLengthtoDecode = &H106
        ''' <summary>Maximum Length to Decode</summary>
        <UsageType(UsageTypes.DynamicValue)> MaximumLengthtoDecode = &H107
        ''' <summary>First Discrete Length to Decode</summary>
        <UsageType(UsageTypes.DynamicValue)> FirstDiscreteLengthtoDecode = &H108
        ''' <summary>Second Discrete Length to Decode</summary>
        <UsageType(UsageTypes.DynamicValue)> econdDiscreteLengthtoDecode = &H109
        ''' <summary>Data Length Method</summary>
        <UsageType(UsageTypes.NamedArray)> DataLengthMethod = &H10A
        ''' <summary>DL Method Read any</summary>
        <UsageType(UsageTypes.Selector)> DLMethodReadany = &H10B
        ''' <summary>DL Method Check in Range</summary>
        <UsageType(UsageTypes.Selector)> DLMethodCheckinRange = &H10C
        ''' <summary>DL Method Check for Discrete</summary>
        <UsageType(UsageTypes.Selector)> DLMethodCheckforDiscrete = &H10D
        ''' <summary>Aztec Code</summary>
        <UsageType(UsageTypes.DynamicFlag)> AztecCode = &H110
        ''' <summary>BC412</summary>
        <UsageType(UsageTypes.DynamicFlag)> BC412 = &H111
        ''' <summary>Channel Code</summary>
        <UsageType(UsageTypes.DynamicFlag)> ChannelCode = &H112
        ''' <summary>Code 16</summary>
        <UsageType(UsageTypes.DynamicFlag)> Code16 = &H113
        ''' <summary>Code 32</summary>
        <UsageType(UsageTypes.DynamicFlag)> Code32 = &H114
        ''' <summary>Code 49</summary>
        <UsageType(UsageTypes.DynamicFlag)> Code49 = &H115
        ''' <summary>Code One</summary>
        <UsageType(UsageTypes.DynamicFlag)> CodeOne = &H116
        ''' <summary>Colorcode</summary>
        <UsageType(UsageTypes.DynamicFlag)> Colorcode = &H117
        ''' <summary>Data Matrix</summary>
        <UsageType(UsageTypes.DynamicFlag)> DataMatrix = &H118
        ''' <summary>MaxiCode</summary>
        <UsageType(UsageTypes.DynamicFlag)> MaxiCode = &H119
        ''' <summary>MicroPDF</summary>
        <UsageType(UsageTypes.DynamicFlag)> MicroPDF = &H11A
        ''' <summary>PDF-417</summary>
        <UsageType(UsageTypes.DynamicFlag)> PDF417 = &H11B
        ''' <summary>PosiCode</summary>
        <UsageType(UsageTypes.DynamicFlag)> PosiCode = &H11C
        ''' <summary>QR Code</summary>
        <UsageType(UsageTypes.DynamicFlag)> QRCode = &H11D
        ''' <summary>SuperCode</summary>
        <UsageType(UsageTypes.DynamicFlag)> SuperCode = &H11E
        ''' <summary>UltraCode</summary>
        <UsageType(UsageTypes.DynamicFlag)> UltraCode = &H11F
        ''' <summary>USD-5 (Slug Code)</summary>
        <UsageType(UsageTypes.DynamicFlag)> USD5 = &H120
        ''' <summary>VeriCode</summary>
        <UsageType(UsageTypes.DynamicFlag)> VeriCode = &H121
    End Enum

    ''' <summary>Contains HID usage code s for <see cref="UsagePages.Scale"/> page</summary>
    ''' <remarks>Values not defined in this enumeration are reserved</remarks>
    Public Enum Usages_Scale
        ''' <summary>Undefined</summary>
        Undefined = 0
        ''' <summary>Weighing Device</summary>
        <UsageType(UsageTypes.ApplicationCollection)> WeighingDevice = &H1
        ''' <summary>Scale Device</summary>
        <UsageType(UsageTypes.LogicalCollection)> ScaleDevice = &H20
        ''' <summary>Scale Class I Metric</summary>
        <UsageType(UsageTypes.LogicalCollection)> ScaleClassIMetric_CL = &H21
        ''' <summary>Scale Class I Metric</summary>
        <UsageType(UsageTypes.Selector)> ScaleClassIMetric = &H22
        ''' <summary>Scale Class II Metric</summary>
        <UsageType(UsageTypes.Selector)> ScaleClassIIMetric = &H23
        ''' <summary>Scale Class III Metric</summary>
        <UsageType(UsageTypes.Selector)> ScaleClassIIIMetric = &H24
        ''' <summary>Scale Class IIIL Metric</summary>
        <UsageType(UsageTypes.Selector)> ScaleClassIIILMetric = &H25
        ''' <summary>Scale Class IV Metric</summary>
        <UsageType(UsageTypes.Selector)> ScaleClassIVMetric = &H26
        ''' <summary>Scale Class III English</summary>
        <UsageType(UsageTypes.Selector)> ScaleClassIIIEnglish = &H27
        ''' <summary>Scale Class IIIL English</summary>
        <UsageType(UsageTypes.Selector)> ScaleClassIIILEnglish = &H28
        ''' <summary>Scale Class IV English</summary>
        <UsageType(UsageTypes.Selector)> ScaleClassIVEnglish = &H29
        ''' <summary>Scale Class Generic</summary>
        <UsageType(UsageTypes.Selector)> ScaleClassGeneric = &H2A
        ''' <summary>Scale Attribute Report</summary>
        <UsageType(UsageTypes.LogicalCollection)> ScaleAttributeReport = &H30
        ''' <summary>Scale Control Report</summary>
        <UsageType(UsageTypes.LogicalCollection)> ScaleControlReport = &H31
        ''' <summary>Scale Data Report</summary>
        <UsageType(UsageTypes.LogicalCollection)> ScaleDataReport = &H32
        ''' <summary>Scale Status Report</summary>
        <UsageType(UsageTypes.LogicalCollection)> ScaleStatusReport = &H33
        ''' <summary>Scale Weight Limit Report</summary>
        <UsageType(UsageTypes.LogicalCollection)> ScaleWeightLimitReport = &H34
        ''' <summary>Scale Statistics Report</summary>
        <UsageType(UsageTypes.LogicalCollection)> ScaleStatisticsReport = &H35
        ''' <summary>Data Weight</summary>
        <UsageType(UsageTypes.DynamicValue)> DataWeight = &H40
        ''' <summary>Data Scaling</summary>
        <UsageType(UsageTypes.DynamicValue)> DataScaling = &H41
        ''' <summary>Weight Unit</summary>
        <UsageType(UsageTypes.LogicalCollection)> WeightUnit = &H50
        ''' <summary>Weight Unit Milligram</summary>
        <UsageType(UsageTypes.Selector)> WeightUnitMilligram = &H51
        ''' <summary>Weight Unit Gram</summary>
        <UsageType(UsageTypes.Selector)> WeightUnitGram = &H52
        ''' <summary>Weight Unit Kilogram</summary>
        <UsageType(UsageTypes.Selector)> WeightUnitKilogram = &H53
        ''' <summary>Weight Unit Carats</summary>
        <UsageType(UsageTypes.Selector)> WeightUnitCarats = &H54
        ''' <summary>Weight Unit Taels</summary>
        <UsageType(UsageTypes.Selector)> WeightUnitTaels = &H55
        ''' <summary>Weight Unit Grains</summary>
        <UsageType(UsageTypes.Selector)> WeightUnitGrains = &H56
        ''' <summary>Weight Unit Pennyweights</summary>
        <UsageType(UsageTypes.Selector)> WeightUnitPennyweights = &H57
        ''' <summary>Weight Unit Metric Ton</summary>
        <UsageType(UsageTypes.Selector)> WeightUnitMetricTon = &H58
        ''' <summary>Weight Unit Avoir Ton</summary>
        <UsageType(UsageTypes.Selector)> WeightUnitAvoirTon = &H59
        ''' <summary>Weight Unit Troy Ounce</summary>
        <UsageType(UsageTypes.Selector)> WeightUnitTroyOunce = &H5A
        ''' <summary>Weight Unit Ounce</summary>
        <UsageType(UsageTypes.Selector)> WeightUnitOunce = &H5B
        ''' <summary>Weight Unit Pound</summary>
        <UsageType(UsageTypes.Selector)> WeightUnitPound = &H5C
        ''' <summary>Calibration Count</summary>
        <UsageType(UsageTypes.DynamicValue)> CalibrationCount = &H60
        ''' <summary>Re-Zero Count</summary>
        <UsageType(UsageTypes.DynamicValue)> ReZeroCount = &H61
        ''' <summary>Scale Status</summary>
        <UsageType(UsageTypes.LogicalCollection)> caleStatus = &H70
        ''' <summary>Scale Status Fault</summary>
        <UsageType(UsageTypes.Selector)> caleStatusFault = &H71
        ''' <summary>Scale Status Stable at Center of Zero</summary>
        <UsageType(UsageTypes.Selector)> caleStatusStableAtCenterOfZero = &H72
        ''' <summary>Scale Status In Motion</summary>
        <UsageType(UsageTypes.Selector)> caleStatusInMotion = &H73
        ''' <summary>Scale Status Weight Stable</summary>
        <UsageType(UsageTypes.Selector)> caleStatusWeightStable = &H74
        ''' <summary>Scale Status Under Zero</summary>
        <UsageType(UsageTypes.Selector)> caleStatusUnderZero = &H75
        ''' <summary>Scale Status Over Weight Limit</summary>
        <UsageType(UsageTypes.Selector)> ScaleStatusOverWeightLimit = &H76
        ''' <summary>Scale Status Requires Calibration</summary>
        <UsageType(UsageTypes.Selector)> ScaleStatusRequiresCalibration = &H77
        ''' <summary>Scale Status Requires Rezeroing</summary>
        <UsageType(UsageTypes.Selector)> ScaleStatusRequiresRezeroing = &H78
        ''' <summary>Zero Scale</summary>
        <UsageType(UsageTypes.OnOffControl)> ZeroScale = &H80
        ''' <summary>Enforced Zero Return</summary>
        <UsageType(UsageTypes.OnOffControl)> EnforcedZeroReturn = &H81
    End Enum

    ''' <summary>Contains HID usage code s for <see cref="UsagePages.MSRDevices"/> page</summary>
    ''' <remarks>Values not defined in this enumeration are reserved</remarks>
    Public Enum Usages_MSRDevices
        ''' <summary>Undefined</summary>
        Undefined = 0
        ''' <summary>MSR Device Read-Only</summary>
        <UsageType(UsageTypes.ApplicationCollection)> MSRDeviceReadOnly = &H1
        ''' <summary>Track 1 Length</summary>
        <UsageType(UsageTypes.StaticFlag Or UsageTypes.DynamicFlag Or UsageTypes.Selector)> Track1Length = &H11
        ''' <summary>Track 2 Length</summary>
        <UsageType(UsageTypes.StaticFlag Or UsageTypes.DynamicFlag Or UsageTypes.Selector)> Track2Length = &H12
        ''' <summary>Track 3 Length</summary>
        <UsageType(UsageTypes.StaticFlag Or UsageTypes.DynamicFlag Or UsageTypes.Selector)> Track3Length = &H13
        ''' <summary>Track JIS Length</summary>
        <UsageType(UsageTypes.StaticFlag Or UsageTypes.DynamicFlag Or UsageTypes.Selector)> TrackJISLength = &H14
        ''' <summary>Track Data</summary>
        <UsageType(UsageTypes.StaticFlag Or UsageTypes.DynamicFlag Or UsageTypes.Selector)> TrackData = &H20
        ''' <summary>Track 1 Data</summary>
        <UsageType(UsageTypes.StaticFlag Or UsageTypes.DynamicFlag Or UsageTypes.Selector)> Track1Data = &H21
        ''' <summary>Track 2 Data</summary>
        <UsageType(UsageTypes.StaticFlag Or UsageTypes.DynamicFlag Or UsageTypes.Selector)> Track2Data = &H22
        ''' <summary>Track 3 Data</summary>
        <UsageType(UsageTypes.StaticFlag Or UsageTypes.DynamicFlag Or UsageTypes.Selector)> Track3Data = &H23
        ''' <summary>Track JIS Data</summary>
        <UsageType(UsageTypes.StaticFlag Or UsageTypes.DynamicFlag Or UsageTypes.Selector)> TrackJISData = &H24
    End Enum

    ''' <summary>Contains HID usage code s for <see cref="UsagePages.Arcade"/> page</summary>
    ''' <remarks>Values not defined in this enumeration are reserved</remarks>
    Public Enum Usages_Arcade
        ''' <summary>Undefined</summary>
        Undefined = 0
        ''' <summary>General Purpose IO Card</summary>
        <UsageType(UsageTypes.ApplicationCollection)> GeneralPurposeIOCard = &H1
        ''' <summary>Coin Door</summary>
        <UsageType(UsageTypes.ApplicationCollection)> CoinDoor = &H2
        ''' <summary>Watchdog Timer</summary>
        <UsageType(UsageTypes.ApplicationCollection)> WatchdogTimer = &H3
        ''' <summary>General Purpose Analog Input State</summary>
        <UsageType(UsageTypes.DynamicValue)> GeneralPurposeAnalogInputState = &H30
        ''' <summary>General Purpose Digital Input State</summary>
        <UsageType(UsageTypes.DynamicValue)> GeneralPurposeDigitalInputState = &H31
        ''' <summary>General Purpose Optical Input State</summary>
        <UsageType(UsageTypes.DynamicValue)> GeneralPurposeOpticalInputState = &H32
        ''' <summary>General Purpose Digital Output State</summary>
        <UsageType(UsageTypes.DynamicValue)> GeneralPurposeDigitalOutputState = &H33
        ''' <summary>Number of Coin Doors</summary>
        <UsageType(UsageTypes.DynamicValue)> NumberOfCoinDoors = &H34
        ''' <summary>Coin Drawer Drop Count</summary>
        <UsageType(UsageTypes.DynamicValue)> CoinDrawerDropCount = &H35
        ''' <summary>Coin Drawer Start</summary>
        <UsageType(UsageTypes.OneShotControl)> CoinDrawerStart = &H36
        ''' <summary>Coin Drawer Service</summary>
        <UsageType(UsageTypes.OneShotControl)> CoinDrawerService = &H37
        ''' <summary>Coin Drawer Tilt</summary>
        <UsageType(UsageTypes.OneShotControl)> CoinDrawerTilt = &H38
        ''' <summary>Coin Door Test</summary>
        <UsageType(UsageTypes.OneShotControl)> CoinDoorTest = &H39
        ''' <summary>Coin Door Lockout</summary>
        <UsageType(UsageTypes.OneShotControl )> CoinDoorLockout = &H40
        ''' <summary>Watchdog Timeout</summary>
        <UsageType(UsageTypes.DynamicValue)> WatchdogTimeout = &H41
        ''' <summary>Watchdog Action</summary>
        <UsageType(UsageTypes.NamedArray)> WatchdogAction = &H42
        ''' <summary>Watchdog Reboot</summary>
        <UsageType(UsageTypes.Selector)> WatchdogReboot = &H43
        ''' <summary>Watchdog Restart</summary>
        <UsageType(UsageTypes.Selector)> WatchdogRestart = &H44
        ''' <summary>Alarm Input</summary>
        <UsageType(UsageTypes.DynamicValue)> AlarmInput = &H45
        ''' <summary>Coin Door Counter</summary>
        <UsageType(UsageTypes.OneShotControl)> CoinDoorCounter = &H46
        ''' <summary>I/O Direction Mapping</summary>
        <UsageType(UsageTypes.DynamicValue)> IODirectionMapping = &H47
        ''' <summary>Set I/O Direction</summary>
        <UsageType(UsageTypes.OneShotControl)> SetIODirection = &H48
        ''' <summary>Extended Optical Input State</summary>
        <UsageType(UsageTypes.DynamicValue)> ExtendedOpticalInputState = &H49
        ''' <summary>Pin Pad Input State</summary>
        <UsageType(UsageTypes.DynamicValue)> PinPadInputState = &H4A
        ''' <summary>Pin Pad Status</summary>
        <UsageType(UsageTypes.DynamicValue)> PinPadStatus = &H4B
        ''' <summary>Pin Pad Output</summary>
        <UsageType(UsageTypes.OneShotControl)> PinPadOutput = &H4C
        ''' <summary>Pin Pad Command</summary>
        <UsageType(UsageTypes.DynamicValue)> PinPadCommand = &H4D
    End Enum
#End Region
End Namespace
#End If