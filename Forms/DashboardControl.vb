Public Class DashboardControl
    Inherits UserControl
    Private ReadOnly cards As New FlowLayoutPanel With {.Dock = DockStyle.Top, .Height = 145, .Padding = New Padding(20), .BackColor = Ui.Canvas}
    Private ReadOnly expiring As New DataGridView With {.Dock = DockStyle.Fill, .ReadOnly = True, .AllowUserToAddRows = False, .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, .BackgroundColor = Color.White, .BorderStyle = BorderStyle.None}

    Public Sub New()
        Controls.Add(expiring)
        Controls.Add(New Label With {.Text = "Sponsorships expiring in 30 days", .Dock = DockStyle.Top, .Height = 42, .Padding = New Padding(20, 12, 0, 0), .Font = New Font("Segoe UI Semibold", 12), .ForeColor = Ui.Navy})
        Controls.Add(cards)
        Controls.Add(New Label With {.Text = "Dashboard", .Dock = DockStyle.Top, .Height = 65, .Padding = New Padding(20, 20, 0, 0), .Font = New Font("Segoe UI Semibold", 22), .ForeColor = Ui.Navy})
        AddHandler Load, AddressOf LoadData
    End Sub

    Private Sub LoadData(sender As Object, e As EventArgs)
        Try
            Dim stats = Db.Query("SELECT * FROM vw_DashboardStatistics").Rows(0)
            cards.Controls.Clear()
            AddCard("Registered", CStr(stats("TotalOrphans")))
            AddCard("Active", CStr(stats("ActiveOrphans")))
            AddCard("Sponsored", CStr(stats("SponsoredOrphans")))
            AddCard("Unsponsored", CStr(stats("UnsponsoredOrphans")))
            AddCard("Sponsors", CStr(stats("ActiveSponsors")))
            expiring.DataSource = Db.Query("SELECT SponsorName,FullName,EndDate,SponsorshipAmount FROM vw_ExpiringSponsorships ORDER BY EndDate")
        Catch ex As Exception
            MessageBox.Show("Dashboard data could not be loaded: " & ex.Message, "Database error")
        End Try
    End Sub

    Private Sub AddCard(title As String, value As String)
        Dim panel As New Panel With {.Width = 170, .Height = 92, .BackColor = Color.White, .Margin = New Padding(0, 0, 15, 0), .Padding = New Padding(15)}
        panel.Controls.Add(New Label With {.Text = title, .Dock = DockStyle.Bottom, .Height = 28, .ForeColor = Color.DimGray})
        panel.Controls.Add(New Label With {.Text = value, .Dock = DockStyle.Top, .Height = 42, .Font = New Font("Segoe UI Semibold", 22), .ForeColor = Ui.Teal})
        cards.Controls.Add(panel)
    End Sub
End Class
