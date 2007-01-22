Namespace Windows.Forms
    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
    Partial Class frmEncodingSelector
        Inherits System.Windows.Forms.Form

        'Form overrides dispose to clean up the component list.
        <System.Diagnostics.DebuggerNonUserCode()> _
        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            Try
                If disposing AndAlso components IsNot Nothing Then
                    components.Dispose()
                End If
            Finally
                MyBase.Dispose(disposing)
            End Try
        End Sub

        'Required by the Windows Form Designer
        Private components As System.ComponentModel.IContainer

        'NOTE: The following procedure is required by the Windows Form Designer
        'It can be modified using the Windows Form Designer.  
        'Do not modify it using the code editor.
        <System.Diagnostics.DebuggerStepThrough()> _
        Private Sub InitializeComponent()
            Me.splMain = New System.Windows.Forms.SplitContainer
            Me.lblSelected = New System.Windows.Forms.Label
            Me.ensSelect = New Tools.Windows.Forms.EncodingSelector
            Me.pgrProperties = New System.Windows.Forms.PropertyGrid
            Me.splMain.Panel1.SuspendLayout()
            Me.splMain.Panel2.SuspendLayout()
            Me.splMain.SuspendLayout()
            Me.SuspendLayout()
            '
            'splMain
            '
            Me.splMain.Dock = System.Windows.Forms.DockStyle.Fill
            Me.splMain.Location = New System.Drawing.Point(0, 0)
            Me.splMain.Name = "splMain"
            Me.splMain.Orientation = System.Windows.Forms.Orientation.Horizontal
            '
            'splMain.Panel1
            '
            Me.splMain.Panel1.Controls.Add(Me.ensSelect)
            Me.splMain.Panel1.Controls.Add(Me.lblSelected)
            '
            'splMain.Panel2
            '
            Me.splMain.Panel2.Controls.Add(Me.pgrProperties)
            Me.splMain.Size = New System.Drawing.Size(287, 492)
            Me.splMain.SplitterDistance = 133
            Me.splMain.TabIndex = 1
            '
            'lblSelected
            '
            Me.lblSelected.Dock = System.Windows.Forms.DockStyle.Bottom
            Me.lblSelected.Location = New System.Drawing.Point(0, 120)
            Me.lblSelected.Name = "lblSelected"
            Me.lblSelected.Size = New System.Drawing.Size(287, 13)
            Me.lblSelected.TabIndex = 1
            Me.lblSelected.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            '
            'ensSelect
            '
            Me.ensSelect.Dock = System.Windows.Forms.DockStyle.Fill
            Me.ensSelect.Location = New System.Drawing.Point(0, 0)
            Me.ensSelect.Name = "ensSelect"
            Me.ensSelect.Size = New System.Drawing.Size(287, 108)
            Me.ensSelect.Style = Tools.Windows.Forms.EncodingSelector.EncodingSelectorStyle.ListBox
            Me.ensSelect.TabIndex = 0
            '
            'pgrProperties
            '
            Me.pgrProperties.Dock = System.Windows.Forms.DockStyle.Fill
            Me.pgrProperties.Location = New System.Drawing.Point(0, 0)
            Me.pgrProperties.Name = "pgrProperties"
            Me.pgrProperties.SelectedObject = Me.ensSelect
            Me.pgrProperties.Size = New System.Drawing.Size(287, 355)
            Me.pgrProperties.TabIndex = 0
            '
            'frmEncodingSelector
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.ClientSize = New System.Drawing.Size(287, 492)
            Me.Controls.Add(Me.splMain)
            Me.MaximizeBox = False
            Me.MinimizeBox = False
            Me.Name = "frmEncodingSelector"
            Me.ShowInTaskbar = False
            Me.Text = "Testing EncodingSelector"
            Me.splMain.Panel1.ResumeLayout(False)
            Me.splMain.Panel2.ResumeLayout(False)
            Me.splMain.ResumeLayout(False)
            Me.ResumeLayout(False)

        End Sub
        Friend WithEvents ensSelect As Tools.Windows.Forms.EncodingSelector
        Friend WithEvents splMain As System.Windows.Forms.SplitContainer
        Friend WithEvents pgrProperties As System.Windows.Forms.PropertyGrid
        Friend WithEvents lblSelected As System.Windows.Forms.Label
    End Class
End Namespace