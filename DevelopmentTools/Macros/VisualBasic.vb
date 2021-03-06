Imports System
Imports EnvDTE
Imports EnvDTE80
Imports EnvDTE90
Imports System.Diagnostics
Imports System.Collections.Generic
''' <summary>Contains Visual-Basic-specific macros</summary>
Public Module VisualBasic

    ''' <summary>In current selection repleces Visual Basic constant declarations with enum member declarations</summary>
    Public Sub Consts2Enum()
        DTE.UndoContext.Open("Const2Enum")
        Try
            DirectCast(DTE.ActiveWindow.Selection, TextSelection).Text = Consts2Enum(DirectCast(DTE.ActiveWindow.Selection, TextSelection).Text)
        Finally
            DTE.UndoContext.Close()
        End Try
    End Sub
    Private ReadOnly ConstRegExp As New System.Text.RegularExpressions.Regex( _
       "^\s*(((Public)|(Private))\s+)?Const\s+(?<Name>[\p{L}_][\p{L}\p{N}_]*)(\s+As\s+[\p{L}_][\p{L}\p{N}_]*)?\s*=\s*(?<Value>[+|-]?([0-9]+|(&h[0-9A-F]+)))\s*$", _
       Text.RegularExpressions.RegexOptions.Compiled Or Text.RegularExpressions.RegexOptions.CultureInvariant Or Text.RegularExpressions.RegexOptions.ExplicitCapture Or Text.RegularExpressions.RegexOptions.IgnoreCase)
    ''' <summary>Parses string which contains series of visual basic numeric constant definitions to enum members</summary>
    ''' <param name="Consts">Constant definitions</param>
    ''' <returns>For each line in <paramref name="Consts"/>: If it contains VB numeric constant definition its converted to Name = Value; otherwise it is returned as is</returns>
    Private Function Consts2Enum$(ByVal Consts$)
        Dim ret As New System.Text.StringBuilder
        Dim r As New IO.StringReader(Consts)
        Dim Line$ = r.ReadLine
        While Line IsNot Nothing
            Dim Result = ConstRegExp.Match(Line)
            If Result.Success Then
                ret.AppendLine(String.Format("{0} = {1}", Result.Groups!Name.Value, Result.Groups!Value.Value))
            Else
                ret.AppendLine(Line)
            End If
            Line = r.ReadLine
        End While
        Return ret.ToString
    End Function

    ''' <summary>Converts text in selection in format used for definition of enumerations on MSDN for unmanaged functions to enum body in Visual Basic</summary>
    Public Sub MsdnEnum2Enum()
        DTE.UndoContext.Open("MsdnEnum2Enum")
        Try
            DirectCast(DTE.ActiveWindow.Selection, TextSelection).Insert(MsdnEnum2Enum(DirectCast(DTE.ActiveWindow.Selection, TextSelection).Text))
        Finally
            DTE.UndoContext.Close()
        End Try
    End Sub
    ''' <summary>Converts text in form<para>constant name</para><para>sumary</para> to form <para>''' &lt;>summary>summary&lt;sumary></para><para>constant name</para></summary>
    ''' <param name="Enum">Text to be converted</param>
    ''' <returns>String suitable as VB enum body</returns>
    Private Function MsdnEnum2Enum(ByVal Enum$) As String
        Dim r As New IO.StringReader([Enum])
        Dim ret As New Text.StringBuilder
        Dim L1 As String = r.ReadLine
        Dim L2 As String = r.ReadLine
        While L1 IsNot Nothing AndAlso L2 IsNot Nothing
            ret.AppendLine(String.Format("''' <summary>{0}</summary>", L2.Trim.Replace("&", "&amp;").Replace("<", "&lt;")))
            ret.AppendLine(L1.Trim)
            L1 = r.ReadLine
            L2 = r.ReadLine
        End While
        Return ret.ToString
    End Function
    ''' <summary>Converts selected lines with C++ #defines to VB enumeration members</summary>
    Public Sub CDefine2Enum()
        DTE.UndoContext.Open("CDefine2Enum")
        Try
            DirectCast(DTE.ActiveWindow.Selection, TextSelection).Insert(CDefine2Enum(DirectCast(DTE.ActiveWindow.Selection, TextSelection).Text))
        Finally
            DTE.UndoContext.Close()
        End Try
    End Sub
    ''' <summary>Converts lines with C++ #defines to VB enumeration members</summary>
    ''' <param name="CText">C++ #defines. 1 a line</param>
    ''' <returns>VB enum members. Unrecognized lines remains unchanged.</returns>
    Private Function CDefine2Enum$(ByVal CText$)
        Dim r As New IO.StringReader(CText)
        Dim ret As New Text.StringBuilder
        Dim Line$ = r.ReadLine
        Static regExp As New System.Text.RegularExpressions.Regex("^\s*#define\s+(?<Name>[\p{L}_][\p{L}\p{N}_]*)\s+((?<Value>[0-9]+)|(0x(?<HexValue>[0-9A-Z]+)))\s*$", Text.RegularExpressions.RegexOptions.Compiled Or Text.RegularExpressions.RegexOptions.CultureInvariant Or Text.RegularExpressions.RegexOptions.IgnoreCase Or Text.RegularExpressions.RegexOptions.ExplicitCapture)
        While Line IsNot Nothing
            Dim match = regExp.Match(Line)
            If match.Success Then
                If match.Groups!Value.Value <> "" Then
                    ret.AppendLine(String.Format("{0} = {1}", match.Groups!Name.Value, match.Groups!Value.Value))
                Else
                    ret.AppendLine(String.Format("{0} = &h{1}", match.Groups!Name.Value, match.Groups!HexValue.Value))
                End If
            Else
                ret.AppendLine(Line)
            End If
            Line = r.ReadLine
        End While
        Return ret.ToString
    End Function
    ''' <summary>In selection merges VB enumeration members (more members with same name) into one meber for each name. Merges comments and values</summary>
    Public Sub EnumMerge()
        DTE.UndoContext.Open("EnumMerge")
        Try
            DirectCast(DTE.ActiveWindow.Selection, TextSelection).Insert(EnumMerge(DirectCast(DTE.ActiveWindow.Selection, TextSelection).Text))
        Finally
            DTE.UndoContext.Close()
        End Try
    End Sub
    ''' <summary>Merges comments and value of enumeration members in Visual Basic format</summary>
    ''' <param name="Enm">Body of enumeration that contains some members more than once</param>
    ''' <returns>Members of enumeration paired by name in case-insensitive way with merged commnents and values</returns>
    Private Function EnumMerge(ByVal Enm As String) As String
        Dim r As New IO.StringReader(Enm)
        Dim ret As New Text.StringBuilder
        Dim Line As String = r.ReadLine
        Static regExp As New System.Text.RegularExpressions.Regex("^\s*(?<Name>[\p{L}_][\p{L}\p{N}_]*)(\s*=\s*(?<Value>[0-9]+|(&h[0-9A-Z]+)))?\s*$", Text.RegularExpressions.RegexOptions.Compiled Or Text.RegularExpressions.RegexOptions.CultureInvariant Or Text.RegularExpressions.RegexOptions.ExplicitCapture Or Text.RegularExpressions.RegexOptions.IgnoreCase)
        Dim Items As New Dictionary(Of String, KeyValuePair(Of String, String))(StringComparer.InvariantCultureIgnoreCase)
        Dim CommentB As New System.Text.StringBuilder
        While Line IsNot Nothing
            If Line.TrimStart.StartsWith("'''") Then
                CommentB.AppendLine(Line)
            Else
                Dim Match = regExp.Match(Line)
                If Match.Success AndAlso Items.ContainsKey(Match.Groups!Name.Value) Then
                    Dim Comment = Items(Match.Groups!Name.Value).Value
                    Dim Value = Items(Match.Groups!Name.Value).Key
                    Comment &= If(Comment <> "" AndAlso CommentB.Length <> 0, vbCrLf, "") & CommentB.ToString
                    Value = If(Value <> "" AndAlso Value <> Match.Groups!Value.Value AndAlso Match.Groups!Value.Value <> "", Value & "'" & Match.Groups!Name.Value, Match.Groups!Value.Value)
                    Items(Match.Groups!Name.Value) = New KeyValuePair(Of String, String)(Value, Comment)
                ElseIf Match.Success Then
                    Items.Add(Match.Groups!Name.Value, New KeyValuePair(Of String, String)(Match.Groups!Value.Value, CommentB.ToString))
                End If
                CommentB = New Text.StringBuilder
            End If
            Line = r.ReadLine
        End While
        For Each item In Items
            If item.Value.Value <> "" Then ret.AppendLine(item.Value.Value.Trim)
            If item.Value.Key <> "" Then ret.AppendLine(String.Format("{0} = {1}", item.Key, item.Value.Key)) Else ret.AppendLine(item.Key)
        Next
        Return ret.ToString
    End Function

End Module



