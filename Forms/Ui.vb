Imports System.Drawing
Imports System.Windows.Forms

Public Module Ui
    Public ReadOnly Navy As Color = Color.FromArgb(23, 43, 77)
    Public ReadOnly Teal As Color = Color.FromArgb(30, 136, 122)
    Public ReadOnly Canvas As Color = Color.FromArgb(244, 247, 250)

    Public Sub StyleForm(form As Form)
        form.Font = New Font("Segoe UI", 10.0F)
        form.BackColor = Canvas
        form.StartPosition = FormStartPosition.CenterScreen
    End Sub

    Public Function Button(text As String, Optional primary As Boolean = True) As Button
        Return New Button With {.Text = text, .Height = 38, .AutoSize = True, .Padding = New Padding(14, 0, 14, 0),
            .FlatStyle = FlatStyle.Flat, .BackColor = If(primary, Teal, Color.White),
            .ForeColor = If(primary, Color.White, Navy), .Cursor = Cursors.Hand}
    End Function

    Public Function Field(labelText As String, control As Control) As TableLayoutPanel
        Dim panel As New TableLayoutPanel With {.ColumnCount = 1, .RowCount = 2, .Dock = DockStyle.Fill, .AutoSize = True, .Margin = New Padding(8)}
        panel.Controls.Add(New Label With {.Text = labelText, .AutoSize = True, .ForeColor = Navy}, 0, 0)
        control.Dock = DockStyle.Fill
        control.Height = 32
        panel.Controls.Add(control, 0, 1)
        Return panel
    End Function
End Module
