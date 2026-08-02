Imports Microsoft.Data.SqlClient

Public Class FrmLogin
    Inherits Form
    Private ReadOnly txtUser As New TextBox With {.Text = "admin"}
    Private ReadOnly txtPassword As New TextBox With {.UseSystemPasswordChar = True, .Text = "Admin@123"}
    Private ReadOnly lblStatus As New Label With {.AutoSize = True, .ForeColor = Color.Firebrick}

    Public Sub New()
        Text = "Orphans Management System — Sign in"
        ClientSize = New Size(480, 430)
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        Ui.StyleForm(Me)
        BuildLayout()
    End Sub

    Private Sub BuildLayout()
        Dim card As New TableLayoutPanel With {.Width = 360, .Height = 340, .ColumnCount = 1, .RowCount = 7, .BackColor = Color.White, .Padding = New Padding(30), .Location = New Point(60, 42)}
        card.Controls.Add(New Label With {.Text = "ORPHANS CARE", .Font = New Font("Segoe UI Semibold", 20), .ForeColor = Ui.Navy, .AutoSize = True})
        card.Controls.Add(New Label With {.Text = "Secure management portal", .ForeColor = Color.DimGray, .AutoSize = True})
        card.Controls.Add(Ui.Field("Username", txtUser))
        card.Controls.Add(Ui.Field("Password", txtPassword))
        card.Controls.Add(lblStatus)
        Dim login = Ui.Button("Sign in")
        login.Dock = DockStyle.Fill
        AddHandler login.Click, AddressOf SignIn
        card.Controls.Add(login)
        card.Controls.Add(New Label With {.Text = "Default evaluation account: admin / Admin@123", .ForeColor = Color.Gray, .AutoSize = True})
        Controls.Add(card)
        AcceptButton = login
    End Sub

    Private Sub SignIn(sender As Object, e As EventArgs)
        lblStatus.Text = String.Empty
        Try
            Const sql = "SELECT TOP 1 u.UserID,u.Username,u.DisplayName,r.RoleName FROM Users u JOIN Roles r ON r.RoleID=u.RoleID WHERE u.Username=@n AND u.PasswordHash=CONVERT(varchar(64),HASHBYTES('SHA2_256',@p),2) AND u.IsActive=1"
            Dim table = Db.Query(sql, Db.P("@n", SqlDbType.NVarChar, txtUser.Text.Trim(), 50), Db.P("@p", SqlDbType.NVarChar, txtPassword.Text, 200))
            If table.Rows.Count = 0 Then
                lblStatus.Text = "Invalid username or password."
                Return
            End If
            Dim row = table.Rows(0)
            AppSession.UserId = CInt(row("UserID"))
            AppSession.Username = CStr(row("Username"))
            AppSession.DisplayName = CStr(row("DisplayName"))
            AppSession.RoleName = CStr(row("RoleName"))
            Db.Audit("LOGIN", "Users", AppSession.UserId, "Successful sign-in")
            Hide()
            Using main As New FrmMain()
                main.ShowDialog()
            End Using
            AppSession.Clear()
            Show()
            txtPassword.SelectAll()
            txtPassword.Focus()
        Catch ex As Exception
            lblStatus.Text = "Cannot connect to the database. Run the setup script and check App.config."
        End Try
    End Sub
End Class
