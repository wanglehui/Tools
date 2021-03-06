#If True
Partial Class MathT
    ''' <summary>Least Common Multiple</summary>
    ''' <param name="n1">First number</param>
    ''' <param name="n2">Second number</param>
    ''' <returns>Least common multiple (LCM) of <paramref name="n1"/> and <paramref name="n2"/></returns>
    ''' <remarks>Uses Ευκλιδέσ's alghoritm <seealso>http://www.devx.com/vb2themax/Tip/19015</seealso> Thanks to Francesco Balena</remarks>
    ''' <exception cref="DivideByZeroException"><paramref name="n1"/> or <paramref name="n2"/> is zero</exception>
    ''' <author www="http://dzonny.cz">Đonny</author>
    ''' <version version="1.5.2" stage="Release"><see cref="VersionAttribute"/> and <see cref="AuthorAttribute"/> removed</version>
    ''' <version version="1.5.3">Fix: Returns only either 0 or 1.</version>
    Public Shared Function LCM(ByVal n1 As Long, ByVal n2 As Long) As Long
        Return (n1 * n2) \ GCD(n1, n2)
    End Function
    ''' <summary>Greatest Common Divisor</summary>
    ''' <param name="n1">First number</param>
    ''' <param name="n2">Second number</param>
    ''' <returns>Greatest common divisor (GCD) of <paramref name="n1"/> and <paramref name="n2"/></returns>
    ''' <remarks>Uses Ευκλιδέσ's alghoritm <seealso>http://www.devx.com/vb2themax/Tip/19014</seealso> Thanks to Francesco Balena</remarks>
    ''' <exception cref="DivideByZeroException"><paramref name="n1"/> or <paramref name="n2"/> is zero</exception>
    ''' <author www="http://dzonny.cz">Đonny</author>
    ''' <version version="1.5.2" stage="Release"><see cref="VersionAttribute"/> and <see cref="AuthorAttribute"/> removed</version>
    Public Shared Function GCD(ByVal n1 As Long, ByVal n2 As Long) As Long
        Dim tmp As Long
        Do
            ' swap the items so that n1 >= n2
            If n1 < n2 Then
                tmp = n1
                n1 = n2
                n2 = tmp
            End If
            ' take the modulo
            n1 = n1 Mod n2
        Loop While n1

        GCD = n2
    End Function
End Class
#End If