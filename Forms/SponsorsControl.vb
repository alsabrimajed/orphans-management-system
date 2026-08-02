Imports System.Data

Public Class SponsorsControl
    Inherits UserControl
    Private ReadOnly grid As New DataGridView With {.Dock = DockStyle.Fill, .ReadOnly = True, .AllowUserToAddRows = False, .SelectionMode = DataGridViewSelectionMode.FullRowSelect, .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, .BackgroundColor = Color.White, .BorderStyle = BorderStyle.None}
    Private ReadOnly search As New TextBox With {.Width = 260, .PlaceholderText = "Search sponsors"}

    Public Sub New()
        Dim bar As New FlowLayoutPanel With {.Dock = DockStyle.Top, .Height = 62, .Padding = New Padding(20, 10, 20, 8), .BackColor = Color.White}
        Dim find = Ui.Button("Search", False), add = Ui.Button("New sponsor"), edit = Ui.Button("Edit", False), toggle = Ui.Button("Toggle active", False)
        bar.Controls.AddRange({search, find, add, edit, toggle})
        AddHandler find.Click, Sub() LoadData()
        AddHandler add.Click, Sub() EditRecord(0)
        AddHandler edit.Click, Sub() EditRecord(SelectedId())
        AddHandler toggle.Click, Sub() ToggleActive()
        Controls.Add(grid) : Controls.Add(bar)
        Controls.Add(New Label With {.Text = "Sponsors", .Dock = DockStyle.Top, .Height = 65, .Padding = New Padding(20, 20, 0, 0), .Font = New Font("Segoe UI Semibold", 22), .ForeColor = Ui.Navy})
        AddHandler Load, Sub() LoadData()
    End Sub

    Private Sub LoadData()
        grid.DataSource = Db.Query("SELECT SponsorID,SponsorCode,SponsorName,SponsorType,PhoneNumber,EmailAddress,Country,IsActive FROM Sponsors WHERE @q='' OR SponsorCode LIKE '%'+@q+'%' OR SponsorName LIKE '%'+@q+'%' ORDER BY SponsorName", Db.P("@q", SqlDbType.NVarChar, search.Text.Trim(), 150))
        If grid.Columns.Contains("SponsorID") Then grid.Columns("SponsorID").Visible = False
    End Sub
    Private Function SelectedId() As Integer
        If grid.CurrentRow Is Nothing Then Return 0 Else Return CInt(grid.CurrentRow.Cells("SponsorID").Value)
    End Function
    Private Sub EditRecord(id As Integer)
        Using dialog As New FrmSponsorEdit(id)
            If dialog.ShowDialog() = DialogResult.OK Then LoadData()
        End Using
    End Sub
    Private Sub ToggleActive()
        Dim id = SelectedId() : If id = 0 Then Return
        Db.Execute("UPDATE Sponsors SET IsActive=IIF(IsActive=1,0,1) WHERE SponsorID=@id", Db.P("@id", SqlDbType.Int, id))
        Db.Audit("TOGGLE_ACTIVE", "Sponsors", id, "Active status changed") : LoadData()
    End Sub
End Class
