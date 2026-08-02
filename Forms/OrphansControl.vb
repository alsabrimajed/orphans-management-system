Imports System.Data

Public Class OrphansControl
    Inherits UserControl
    Private ReadOnly grid As New DataGridView With {.Dock = DockStyle.Fill, .ReadOnly = True, .AllowUserToAddRows = False, .SelectionMode = DataGridViewSelectionMode.FullRowSelect, .MultiSelect = False, .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, .BackgroundColor = Color.White, .BorderStyle = BorderStyle.None}
    Private ReadOnly search As New TextBox With {.Width = 260, .PlaceholderText = "Search code, name or national ID"}

    Public Sub New()
        Dim toolbar As New FlowLayoutPanel With {.Dock = DockStyle.Top, .Height = 62, .Padding = New Padding(20, 10, 20, 8), .BackColor = Color.White}
        toolbar.Controls.Add(search)
        Dim find = Ui.Button("Search", False), add = Ui.Button("New orphan"), edit = Ui.Button("Edit", False), deactivate = Ui.Button("Deactivate", False)
        toolbar.Controls.AddRange({find, add, edit, deactivate})
        AddHandler find.Click, Sub() LoadData()
        AddHandler search.KeyDown, Sub(s, e) If e.KeyCode = Keys.Enter Then LoadData()
        AddHandler add.Click, Sub() EditRecord(0)
        AddHandler edit.Click, Sub() EditRecord(SelectedId())
        AddHandler deactivate.Click, AddressOf DeactivateRecord
        Controls.Add(grid)
        Controls.Add(toolbar)
        Controls.Add(New Label With {.Text = "Orphan records", .Dock = DockStyle.Top, .Height = 65, .Padding = New Padding(20, 20, 0, 0), .Font = New Font("Segoe UI Semibold", 22), .ForeColor = Ui.Navy})
        AddHandler Load, Sub() LoadData()
        AddHandler grid.CellDoubleClick, Sub() EditRecord(SelectedId())
    End Sub

    Private Sub LoadData()
        Const sql = "SELECT OrphanID,OrphanCode,FullName,Gender,DateOfBirth,NationalID,Governorate,District,OrphanStatus FROM Orphans WHERE @q='' OR OrphanCode LIKE '%'+@q+'%' OR FullName LIKE '%'+@q+'%' OR NationalID LIKE '%'+@q+'%' ORDER BY FullName"
        grid.DataSource = Db.Query(sql, Db.P("@q", SqlDbType.NVarChar, search.Text.Trim(), 150))
        If grid.Columns.Contains("OrphanID") Then grid.Columns("OrphanID").Visible = False
    End Sub

    Private Function SelectedId() As Integer
        If grid.CurrentRow Is Nothing Then Return 0
        Return CInt(grid.CurrentRow.Cells("OrphanID").Value)
    End Function

    Private Sub EditRecord(id As Integer)
        Using dialog As New FrmOrphanEdit(id)
            If dialog.ShowDialog() = DialogResult.OK Then LoadData()
        End Using
    End Sub

    Private Sub DeactivateRecord(sender As Object, e As EventArgs)
        Dim id = SelectedId()
        If id = 0 OrElse MessageBox.Show("Deactivate the selected orphan record?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Return
        Db.Execute("UPDATE Orphans SET OrphanStatus='Inactive',ModifiedDate=SYSDATETIME() WHERE OrphanID=@id", Db.P("@id", SqlDbType.Int, id))
        Db.Audit("DEACTIVATE", "Orphans", id, "Record marked inactive")
        LoadData()
    End Sub
End Class
