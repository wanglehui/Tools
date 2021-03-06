Imports System, System.Windows.Forms
Imports EnvDTE
Imports EnvDTE80
Imports EnvDTE90
Imports System.Diagnostics
Imports Microsoft.VisualStudio.Shell.Interop
Imports System.Xml
Imports Microsoft.VisualStudio.Shell
Imports Microsoft.VisualStudio.VSConstants
Imports System.Runtime.InteropServices
Imports System.Collections.Generic

''' <summary>Contains methods for manipulating item attributes</summary>
Public Module Properties
    ''' <summary>Shows user value, asks for new ad sets value of attribute with given name of all selected items</summary>
    ''' <param name="Name">Name of attribute to show a nd set value of</param>
    ''' <exception cref="ArgumentNullException"><paramref name="Name"/> is null</exception>
    Public Sub SetProperty(ByVal Name$)
        Dim OldValue As String = Nothing
        Dim ovSet As Boolean = False
        For i = 1 To DTE.SelectedItems.Count
            Dim ps = GetPropertyStorage(DTE.SelectedItems.Item(i).ProjectItem)
            If Not ovSet Then
                Try
                    OldValue = ps.Attributes(Name)
                Catch ex As COMException
                    OldValue = Nothing
                End Try
                ovSet = True
            Else
                Dim v = ps.Attributes(Name)
                If v <> OldValue Then OldValue = ""
            End If
        Next
        Dim NewV = InputBox(String.Format("Enter <{0}>", Name), String.Format("<{0}>", Name), OldValue)
        If NewV <> "" Then
            For i = 1 To DTE.SelectedItems.Count
                Dim ps = GetPropertyStorage(DTE.SelectedItems.Item(i).ProjectItem)
                ps.Attributes(Name) = NewV
            Next
        End If
    End Sub
    ''' <summary>Asks user and possibly removes attribute with given name form all selected project items</summary>
    ''' <param name="Name">Name of attribute to remove</param>
    ''' <exception cref="ArgumentNullException"><paramref name="Name"/> is null</exception>
    Public Sub UnsetProperty(ByVal Name$)
        If MsgBox(String.Format("Do you really want to unset <{0}>?", Name), MsgBoxStyle.Question Or MsgBoxStyle.YesNo, String.Format("<{0}>", Name)) = MsgBoxResult.Yes Then
            For i = 1 To DTE.SelectedItems.Count
                Dim ps = GetPropertyStorage(DTE.SelectedItems.Item(i).ProjectItem)
                ps.Attributes(Name) = ""
            Next
        End If
    End Sub

    ''' <summary>Gets property storage for project item</summary>
    ''' <param name="Item">Project item</param>
    ''' <returns><see cref="BuildStorageWrapper"/> to manipulate atributes of <paramref name="Item"/> or null on failure</returns>
    ''' <remarks>See also <a href="http://social.msdn.microsoft.com/forums/en-US/vsx/thread/e1983591-120a-4a2f-a910-e596dd625e68/">http://social.msdn.microsoft.com/forums/en-US/vsx/thread/e1983591-120a-4a2f-a910-e596dd625e68/</a></remarks>
    ''' <exception cref="ArgumentNullException"><paramef name="Item"/> is null</exception>
    ''' <version version="1.5.4">Added <see cref="CLSCompliantAttribute"/>(False)</version>
    <CLSCompliant(False)>
    Public Function GetPropertyStorage(ByVal Item As EnvDTE.ProjectItem) As BuildStorageWrapper
        If Item Is Nothing Then Throw New ArgumentNullException("Item")
        Dim fullPath = Item.Properties.Item("FullPath").Value
        Dim project = Item.ContainingProject
        Dim uniqueName As String = project.UniqueName
        Dim hierarchy As IVsHierarchy = ToHierarchy(project)
        Dim buildPropertyStorage As IVsBuildPropertyStorage = TryCast(hierarchy, IVsBuildPropertyStorage)
        If Not buildPropertyStorage Is Nothing Then
            Dim itemId As System.UInt32
            hierarchy.ParseCanonicalName(fullPath, itemId)
            Return New BuildStorageWrapper(buildPropertyStorage, itemId)
        End If
        Return Nothing
    End Function

    ''' <summary>Setvs value of given attribute of given project item</summary>
    ''' <param name="ProjectItem">Item to set value of attribute of</param>
    ''' <param name="Attribute">Name of attribute to set value of</param>
    ''' <param name="Value">New value of attribute. Null remove attribute.</param>
    ''' <exception cref="ArgumentNullException"><paramref name="Value"/>, <paramref name="Attribute"/> or <paramref name="Item"/> is null</exception>
    ''' <exception cref="COMException">Setting of attribute failed</exception>
    ''' <exception cref="InvalidOperationException">The <see cref="GetPropertyStorage"/> function returned null</exception>
    ''' <seelaso cref="BuildStorageWrapper.Attributes"/>
    ''' <version version="1.5.4">Added <see cref="CLSCompliantAttribute"/>(False)</version>
    <CLSCompliant(False)>
    Public Sub SetCustomAttribute(ByVal ProjectItem As EnvDTE.ProjectItem, ByVal Attribute$, ByVal Value$)
        Dim ps = GetPropertyStorage(ProjectItem)
        If ps IsNot Nothing Then ps.Attributes(Attribute) = Value Else Throw New InvalidOperationException("Unable to obtain BuildStorageWrapper")
    End Sub
    ''' <summary>Gets value of given attribute of given project item</summary>
    ''' <param name="ProjectItem">Item to get value of attribute of</param>
    ''' <param name="Attribute">Name of attribute to get value of</param>
    ''' <returns>Current value of attribute. Null when attribute is not present or obtaining fails</returns>
    ''' <seelaso cref="BuildStorageWrapper.Attributes"/>
    ''' <version version="1.5.4">Added <see cref="CLSCompliantAttribute"/>(False)</version>
    <CLSCompliant(False)>
    Public Function GetCustomAttribute$(ByVal ProjectItem As EnvDTE.ProjectItem, ByVal Attribute$)
        Dim ps = GetPropertyStorage(ProjectItem)
        If ps IsNot Nothing Then Return ps.Attributes(Attribute)
        Return Nothing
    End Function
End Module
