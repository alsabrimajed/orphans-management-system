Imports System.Data

Public Class FrmSponsorEdit
    Inherits Form
    Private ReadOnly recordId As Integer
    Private ReadOnly code As New TextBox(), sponsorName As New TextBox(), phone As New TextBox(), email As New TextBox(), country As New TextBox(), address As New TextBox()
    Private ReadOnly sponsorType As New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDownList}

    Public Sub New(id As Integer)
        recordId = id : Text = If(id = 0, "New sponsor", "Edit sponsor") : ClientSize = New Size(720, 510) : Ui.StyleForm(Me)
        sponsorType.Items.AddRange({"Individual", "Company", "NGO", "Institution"}) : BuildLayout()
        If id > 0 Then LoadRecord()
    End Sub
    Private Sub BuildLayout()
        Dim f As New TableLayoutPanel With {.Dock = DockStyle.Fill, .ColumnCount = 2, .Padding = New Padding(22), .BackColor = Color.White}
        f.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50)) : f.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))
        f.Controls.Add(Ui.Field("Sponsor code *", code), 0, 0) : f.Controls.Add(Ui.Field("Sponsor name *", sponsorName), 1, 0)
        f.Controls.Add(Ui.Field("Type", sponsorType), 0, 1) : f.Controls.Add(Ui.Field("Phone", phone), 1, 1)
        f.Controls.Add(Ui.Field("Email", email), 0, 2) : f.Controls.Add(Ui.Field("Country", country), 1, 2)
        f.Controls.Add(Ui.Field("Address", address), 0, 3) : f.SetColumnSpan(f.GetControlFromPosition(0, 3), 2)
        Dim bar As New FlowLayoutPanel With {.Dock = DockStyle.Bottom, .Height = 68, .FlowDirection = FlowDirection.RightToLeft, .Padding = New Padding(15)}
        Dim save = Ui.Button("Save"), cancel = Ui.Button("Cancel", False) : bar.Controls.AddRange({save, cancel})
        AddHandler save.Click, AddressOf SaveRecord : AddHandler cancel.Click, Sub() DialogResult = DialogResult.Cancel
        Controls.Add(f) : Controls.Add(bar)
    End Sub
    Private Sub LoadRecord()
        Dim r = Db.Query("SELECT * FROM Sponsors WHERE SponsorID=@id", Db.P("@id", SqlDbType.Int, recordId)).Rows(0)
        code.Text = CStr(r("SponsorCode")) : sponsorName.Text = CStr(r("SponsorName")) : sponsorType.Text = CStr(r("SponsorType"))
        phone.Text = S(r("PhoneNumber")) : email.Text = S(r("EmailAddress")) : country.Text = S(r("Country")) : address.Text = S(r("Address"))
    End Sub
    Private Shared Function S(value As Object) As String
        Return If(value Is DBNull.Value, "", CStr(value))
    End Function
    Private Sub SaveRecord(sender As Object, e As EventArgs)
        If String.IsNullOrWhiteSpace(code.Text) OrElse String.IsNullOrWhiteSpace(sponsorName.Text) Then MessageBox.Show("Sponsor code and name are required.", "Validation") : Return
        Dim p = {Db.P("@c", SqlDbType.NVarChar, code.Text.Trim(), 30), Db.P("@n", SqlDbType.NVarChar, sponsorName.Text.Trim(), 150), Db.P("@t", SqlDbType.NVarChar, sponsorType.Text, 50), Db.P("@p", SqlDbType.NVarChar, phone.Text.Trim(), 30), Db.P("@e", SqlDbType.NVarChar, email.Text.Trim(), 150), Db.P("@o", SqlDbType.NVarChar, country.Text.Trim(), 100), Db.P("@a", SqlDbType.NVarChar, address.Text.Trim(), 250)}
        Try
            If recordId = 0 Then
                Dim id = Db.Scalar(Of Integer)("INSERT INTO Sponsors(SponsorCode,SponsorName,SponsorType,PhoneNumber,EmailAddress,Country,Address) VALUES(@c,@n,@t,@p,@e,@o,@a); SELECT CAST(SCOPE_IDENTITY() AS int)", p)
                Db.Audit("CREATE", "Sponsors", id, sponsorName.Text.Trim())
            Else
                Dim list = p.ToList() : list.Add(Db.P("@id", SqlDbType.Int, recordId))
                Db.Execute("UPDATE Sponsors SET SponsorCode=@c,SponsorName=@n,SponsorType=@t,PhoneNumber=@p,EmailAddress=@e,Country=@o,Address=@a WHERE SponsorID=@id", list.ToArray())
                Db.Audit("UPDATE", "Sponsors", recordId, sponsorName.Text.Trim())
            End If
            DialogResult = DialogResult.OK
        Catch ex As Exception
            MessageBox.Show("The sponsor could not be saved: " & ex.Message, "Error")
        End Try
    End Sub
End Class
