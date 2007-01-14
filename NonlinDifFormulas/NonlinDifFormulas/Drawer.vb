Imports System.Drawing.Drawing2D
''' <summary>Vykresluje rovnici</summary>
Class Drawer
    ''' <summary>Rozmkěry světa ve krerém se kreslí</summary>
    Private Window As RectangleF
    ''' <summary>Počáteční podmínky XY</summary>
    Private PočXY As PointF
    ''' <summary>Počáteční podmínky ZU</summary>
    Private PočZU As PointF
    ''' <summary>Interpret rovnice dx</summary>
    Private dx As SyntaktickyAnalyzator.Analyzer
    ''' <summary>Interpret rovnice dy</summary>
    Private dy As SyntaktickyAnalyzator.Analyzer
    ''' <summary>Interpret rovnice dz</summary>
    Private dz As SyntaktickyAnalyzator.Analyzer
    ''' <summary>Interpret rovnice du</summary>
    Private du As SyntaktickyAnalyzator.Analyzer
    ''' <summary>Cílový obrázek</summary>
    Private img As Bitmap
    ''' <summary>Grafika cílového obrázku</summary>
    Private g As Graphics
    ''' <summary>Tímhle kreslit osy</summary>
    Private AxPen As Pen = Pens.Green
    ''' <summary>Tímhle kreslit graf</summary>
    Private DrPen As Pen = Pens.Yellow
    ''' <summary>Tímhle psát popisky</summary>
    Private AxBrush As System.Drawing.Brush = Brushes.LightBlue
    ''' <summary>Matrix pro převod souřasnic ze světa <see cref="Window"/> na obrázek <see cref="img"/></summary>
    Private TrnsMx As Matrix
    ''' <summary>Počáteční hodnota proměnné t (čas)</summary>
    Private Tmin As Single
    ''' <summary>Koncová hodnota proměnné t (ćas)</summary>
    Private Tmax As Single
    ''' <summary>Krok proměnné t (čas)</summary>
    Private Δt As Single
#Region "CTors"
    ''' <summary>CTor - sloučené parametry a inicializované interprety</summary>
    ''' <param name="Window">Svět do kterého kreslit</param>
    ''' <param name="PočXY">Počáteční podmínky pro X a Y</param>
    ''' <param name="PočZU">Počáteční podmínky pro Z a U</param>
    ''' <param name="dx">Interpret rovnice dx</param>
    ''' <param name="dy">Interpret rovnice dy</param>
    ''' <param name="dz">interpret rovnice dz</param>
    ''' <param name="du">Interpret rovnice du</param>
    ''' <param name="ImgSize">Velikost cílového obrázku v pixelech</param>
    ''' <param name="tMax">Cílová hodnota proměnné t (čas)</param>
    ''' <param name="Δt">Krok proměnné t (čas)</param>
    ''' <param name="tMin">Počáteční hodnota proměnné t (čas)</param>
    Public Sub New( _
             ByVal Window As RectangleF, _
             ByVal PočXY As PointF, ByVal PočZU As PointF, _
             ByVal dx As SyntaktickyAnalyzator.Analyzer, _
             ByVal dy As SyntaktickyAnalyzator.Analyzer, _
             ByVal dz As SyntaktickyAnalyzator.Analyzer, _
             ByVal du As SyntaktickyAnalyzator.Analyzer, _
             ByVal ImgSize As Size, _
             ByVal tMax As Single, ByVal Δt As Single, Optional ByVal tMin As Single = 0.0)
        img = New Bitmap(ImgSize.Width, ImgSize.Height)
        Me.Window = Window
        Me.dx = dx
        Me.dy = dy
        Me.PočXY = PočXY
        Me.PočZU = PočZU
        Me.Tmax = tMax
        Me.Tmin = tMin
        Me.Δt = Δt
        g = Graphics.FromImage(img)
        TransformG()
    End Sub

    ''' <summary>CTor - oddělené parametry a inicializované interprety</summary>
    ''' <param name="minX">Hodnota na ose x na levém okraji okna</param>
    ''' <param name="minY">Hodnota na ose y na spodním okraji okna</param>
    ''' <param name="maxX">Hodnota na ose x na pravém okraji okna</param>
    ''' <param name="maxY">Hosnota na ose y na horním okraji okna</param>
    ''' <param name="PočX">Počáteční hodnota proměnné x</param>
    ''' <param name="PočY">Počáteční hodnota proměnné y</param>
    ''' <param name="PočZ">Počáteční hodnota proměnné z</param>
    ''' <param name="PočU">Počáteční hodnota proměnné u</param>
    ''' <param name="dx">Interpret rovnice dx</param>
    ''' <param name="dy">Interpret rovnice dy</param>
    ''' <param name="dz">interpret rovnice dz</param>
    ''' <param name="du">Interpret rovnice du</param>
    ''' <param name="tMax">Cílová hodnota proměnné t (čas)</param>
    ''' <param name="Δt">Krok proměnné t (čas)</param>
    ''' <param name="tMin">Počáteční hodnota proměnné t (čas)</param>
    ''' <param name="imgWidth">Šířka cílového obrázku v pixelech</param>
    ''' <param name="imgHeight">Výčka cílového obrázku v pixelech</param>
   Public Sub New( _
            ByVal minX As Single, ByVal minY As Single, ByVal maxX As Single, ByVal maxY As Single, _
            ByVal PočX As Single, ByVal PočY As Single, ByVal PočZ As Single, ByVal PočU As Single, _
            ByVal dx As SyntaktickyAnalyzator.Analyzer, ByVal dy As SyntaktickyAnalyzator.Analyzer, _
            ByVal dz As SyntaktickyAnalyzator.Analyzer, ByVal du As SyntaktickyAnalyzator.Analyzer, _
            ByVal tMax As Single, ByVal Δt As Single, Optional ByVal tMin As Single = 0.0, _
            Optional ByVal imgWidth As Integer = 1280, Optional ByVal imgHeight As Integer = 1024 _
    )
        Me.New(New Rectangle(minX, minY, maxX - minX, -(maxY - minY)), _
                New PointF(PočX, PočY), New PointF(PočZ, PočU), _
                dx, dy, dz, du, New Size(imgWidth, imgHeight), tMax, Δt, tMin)
    End Sub
    ''' <summary>CTor - oddělené parametry a rovnice</summary>
    ''' <param name="minX">Hodnota na ose x na levém okraji okna</param>
    ''' <param name="minY">Hodnota na ose y na spodním okraji okna</param>
    ''' <param name="maxX">Hodnota na ose x na pravém okraji okna</param>
    ''' <param name="maxY">Hosnota na ose y na horním okraji okna</param>
    ''' <param name="PočX">Počáteční hodnota proměnné x</param>
    ''' <param name="PočY">Počáteční hodnota proměnné y</param>
    ''' <param name="PočZ">Počáteční hodnota proměnné z</param>
    ''' <param name="PočU">Počáteční hodnota proměnné u</param>
    ''' <param name="dx">Rovnice pro dx</param>
    ''' <param name="dy">Rovnice pro dy</param>
    ''' <param name="dz">Rovnice pro dz</param>
    ''' <param name="du">Rovnice pro du</param>
    ''' <param name="tMax">Cílová hodnota proměnné t (čas)</param>
    ''' <param name="Δt">Krok proměnné t (čas)</param>
    ''' <param name="tMin">Počáteční hodnota proměnné t (čas)</param>
    ''' <param name="imgWidth">Šířka cílového obrázku v pixelech</param>
    ''' <param name="imgHeight">Výčka cílového obrázku v pixelech</param>
   Public Sub New( _
           ByVal minX As Single, ByVal minY As Single, ByVal maxX As Single, ByVal maxY As Single, _
           ByVal PočX As Single, ByVal PočY As Single, ByVal PočZ As Single, ByVal PočU As Single, _
           ByVal dx As String, ByVal dy As String, ByVal dz As String, ByVal du As String, _
           ByVal tMax As Single, ByVal Δt As Single, Optional ByVal tMin As Single = 0.0, _
           Optional ByVal imgWidth As Integer = 1280, Optional ByVal imgHeight As Integer = 1024 _
   )
        Me.New(minX, minY, maxX, maxY, PočX, PočY, PočZ, PočU, _
                New SyntaktickyAnalyzator.Analyzer(dx, New String() {"x", "y", "z", "u", "t"}), _
                New SyntaktickyAnalyzator.Analyzer(dy, New String() {"x", "y", "z", "u", "t"}), _
                New SyntaktickyAnalyzator.Analyzer(dz, New String() {"x", "y", "z", "u", "t"}), _
                New SyntaktickyAnalyzator.Analyzer(du, New String() {"x", "y", "z", "u", "t"}), _
                tMax, Δt, tMin, _
                imgWidth, imgHeight)
    End Sub
#End Region
    ''' <summary>Inicializuje transformační matici <see cref="TrnsMx"/></summary>
    Private Sub TransformG()
        TrnsMx = New Matrix(Window, New PointF() { _
            New PointF(0, 0), New PointF(img.Width, 0), New PointF(0, img.Height)})
    End Sub
    ''' <summary>Vykreslí vše</summary>
    Public Function Draw() As Image
        DrawAxes()
        DrawEqua()
        g.Flush(FlushIntention.Sync)
        Return img
    End Function

    ''' <summary>Vykreslí osy</summary>
    ''' <param name="minX">Hodnota na ose x na levém okraji okna</param>
    ''' <param name="minY">Hodnota na ose y na spodním okraji okna</param>
    ''' <param name="maxX">Hodnota na ose x na pravém okraji okna</param>
    ''' <param name="maxY">Hosnota na ose y na horním okraji okna</param>
    ''' <param name="imgWidth">Šířka obrázku v pixelech</param>
    ''' <param name="imgHeight">Výška obrázku v pixelech</param>
    ''' <returns>Obrázek s vykreslenými osami</returns>
    Public Shared Function DrawAxes(ByVal minX As Single, ByVal minY As Single, ByVal maxX As Single, ByVal maxY As Single, ByVal imgWidth As Integer, ByVal imgHeight As Integer) As Image
        Dim prid As New Drawer(minX, minY, maxX, maxY, 0, 0, 0, 0, "0", "0", "0", "0", 0, 0, , imgWidth, imgHeight)
        prid.DrawAxes()
        prid.g.Flush()
        Return prid.img
    End Function
    ''' <summary>Vykreslí osy</summary>
    Private Sub DrawAxes()
        '  2  
        '  |  
        '0-4-1
        '  |  
        '  3  
        Dim pts As PointF() = { _
                New PointF(Window.Left, 0), New PointF(Window.Right, 0), _
                New PointF(0, Window.Top), New PointF(0, Window.Bottom), _
                New PointF(0, 0)}
        TrnsMx.TransformPoints(pts)
        g.DrawLine(AxPen, pts(0), pts(1))
        g.DrawLine(AxPen, pts(2), pts(3))
        Dim f As New Font("Arial", 16, FontStyle.Regular, GraphicsUnit.Pixel)
        g.DrawString(Window.Top, f, AxBrush, pts(2).X - g.MeasureString(Window.Top, f).Width, pts(2).Y)
        g.DrawString(Window.Bottom, f, AxBrush, pts(3).X - g.MeasureString(Window.Bottom, f).Width, pts(3).Y - 16)
        g.DrawString(Window.Left, f, AxBrush, pts(0).X, pts(0).Y)
        g.DrawString(Window.Right, f, AxBrush, pts(1).X - g.MeasureString(Window.Right, f).Width, pts(1).Y)
        g.DrawString(0, f, AxBrush, pts(4).X - g.MeasureString(0, f).Width, pts(4).Y)
    End Sub
    ''' <summary>Vkreslí graf rovnice</summary>
    Private Sub DrawEqua()
        Dim x As Double = PočXY.X
        Dim y As Double = PočXY.Y
        Dim z As Double = PočZU.X
        Dim u As Double = PočZU.Y
        For t As Double = Tmin To Tmax Step Δt
            Dim oldx As Double = x, oldy As Double = y, oldz As Double = z, oldu As Double = u
            If False Then 'Newtonovo schéma
                Dim newR As Double() = CountRight(t, x, y, z, u)
                x += Δt * newR(0)
                y += Δt * newR(1)
                z += Δt * newR(2)
                u += Δt * newR(3)
            Else 'RK schéma
                Dim k1, k2, k3, k4 As Double()
                k1 = CountRight(t, x, y, z, u)
                Dim pomX As Double = x + k1(0) * Δt / 2, pomY As Double = y + k1(1) * Δt / 2, pomZ As Double = z + k1(2) * Δt / 2, pomU As Double = u + k1(3) * Δt / 2
                k2 = CountRight(t + Δt / 2, pomX, pomY, pomZ, pomU)
                pomX = x + k2(0) * Δt / 2 : pomY = y + k2(1) * Δt / 2 : pomZ = z + k2(2) * Δt / 2 : pomU = u + k2(3) * Δt / 2
                k3 = CountRight(t + Δt / 2, pomX, pomY, pomZ, pomU)
                pomX = x + k3(0) * Δt : pomY = y + k3(1) * Δt : pomZ = z + k3(1) * Δt : pomU = u + k3(3) * Δt
                k4 = CountRight(t + Δt, pomX, pomY, pomZ, pomU)
                x += Δt * (k1(0) + 2 * k2(0) + 2 * k3(0) + k4(0)) / 6
                y += Δt * (k1(1) + 2 * k2(1) + 2 * k3(1) + k4(1)) / 6
                z += Δt * (k1(2) + 2 * k2(2) + 2 * k3(2) + k4(2)) / 6
                u += Δt * (k1(3) + 2 * k2(3) + 2 * k3(3) + k4(3)) / 6
            End If
            Dim pts() As PointF = {New PointF(oldx, oldy), New PointF(x, y)}
            TrnsMx.TransformPoints(pts)
            g.DrawLine(DrPen, pts(0), pts(1))
        Next t
    End Sub
    ''' <summary>Vypočítá pravou stranu rovnice</summary>
    ''' <param name="t">Čas</param>
    ''' <param name="x">x</param>
    ''' <param name="y">x</param>
    ''' <returns>Pole obsahující nové hodnoty pro x,y,z,u</returns>
    Private Function CountRight(ByVal t As Double, ByVal x As Double, ByVal y As Double, ByVal z As Double, ByVal u As Double) As Double()
        Dim ret(1) As Double
        Dim pairs() As KeyValuePair(Of String, Double) = { _
                New KeyValuePair(Of String, Double)("x", x), _
                New KeyValuePair(Of String, Double)("y", y), _
                New KeyValuePair(Of String, Double)("z", z), _
                New KeyValuePair(Of String, Double)("u", u), _
                New KeyValuePair(Of String, Double)("t", t)}
        ret(0) = dx.Calculate(pairs)
        ret(1) = dy.Calculate(pairs)
        ret(2) = dy.Calculate(pairs)
        ret(3) = dy.Calculate(pairs)
        Return ret
    End Function
End Class
