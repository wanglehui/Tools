﻿Imports System.Xml.Linq
Imports <xmlns='http://www.unicode.org/ns/2003/ucd/1.0'>
Imports Tools.ExtensionsT
Imports System.Globalization.CultureInfo
Imports Tools.MathT

Namespace TextT.UnicodeT
    ''' <summary>Represents single Unicode code point and provides information about it</summary>
    ''' <version version="1.5.4">This class is new in version 1.5.4</version>
    Public MustInherit Class UnicodeCodePoint : Inherits UnicodePropertiesProvider
        ''' <summary>CTor - creates a new instance of the <see cref="UnicodeCodePoint"/> class</summary>
        ''' <param name="element">A XML element which stores the properties</param>
        ''' <exception cref="ArgumentNullException"><paramref name="element"/> is null.</exception>
        Protected Sub New(element As XElement)
            MyBase.New(element)
        End Sub

        ''' <summary>When overriden in derived class gets type of this code point</summary>
        Public MustOverride ReadOnly Property Type As UnicodeCodePointType

        ''' <summary>Creates an instance of <see cref="UnicodeCodePoint"/>-derived class from XML element</summary>
        ''' <param name="element">An XML element representing Unicode code point</param>
        ''' <returns>An instance of <see cref="UnicodeCodePoint"/>-derived class. Which? It depends on name of <paramref name="element"/></returns>
        ''' <exception cref="ArgumentNullException"><paramref name="element"/> is nulll</exception>
        ''' <exception cref="ArgumentException"><paramref name="element"/> is neither &lt;reserved>, &lt;noncharacter>, &lt;surrogate> nor &lt;char> in namespace http://www.unicode.org/ns/2003/ucd/1.0.</exception>
        Public Shared Function Create(element As XElement) As UnicodeCodePoint
            If element Is Nothing Then Throw New ArgumentNullException("elemen")
            Select Case element.Name
                Case ReservedUnicodeCodePoint.elementName : Return New ReservedUnicodeCodePoint(element)
                Case UnicodeNoncharacter.elementName : Return New UnicodeNoncharacter(element)
                Case UnicodeSurrogate.elementName : Return New UnicodeSurrogate(element)
                Case UnicodeCharacter.elementName : Return New UnicodeCharacter(element)
                Case Else : Throw New ArgumentException(ResourcesT.Exceptions.UnsupportedElement.f(element.Name))
            End Select
        End Function

        ''' <summary>Creates an instance of <see cref="UnicodeCodePoint"/>-derived class from XML element and sets its group</summary>
        ''' <param name="element">An XML element representing Unicode code point</param>
        ''' <param name="group">A group code point is defined in</param>
        ''' <returns>An instance of <see cref="UnicodeCodePoint"/>-derived class. Which? It depends on name of <paramref name="element"/></returns>
        ''' <exception cref="ArgumentNullException"><paramref name="element"/> is nulll</exception>
        ''' <exception cref="ArgumentException"><paramref name="element"/> is neither &lt;reserved>, &lt;noncharacter>, &lt;surrogate> nor &lt;char> in namespace http://www.unicode.org/ns/2003/ucd/1.0.</exception>
        Public Shared Function Create(element As XElement, group As UnicodeCharacterGroup) As Object
            If element Is Nothing Then Throw New ArgumentNullException("elemen")
            Select Case element.Name
                Case ReservedUnicodeCodePoint.elementName : Return New ReservedUnicodeCodePoint(element) With {.Group = group}
                Case UnicodeNoncharacter.elementName : Return New UnicodeNoncharacter(element) With {.Group = group}
                Case UnicodeSurrogate.elementName : Return New UnicodeSurrogate(element) With {.Group = group}
                Case UnicodeCharacter.elementName : Return New UnicodeCharacter(element) With {.Group = group}
                Case Else : Throw New ArgumentException(ResourcesT.Exceptions.UnsupportedElement.f(element.Name))
            End Select
        End Function

        Private _group As UnicodeCharacterGroup
        ''' <summary>Gets group code point represented by this instance belongs to</summary>
        ''' <returns>A group this code point belongs to. Null if current code point does not belong to any group.</returns>
        ''' <exception cref="InvalidOperationException">In setter: <note>Setter of this property is private.</note> Property is being set and it was already set.</exception>
        ''' <remarks>If Unicode Character Database was loaded from so-called flat file no character is in a group.</remarks>
        Public Property Group As UnicodeCharacterGroup
            Get
                If _group IsNot Nothing Then Return _group
                If Element.Parent.Name = UnicodeCharacterGroup.elementName Then
                    _group = New UnicodeCharacterGroup(Element.Parent)
                    Return _group
                Else
                    Return Nothing
                End If
            End Get
            Private Set(value As UnicodeCharacterGroup)
                If _group IsNot Nothing Then Throw New InvalidOperationException(ResourcesT.Exceptions.PropertyWasAlreadySet.f("Group"))
                _group = value
            End Set
        End Property

        ''' <summary>Gets value of given property (attributes)</summary>
        ''' <param name="name">Name of the property (attribute) to get value of. This is name of property (XML attribute) as used in Unicode Character Database XML.</param>
        ''' <returns>Value of the property (attribute) as string. If the attribute is not present on <see cref="Element"/> and <see cref="Group"/> is not null the attribute si searched on <see cref="Group"/>. If it's present neither on <see cref="Element"/> nor on <see cref="Group"/> null is returned.</returns>
        <EditorBrowsable(EditorBrowsableState.Advanced)>
        Public Overrides Function GetPropertyValue$(name$)
            Dim attr = Element.Attribute(name)
            If attr Is Nothing AndAlso Group IsNot Nothing Then Return Group.GetPropertyValue(name)
            Return attr.Value
        End Function

