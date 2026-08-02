Public Class FrmMain
    Inherits Form
    Private ReadOnly content As New Panel With {.Dock = DockStyle.Fill, .BackColor = Ui.Canvas}

    Public Sub New()
        Text = "Orphans Management System"
        WindowState = FormWindowState.Maximized
        MinimumSize = New Size(1050, 680)
        Ui.StyleForm(Me)
        BuildLayout()
        ShowPage(New DashboardControl())
    End Sub

    Private Sub BuildLayout()
        Dim nav As New FlowLayoutPanel With {.Dock = DockStyle.Left, .Width = 220, .FlowDirection = FlowDirection.TopDown, .WrapContents = False, .BackColor = Ui.Navy, .Padding = New Padding(12, 24, 12, 12)}
        nav.Controls.Add(New Label With {.Text = "ORPHANS CARE", .ForeColor = Color.White, .Font = New Font("Segoe UI Semibold", 17), .AutoSize = True, .Margin = New Padding(8, 0, 0, 25)})
        AddNav(nav, "Dashboard", Sub() ShowPage(New DashboardControl()))
        AddNav(nav, "Orphans", Sub() ShowPage(New OrphansControl()))
        AddNav(nav, "Sponsors", Sub() ShowPage(New SponsorsControl()))
        For Each label In {"Guardians & households", "Sponsorships", "Assistance", "Education", "Health", "Social assessments", "Documents", "Reports"}
            AddNav(nav, label, Sub() MessageBox.Show("This module is reserved in the foundation and ready for implementation.", "Module roadmap"))
        Next
        Dim top As New Panel With {.Dock = DockStyle.Top, .Height = 62, .BackColor = Color.White, .Padding = New Padding(22, 18, 22, 0)}
        top.Controls.Add(New Label With {.Text = AppSession.DisplayName & "  •  " & AppSession.RoleName, .Dock = DockStyle.Right, .AutoSize = True, .ForeColor = Ui.Navy})
        Controls.Add(content)
        Controls.Add(top)
        Controls.Add(nav)
    End Sub

    Private Sub AddNav(nav As FlowLayoutPanel, text As String, action As Action)
        Dim button As New Button With {.Text = text, .Width = 190, .Height = 38, .TextAlign = ContentAlignment.MiddleLeft, .FlatStyle = FlatStyle.Flat, .BackColor = Ui.Navy, .ForeColor = Color.White, .Cursor = Cursors.Hand}
        button.FlatAppearance.BorderSize = 0
        AddHandler button.Click, Sub() action()
        nav.Controls.Add(button)
    End Sub

    Private Sub ShowPage(page As Control)
        content.Controls.Clear()
        page.Dock = DockStyle.Fill
        content.Controls.Add(page)
    End Sub
End Class
