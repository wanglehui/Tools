Namespace WindowsT.FormsT
#If Config <= Alpha Then
    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
        Partial Class ThesaurusForm
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
            Me.components = New System.ComponentModel.Container
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ThesaurusForm))
            Me.splVertical = New System.Windows.Forms.SplitContainer
            Me.splAutoComplete = New System.Windows.Forms.SplitContainer
            Me.fraAutoComplete = New System.Windows.Forms.GroupBox
            Me.kweAutoComplete = New Tools.WindowsT.FormsT.KeyWordsEditor
            Me.fraCache = New System.Windows.Forms.GroupBox
            Me.kweCache = New Tools.WindowsT.FormsT.KeyWordsEditor
            Me.fraSynonyms = New System.Windows.Forms.GroupBox
            Me.splSynonyms = New System.Windows.Forms.SplitContainer
            Me.fraKeys = New System.Windows.Forms.GroupBox
            Me.kweKeys = New Tools.WindowsT.FormsT.KeyWordsEditor
            Me.fraValues = New System.Windows.Forms.GroupBox
            Me.kweValues = New Tools.WindowsT.FormsT.KeyWordsEditor
            Me.tlpSelect = New System.Windows.Forms.TableLayoutPanel
            Me.cmdDelSyn = New System.Windows.Forms.Button
            Me.cmdAddSyn = New System.Windows.Forms.Button
            Me.cmbSyn = New System.Windows.Forms.ComboBox
            Me.totToolTip = New System.Windows.Forms.ToolTip(Me.components)
            Me.cmdOpen = New System.Windows.Forms.Button
            Me.cmdSave = New System.Windows.Forms.Button
            Me.tlpButtons = New System.Windows.Forms.TableLayoutPanel
            Me.flpButtons = New System.Windows.Forms.FlowLayoutPanel
            Me.cmdOK = New System.Windows.Forms.Button
            Me.cmdCancel = New System.Windows.Forms.Button
            Me.sfdSave = New System.Windows.Forms.SaveFileDialog
            Me.ofdLoad = New System.Windows.Forms.OpenFileDialog
            Me.splVertical.Panel1.SuspendLayout()
            Me.splVertical.Panel2.SuspendLayout()
            Me.splVertical.SuspendLayout()
            Me.splAutoComplete.Panel1.SuspendLayout()
            Me.splAutoComplete.Panel2.SuspendLayout()
            Me.splAutoComplete.SuspendLayout()
            Me.fraAutoComplete.SuspendLayout()
            Me.fraCache.SuspendLayout()
            Me.fraSynonyms.SuspendLayout()
            Me.splSynonyms.Panel1.SuspendLayout()
            Me.splSynonyms.Panel2.SuspendLayout()
            Me.splSynonyms.SuspendLayout()
            Me.fraKeys.SuspendLayout()
            Me.fraValues.SuspendLayout()
            Me.tlpSelect.SuspendLayout()
            Me.tlpButtons.SuspendLayout()
            Me.flpButtons.SuspendLayout()
            Me.SuspendLayout()
            '
            'splVertical
            '
            resources.ApplyResources(Me.splVertical, "splVertical")
            Me.splVertical.Name = "splVertical"
            '
            'splVertical.Panel1
            '
            Me.splVertical.Panel1.Controls.Add(Me.splAutoComplete)
            '
            'splVertical.Panel2
            '
            Me.splVertical.Panel2.Controls.Add(Me.fraSynonyms)
            '
            'splAutoComplete
            '
            resources.ApplyResources(Me.splAutoComplete, "splAutoComplete")
            Me.splAutoComplete.Name = "splAutoComplete"
            '
            'splAutoComplete.Panel1
            '
            Me.splAutoComplete.Panel1.Controls.Add(Me.fraAutoComplete)
            '
            'splAutoComplete.Panel2
            '
            Me.splAutoComplete.Panel2.Controls.Add(Me.fraCache)
            '
            'fraAutoComplete
            '
            Me.fraAutoComplete.Controls.Add(Me.kweAutoComplete)
            resources.ApplyResources(Me.fraAutoComplete, "fraAutoComplete")
            Me.fraAutoComplete.Name = "fraAutoComplete"
            Me.fraAutoComplete.TabStop = False
            '
            'kweAutoComplete
            '
            Me.kweAutoComplete.AutomaticsLists_Designer = True
            resources.ApplyResources(Me.kweAutoComplete, "kweAutoComplete")
            Me.kweAutoComplete.MergeButtonState = Tools.WindowsT.FormsT.UtilitiesT.ControlState.Hidden
            Me.kweAutoComplete.Name = "kweAutoComplete"
            Me.kweAutoComplete.StatusState = Tools.WindowsT.FormsT.UtilitiesT.ControlState.Hidden
            Me.kweAutoComplete.ThesaurusButtonState = Tools.WindowsT.FormsT.UtilitiesT.ControlState.Hidden
            '
            'fraCache
            '
            Me.fraCache.Controls.Add(Me.kweCache)
            resources.ApplyResources(Me.fraCache, "fraCache")
            Me.fraCache.Name = "fraCache"
            Me.fraCache.TabStop = False
            '
            'kweCache
            '
            Me.kweCache.AutomaticsLists_Designer = True
            resources.ApplyResources(Me.kweCache, "kweCache")
            Me.kweCache.MergeButtonState = Tools.WindowsT.FormsT.UtilitiesT.ControlState.Hidden
            Me.kweCache.Name = "kweCache"
            Me.kweCache.StatusState = Tools.WindowsT.FormsT.UtilitiesT.ControlState.Hidden
            Me.kweCache.ThesaurusButtonState = Tools.WindowsT.FormsT.UtilitiesT.ControlState.Hidden
            '
            'fraSynonyms
            '
            Me.fraSynonyms.Controls.Add(Me.splSynonyms)
            Me.fraSynonyms.Controls.Add(Me.tlpSelect)
            resources.ApplyResources(Me.fraSynonyms, "fraSynonyms")
            Me.fraSynonyms.Name = "fraSynonyms"
            Me.fraSynonyms.TabStop = False
            '
            'splSynonyms
            '
            resources.ApplyResources(Me.splSynonyms, "splSynonyms")
            Me.splSynonyms.Name = "splSynonyms"
            '
            'splSynonyms.Panel1
            '
            Me.splSynonyms.Panel1.Controls.Add(Me.fraKeys)
            '
            'splSynonyms.Panel2
            '
            Me.splSynonyms.Panel2.Controls.Add(Me.fraValues)
            '
            'fraKeys
            '
            Me.fraKeys.Controls.Add(Me.kweKeys)
            resources.ApplyResources(Me.fraKeys, "fraKeys")
            Me.fraKeys.Name = "fraKeys"
            Me.fraKeys.TabStop = False
            '
            'kweKeys
            '
            Me.kweKeys.AutomaticsLists_Designer = True
            resources.ApplyResources(Me.kweKeys, "kweKeys")
            Me.kweKeys.MergeButtonState = Tools.WindowsT.FormsT.UtilitiesT.ControlState.Hidden
            Me.kweKeys.Name = "kweKeys"
            Me.kweKeys.StatusState = Tools.WindowsT.FormsT.UtilitiesT.ControlState.Hidden
            Me.kweKeys.ThesaurusButtonState = Tools.WindowsT.FormsT.UtilitiesT.ControlState.Hidden
            '
            'fraValues
            '
            Me.fraValues.Controls.Add(Me.kweValues)
            resources.ApplyResources(Me.fraValues, "fraValues")
            Me.fraValues.Name = "fraValues"
            Me.fraValues.TabStop = False
            '
            'kweValues
            '
            Me.kweValues.AutomaticsLists_Designer = True
            resources.ApplyResources(Me.kweValues, "kweValues")
            Me.kweValues.MergeButtonState = Tools.WindowsT.FormsT.UtilitiesT.ControlState.Hidden
            Me.kweValues.Name = "kweValues"
            Me.kweValues.StatusState = Tools.WindowsT.FormsT.UtilitiesT.ControlState.Hidden
            Me.kweValues.ThesaurusButtonState = Tools.WindowsT.FormsT.UtilitiesT.ControlState.Hidden
            '
            'tlpSelect
            '
            resources.ApplyResources(Me.tlpSelect, "tlpSelect")
            Me.tlpSelect.Controls.Add(Me.cmdDelSyn, 2, 0)
            Me.tlpSelect.Controls.Add(Me.cmdAddSyn, 1, 0)
            Me.tlpSelect.Controls.Add(Me.cmbSyn, 0, 0)
            Me.tlpSelect.Name = "tlpSelect"
            '
            'cmdDelSyn
            '
            resources.ApplyResources(Me.cmdDelSyn, "cmdDelSyn")
            Me.cmdDelSyn.Image = Global.Tools.My.Resources.Resources.Delete
            Me.cmdDelSyn.Name = "cmdDelSyn"
            Me.totToolTip.SetToolTip(Me.cmdDelSyn, resources.GetString("cmdDelSyn.ToolTip"))
            Me.cmdDelSyn.UseVisualStyleBackColor = True
            '
            'cmdAddSyn
            '
            resources.ApplyResources(Me.cmdAddSyn, "cmdAddSyn")
            Me.cmdAddSyn.Image = Global.Tools.My.Resources.Resources.Plus
            Me.cmdAddSyn.Name = "cmdAddSyn"
            Me.totToolTip.SetToolTip(Me.cmdAddSyn, resources.GetString("cmdAddSyn.ToolTip"))
            Me.cmdAddSyn.UseVisualStyleBackColor = True
            '
            'cmbSyn
            '
            resources.ApplyResources(Me.cmbSyn, "cmbSyn")
            Me.cmbSyn.FormattingEnabled = True
            Me.cmbSyn.Name = "cmbSyn"
            '
            'cmdOpen
            '
            resources.ApplyResources(Me.cmdOpen, "cmdOpen")
            Me.cmdOpen.FlatAppearance.BorderSize = 0
            Me.cmdOpen.Image = Global.Tools.My.Resources.Resources.Open
            Me.cmdOpen.Name = "cmdOpen"
            Me.totToolTip.SetToolTip(Me.cmdOpen, resources.GetString("cmdOpen.ToolTip"))
            Me.cmdOpen.UseVisualStyleBackColor = True
            '
            'cmdSave
            '
            resources.ApplyResources(Me.cmdSave, "cmdSave")
            Me.cmdSave.FlatAppearance.BorderSize = 0
            Me.cmdSave.Image = Global.Tools.My.Resources.Resources.Save
            Me.cmdSave.Name = "cmdSave"
            Me.totToolTip.SetToolTip(Me.cmdSave, resources.GetString("cmdSave.ToolTip"))
            Me.cmdSave.UseVisualStyleBackColor = True
            '
            'tlpButtons
            '
            resources.ApplyResources(Me.tlpButtons, "tlpButtons")
            Me.tlpButtons.Controls.Add(Me.flpButtons, 0, 0)
            Me.tlpButtons.Name = "tlpButtons"
            '
            'flpButtons
            '
            resources.ApplyResources(Me.flpButtons, "flpButtons")
            Me.flpButtons.Controls.Add(Me.cmdOK)
            Me.flpButtons.Controls.Add(Me.cmdCancel)
            Me.flpButtons.Controls.Add(Me.cmdOpen)
            Me.flpButtons.Controls.Add(Me.cmdSave)
            Me.flpButtons.Name = "flpButtons"
            '
            'cmdOK
            '
            resources.ApplyResources(Me.cmdOK, "cmdOK")
            Me.cmdOK.DialogResult = System.Windows.Forms.DialogResult.OK
            Me.cmdOK.Name = "cmdOK"
            Me.cmdOK.UseVisualStyleBackColor = True
            '
            'cmdCancel
            '
            resources.ApplyResources(Me.cmdCancel, "cmdCancel")
            Me.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
            Me.cmdCancel.Name = "cmdCancel"
            Me.cmdCancel.UseVisualStyleBackColor = True
            '
            'sfdSave
            '
            Me.sfdSave.DefaultExt = "xml"
            resources.ApplyResources(Me.sfdSave, "sfdSave")
            '
            'ofdLoad
            '
            Me.ofdLoad.DefaultExt = "xml"
            resources.ApplyResources(Me.ofdLoad, "ofdLoad")
            '
            'ThesaurusForm
            '
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.CancelButton = Me.cmdCancel
            Me.ControlBox = False
            Me.Controls.Add(Me.splVertical)
            Me.Controls.Add(Me.tlpButtons)
            Me.KeyPreview = True
            Me.MaximizeBox = False
            Me.MinimizeBox = False
            Me.Name = "ThesaurusForm"
            Me.ShowInTaskbar = False
            Me.splVertical.Panel1.ResumeLayout(False)
            Me.splVertical.Panel2.ResumeLayout(False)
            Me.splVertical.ResumeLayout(False)
            Me.splAutoComplete.Panel1.ResumeLayout(False)
            Me.splAutoComplete.Panel2.ResumeLayout(False)
            Me.splAutoComplete.ResumeLayout(False)
            Me.fraAutoComplete.ResumeLayout(False)
            Me.fraCache.ResumeLayout(False)
            Me.fraSynonyms.ResumeLayout(False)
            Me.fraSynonyms.PerformLayout()
            Me.splSynonyms.Panel1.ResumeLayout(False)
            Me.splSynonyms.Panel2.ResumeLayout(False)
            Me.splSynonyms.ResumeLayout(False)
            Me.fraKeys.ResumeLayout(False)
            Me.fraValues.ResumeLayout(False)
            Me.tlpSelect.ResumeLayout(False)
            Me.tlpSelect.PerformLayout()
            Me.tlpButtons.ResumeLayout(False)
            Me.tlpButtons.PerformLayout()
            Me.flpButtons.ResumeLayout(False)
            Me.flpButtons.PerformLayout()
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub
        Friend WithEvents splVertical As System.Windows.Forms.SplitContainer
        Friend WithEvents splAutoComplete As System.Windows.Forms.SplitContainer
        Friend WithEvents kweAutoComplete As Tools.WindowsT.FormsT.KeyWordsEditor
        Friend WithEvents fraAutoComplete As System.Windows.Forms.GroupBox
        Friend WithEvents fraCache As System.Windows.Forms.GroupBox
        Friend WithEvents fraSynonyms As System.Windows.Forms.GroupBox
        Friend WithEvents splSynonyms As System.Windows.Forms.SplitContainer
        Friend WithEvents cmbSyn As System.Windows.Forms.ComboBox
        Friend WithEvents cmdAddSyn As System.Windows.Forms.Button
        Friend WithEvents cmdDelSyn As System.Windows.Forms.Button
        Friend WithEvents fraKeys As System.Windows.Forms.GroupBox
        Friend WithEvents fraValues As System.Windows.Forms.GroupBox
        Friend WithEvents tlpSelect As System.Windows.Forms.TableLayoutPanel
        Friend WithEvents kweKeys As Tools.WindowsT.FormsT.KeyWordsEditor
        Friend WithEvents kweValues As Tools.WindowsT.FormsT.KeyWordsEditor
        Friend WithEvents totToolTip As System.Windows.Forms.ToolTip
        Friend WithEvents tlpButtons As System.Windows.Forms.TableLayoutPanel
        Friend WithEvents flpButtons As System.Windows.Forms.FlowLayoutPanel
        Friend WithEvents cmdOK As System.Windows.Forms.Button
        Friend WithEvents cmdCancel As System.Windows.Forms.Button
        Friend WithEvents cmdSave As System.Windows.Forms.Button
        Friend WithEvents cmdOpen As System.Windows.Forms.Button
        Friend WithEvents sfdSave As System.Windows.Forms.SaveFileDialog
        Friend WithEvents ofdLoad As System.Windows.Forms.OpenFileDialog
        Friend WithEvents kweCache As Tools.WindowsT.FormsT.KeyWordsEditor
    End Class
#End If
End Namespace