#Region "Properties"
        ''' <summary>Gets name of the character in current version of Unicode standard</summary>
        ''' <remarks>
        ''' If specified on group or range can contain character #. When specified on individual code point, character # is replaced with value of current code point.
        ''' <para>Unicode character names are usually uppercase.</para>
        ''' <para>Underlying XML attribute is @na.</para>
        ''' </remarks>
        Public Overrides ReadOnly Property Name As String
            Get
                Dim value = MyBase.Name
                If value IsNot Nothing AndAlso value.Contains("#") AndAlso CodePoint.HasValue Then
                    Return value.Replace("#", CodePoint.Value.ToString("X", InvariantCulture))
                End If
                Return value
            End Get
        End Property
        ''' <summary>Gets name of the character the character had in version 1 of Unicode standard</summary>
        ''' <returns>Name character had in version 1 of Unicode standard (if specified; null otherwise)</returns>
        ''' <remarks>
        ''' If specified on group or range can contain character #. When specified on individual code point, character # is replaced with value of current code point.
        ''' <para>Underlying XML attribute is @na1.</para>
        ''' </remarks>
        <EditorBrowsable(EditorBrowsableState.Advanced)>
        Public Overrides ReadOnly Property Name1 As String
            Get
                Dim value = MyBase.Name1
                If value IsNot Nothing AndAlso value.Contains("#") AndAlso CodePoint.HasValue Then
                    Return value.Replace("#", CodePoint.Value.ToString("X", InvariantCulture))
                End If
                Return value
            End Get
        End Property

        ''' <summary>When not null specifies value of the <see cref="CodePoint"/> property used for single-character instances created from ranges</summary>
        Private _codePoint As UInteger?
        ''' <summary>Gets value of current code point</summary>
        ''' <remarks>
        ''' This property is null for character ranges. They have <see cref="FirstCodePoint"/> and <see cref="LastCodePoint"/> specified instead.
        ''' <para>This property is not CLS-compliant. Corresponding CLS-compilant property is <see cref="CodePointSigned"/>.</para>
        ''' <para>Underlying XML attributes is @cp.</para>
        ''' </remarks>
        <CLSCompliant(False)>
        Public ReadOnly Property CodePoint As UInteger?
            Get
                If _codePoint.HasValue Then Return _codePoint
                Dim value = GetPropertyValue("cp")
                If value.IsNullOrEmpty Then Return Nothing
                Return UInteger.Parse("0x" & value, Globalization.NumberStyles.HexNumber, InvariantCulture)
            End Get
        End Property

        ''' <summary>CLS-comliant version of <see cref="CodePoint"/> ptoperty</summary>
        ''' <seelaso cref="CodePoint"/>
        <EditorBrowsable(EditorBrowsableState.Advanced)>
        Public ReadOnly Property CodePointSigned As Integer?
            Get
                Dim value = CodePoint
                If value Is Nothing Then Return Nothing
                Return value.Value.BitwiseSame
            End Get
        End Property

        ''' <summary>For range characters gets value of first code point this range starts with</summary>
        ''' <returns>Value of code point character range starts with. Null if this instance does not represent character range.</returns>
        ''' <remarks>
        ''' This property is not CLS-compliant. CLS-compliant alternative is <see cref="FirstCodePointSigned"/>.
        ''' <para>Underlying XML attribute is @first-cp.</para>
        ''' </remarks>
        <CLSCompliant(False)>
        Public ReadOnly Property FirstCodePoint As UInteger?
            Get
                Dim value = GetPropertyValue("first-cp")
                If value.IsNullOrEmpty Then Return Nothing
                Return UInteger.Parse("0x" & value, Globalization.NumberStyles.HexNumber, InvariantCulture)
            End Get
        End Property

        ''' <summary>For range characters gets value of last code point this range ends with</summary>
        ''' <returns>Value of code point character range starts with. Null if this instance does not represent character range.</returns>
        ''' <remarks>
        ''' This property is not CLS-compliant. CLS-compliant alternative is <see cref="LastCodePointSigned"/>.
        ''' <para>Underlying XML attribute is @last-cp.</para>
        ''' </remarks>
        <CLSCompliant(False)>
        Public ReadOnly Property LastCodePoint As UInteger?
            Get
                Dim value = GetPropertyValue("last-cp")
                If value.IsNullOrEmpty Then Return Nothing
                Return UInteger.Parse("0x" & value, Globalization.NumberStyles.HexNumber, InvariantCulture)
            End Get
        End Property

        ''' <summary>CLS-comliant version of <see cref="FirstCodePoint"/> ptoperty</summary>
        ''' <seelaso cref="CodePoint"/>
        <EditorBrowsable(EditorBrowsableState.Advanced)>
        Public ReadOnly Property FirstCodePointSigned As Integer?
            Get
                Dim value = FirstCodePoint
                If value Is Nothing Then Return Nothing
                Return value.Value.BitwiseSame
            End Get
        End Property
        ''' <summary>CLS-comliant version of <see cref="LastCodePoint"/> ptoperty</summary>
        ''' <seelaso cref="CodePoint"/>
        <EditorBrowsable(EditorBrowsableState.Advanced)>
        Public ReadOnly Property LastCodePointSigned As Integer?
            Get
                Dim value = LastCodePoint
                If value Is Nothing Then Return Nothing
                Return value.Value.BitwiseSame
            End Get
        End Property
#End Region
        ''' <summary>Returns a <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.</summary>
        ''' <returns>A <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.</returns>
        Public Overrides Function ToString() As String
            Dim name As String = ""
            If Me.Name IsNot Nothing Then name = Me.Name
            Dim value As String = ""
            If Me.CodePoint.HasValue Then
                value = String.Format(InvariantCulture, "U+{0:X}", Me.CodePoint)
            ElseIf Me.FirstCodePoint.HasValue AndAlso Me.LastCodePoint.HasValue Then
                value = String.Format(InvariantCulture, "U+{0:X}-U+{1:X}", Me.FirstCodePoint, Me.LastCodePoint)
            ElseIf Me.FirstCodePoint.HasValue Then
                value = String.Format(InvariantCulture, "U+{0:X}-", Me.FirstCodePoint)
            ElseIf Me.LastCodePoint.HasValue Then
                value = String.Format(InvariantCulture, "-U+{0:X}", Me.LastCodePoint)
            End If
            If name <> "" AndAlso value <> "" Then
                Return name & " " & value
            ElseIf name <> "" OrElse value <> "" Then
                Return name & value
            End If
            Return MyBase.ToString()
        End Function

        ''' <summary>Makes single-character instance form range instance</summary>
        ''' <param name="codePoint">Code point from current range to make single-character instance pointing to</param>
        ''' <returns>
        ''' A new instance of <see cref="UnicodeCodePoint"/>-derived class pointing to single character within range represented by current instance.
        ''' In case <see cref="CodePoint"/> is not null and <paramref name="codePoint"/> equals <see cref="CodePoint"/> returns current instance.
        ''' </returns>
        ''' <exception cref="InvalidOperationException">
        ''' Current instance is not character range and <paramref name="codePoint"/> differs from <see cref="CodePoint"/> -or-
        ''' The range is specified incorrectly (i.e. either <see cref="FirstCodePoint"/> or <see cref="LastCodePoint"/> is null.
        ''' </exception>
        ''' <exception cref="ArgumentOutOfRangeException"><paramref name="codePoint"/> is less than <see cref="FirstCodePoint"/> or greater than <see cref="LastCodePoint"/>.</exception>
        ''' <remarks>This function is not CLS-compliant. CLS-compliant overload exists.</remarks>
        <CLSCompliant(False)>
        Public Function MakeSingle(codePoint As UInteger) As UnicodeCodePoint
            If Me.CodePoint.HasValue AndAlso codePoint = Me.CodePoint.Value Then Return Me
            If Me.CodePoint.HasValue Then Throw New InvalidOperationException(ResourcesT.Exceptions.MakeSingleFromSingle)
            If Me.FirstCodePoint Is Nothing OrElse Me.LastCodePoint Is Nothing Then Throw New InvalidOperationException(ResourcesT.Exceptions.RangeNotFullySpecified)
            If codePoint < Me.FirstCodePoint.Value OrElse codePoint > Me.LastCodePoint.Value Then Throw New ArgumentOutOfRangeException("codePoint")
            Dim clone = Create(Element)
            If _group IsNot Nothing Then clone.Group = _group
            clone._codePoint = codePoint
            Return clone
        End Function
        ''' <summary>Makes single-character instance form range instance</summary>
        ''' <param name="codePoint">Code point from current range to make single-character instance pointing to</param>
        ''' <returns>
        ''' A new instance of <see cref="UnicodeCodePoint"/>-derived class pointing to single character within range represented by current instance.
        ''' In case <see cref="CodePoint"/> is not null and <paramref name="codePoint"/> equals <see cref="CodePointSigned"/> returns current instance.
        ''' </returns>
        ''' <exception cref="InvalidOperationException">
        ''' Current instance is not character range and <paramref name="codePoint"/> differs from <see cref="CodePointSigned"/> -or-
        ''' The range is specified incorrectly (i.e. either <see cref="FirstCodePoint"/> or <see cref="LastCodePoint"/> is null.
        ''' </exception>
        ''' <exception cref="ArgumentOutOfRangeException"><paramref name="codePoint"/> is less than <see cref="FirstCodePointSigned"/> or greater than <see cref="LastCodePointSigned"/>.</exception>
        <CLSCompliant(False)>
        Public Function MakeSingle(codePoint As Integer) As UnicodeCodePoint
            Return MakeSingle(codePoint.BitwiseSame)
        End Function

        ''' <summary>Gets <see cref="CodePointInfo"/> instance pointing to current character</summary>
        ''' <returns>A new <see cref="CodePointInfo"/> instance pointing to charatcer indicated by <see cref="CodePoint"/> property.</returns>
        ''' <exception cref="InvalidOperationException">This instance represents character range.</exception>
        Public Function AsCodePointInfo() As CodePointInfo
            If Not CodePoint.HasValue Then Throw New InvalidOperationException(ResourcesT.Exceptions.OperationIsNotValidForCharacterRanges)
            Return New CodePointInfo(Element.Document, CodePoint.Value)
        End Function

        ''' <summary>Converts <see cref="UnicodeCodePoint"/> to <see cref="CodePointInfo"/></summary>
        ''' <param name="a">A <see cref="UnicodeCodePoint"/></param>
        ''' <returns>A new instance of <see cref="CodePointInfo"/> class pointing to same character as <paramref name="a"/>; null if <paramref name="a"/> is null.</returns>
        ''' <exception cref="InvalidOperationException"><paramref name="a"/> is character range.</exception>
        ''' <seelaso cref="AsCodePointInfo"/>
        Public Shared Narrowing Operator CType(a As UnicodeCodePoint) As CodePointInfo
            If a Is Nothing Then Return Nothing
            Return a.AsCodePointInfo
        End Operator

        ''' <summary>Converts <see cref="CodePointInfo"/> to <see cref="UnicodeCodePoint"/></summary>
        ''' <param name="a">A <see cref="CodePointInfo"/></param>
        ''' <returns>A new instance of <see cref="UnicodeCodePoint"/> providing information about character pointed by <paramref name="a"/>; null if <paramref name="a"/> is null.</returns>
        ''' <exception cref="InvalidOperationException"><paramref name="a"/> was initialized without instance of <see cref="XDocument"/> -or- Information about code point pointed by <paramref name="a"/> cannot be found in the XML document.</exception>
        Public Shared Widening Operator CType(a As CodePointInfo) As UnicodeCodePoint
            If a Is Nothing Then Return Nothing
            Dim ret = a.UnicodeCodePoint
            If ret Is Nothing Then Throw New InvalidOperationException(ResourcesT.Exceptions.CannotFindCodePoint)
            Return ret
        End Operator
    End Class

    ''' <summary>Enumerates Unicode code point types</summary>
    ''' <version version="1.5.4">This enum is new in version 1.5.4</version>
    Public Enum UnicodeCodePointType
        ''' <summary>Reserved code point (i.e. it's not assigned to a character in current version of Unicode standard)</summary>
        Reserved
        ''' <summary>Non-character code point</summary>
        Noncharacter
        ''' <summary>Surrogate code point</summary>
        Surrogate
        ''' <summary>Character code point</summary>
        Character
    End Enum

    ''' <summary>Represents reserved Unicode code point (i.e. code point that's currently not assigned to a character)</summary>
    ''' <version version="1.5.4">This class is new in version 1.5.4</version>
    Public Class ReservedUnicodeCodePoint : Inherits UnicodeCodePoint
        ''' <summary>Name of element representing this class in XML</summary>
        Friend Shared ReadOnly elementName As XName = <reserved/>.Name
        ''' <summary>CTor - creates a new instance of the <see cref="ReservedUnicodeCodePoint"/> class</summary>
        ''' <param name="element">An XML element containing data for this unicode code point</param>
        ''' <exception cref="ArgumentNullException"><paramref name="element"/> is null</exception>
        ''' <exception cref="ArgumentException"><paramref name="element"/> is not &lt;reserved> in namespace http://www.unicode.org/ns/2003/ucd/1.0.</exception>
        Public Sub New(element As XElement)
            MyBase.New(element)
            If element.Name <> elementName Then Throw New ArgumentException(ResourcesT.Exceptions.ElementMustBe.f(elementName))
        End Sub
        ''' <summary>Gets type of this code point</summary>
        ''' <returns>This implementation always returns <see cref="UnicodeCodePointType.Reserved"/></returns>
        Public NotOverridable Overrides ReadOnly Property Type As UnicodeCodePointType
            Get
                Return UnicodeCodePointType.Reserved
            End Get
        End Property
    End Class

    ''' <summary>Represents non-character code point in Unicode</summary>
    ''' <version version="1.5.4">This class is new in version 1.5.4</version>
    Public Class UnicodeNoncharacter : Inherits UnicodeCodePoint
        ''' <summary>Name of element representing this class in XML</summary>
        Friend Shared ReadOnly elementName As XName = <noncharacter/>.Name
        ''' <summary>CTor - creates a new instance of the <see cref="UnicodeNoncharacter"/> class</summary>
        ''' <param name="element">An XML element containing data for this unicode code point</param>
        ''' <exception cref="ArgumentNullException"><paramref name="element"/> is null</exception>
        ''' <exception cref="ArgumentException"><paramref name="element"/> is not &lt;noncharacter> in namespace http://www.unicode.org/ns/2003/ucd/1.0.</exception>
        Public Sub New(element As XElement)
            MyBase.New(element)
            If element.Name <> elementName Then Throw New ArgumentException(ResourcesT.Exceptions.ElementMustBe.f(elementName))
        End Sub
        ''' <summary>Gets type of this code point</summary>
        ''' <returns>This implementation always returns <see cref="UnicodeCodePointType.Noncharacter"/></returns>
        Public NotOverridable Overrides ReadOnly Property Type As UnicodeCodePointType
            Get
                Return UnicodeCodePointType.Noncharacter
            End Get
        End Property
    End Class

    ''' <summary>Represents surrogate code point in Unicode</summary>
    ''' <version version="1.5.4">This class is new in version 1.5.4</version>
    Public Class UnicodeSurrogate : Inherits UnicodeCodePoint
        ''' <summary>Name of element representing this class in XML</summary>
        Friend Shared ReadOnly elementName As XName = <surrogate/>.Name
        ''' <summary>CTor - creates a new instance of the <see cref="UnicodeSurrogate"/> class</summary>
        ''' <param name="element">An XML element containing data for this unicode code point</param>
        ''' <exception cref="ArgumentNullException"><paramref name="element"/> is null</exception>
        ''' <exception cref="ArgumentException"><paramref name="element"/> is not &lt;surrogate> in namespace http://www.unicode.org/ns/2003/ucd/1.0.</exception>
        Public Sub New(element As XElement)
            MyBase.New(element)
            If element.Name <> elementName Then Throw New ArgumentException(ResourcesT.Exceptions.ElementMustBe.f(elementName))
        End Sub
        ''' <summary>Gets type of this code point</summary>
        ''' <returns>This implementation always returns <see cref="UnicodeCodePointType.Surrogate"/></returns>
        Public NotOverridable Overrides ReadOnly Property Type As UnicodeCodePointType
            Get
                Return UnicodeCodePointType.Surrogate
            End Get
        End Property
    End Class

    ''' <summary>Represents character code point in Unicode</summary>
    ''' <version version="1.5.4">This class is new in version 1.5.4</version>
    Public Class UnicodeCharacter : Inherits UnicodeCodePoint
        ''' <summary>Name of element representing this class in XML</summary>
        Friend Shared ReadOnly elementName As XName = <char/>.Name
        ''' <summary>CTor - creates a new instance of the <see cref="UnicodeCharacter"/> class</summary>
        ''' <param name="element">An XML element containing data for this unicode code point</param>
        ''' <exception cref="ArgumentNullException"><paramref name="element"/> is null</exception>
        ''' <exception cref="ArgumentException"><paramref name="element"/> is not &lt;char> in namespace http://www.unicode.org/ns/2003/ucd/1.0.</exception>
        Public Sub New(element As XElement)
            MyBase.New(element)
            If element.Name <> elementName Then Throw New ArgumentException(ResourcesT.Exceptions.ElementMustBe.f(elementName))
        End Sub
        ''' <summary>Gets type of this code point</summary>
        ''' <returns>This implementation always returns <see cref="UnicodeCodePointType.Character"/></returns>
        Public NotOverridable Overrides ReadOnly Property Type As UnicodeCodePointType
            Get
                Return UnicodeCodePointType.Character
            End Get
        End Property
    End Class
End Namespace