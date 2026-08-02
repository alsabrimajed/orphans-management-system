Imports System.Data

Public Class FrmOrphanEdit
    Inherits Form
    Private ReadOnly recordId As Integer
    Private ReadOnly code As New TextBox(), fullName As New TextBox(), nationalId As New TextBox(), governorate As New TextBox(), district As New TextBox(), address As New TextBox()
    Private ReadOnly gender As New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDownList}
    Private ReadOnly dob As New DateTimePicker With {.Format = DateTimePickerFormat.Short, .ShowCheckBox = True}

    Public Sub New(id As Integer)
        recordId = id
        Text = If(id = 0, "New orphan", "Edit orphan")
        ClientSize = New Size(720, 560)
        Ui.StyleForm(Me)
        gender.Items.AddRange({"Male", "Female"})
        BuildLayout()
        If id > 0 Then LoadRecord()
    End Sub

    Private Sub BuildLayout()
        Dim fields As New TableLayoutPanel With {.Dock = DockStyle.Fill, .ColumnCount = 2, .RowCount = 5, .Padding = New Padding(22), .BackColor = Color.White}
        fields.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))
        fields.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))
        fields.Controls.Add(Ui.Field("Orphan code *", code), 0, 0)
        fields.Controls.Add(Ui.Field("Full name *", fullName), 1, 0)
        fields.Controls.Add(Ui.Field("Gender", gender), 0, 1)
        fields.Controls.Add(Ui.Field("Date of birth", dob), 1, 1)
        fields.Controls.Add(Ui.Field("National ID", nationalId), 0, 2)
        fields.Controls.Add(Ui.Field("Governorate", governorate), 1, 2)
        fields.Controls.Add(Ui.Field("District", district), 0, 3)
        fields.Controls.Add(Ui.Field("Address", address), 1, 3)
        Dim buttons As New FlowLayoutPanel With {.Dock = DockStyle.Bottom, .Height = 68, .FlowDirection = FlowDirection.RightToLeft, .Padding = New Padding(15)}
        Dim save = Ui.Button("Save"), cancel = Ui.Button("Cancel", False)
        AddHandler save.Click, AddressOf SaveRecord
        AddHandler cancel.Click, Sub() DialogResult = DialogResult.Cancel
        buttons.Controls.AddRange({save, cancel})
        Controls.Add(fields)
        Controls.Add(buttons)
    End Sub

    Private Sub LoadRecord()
        Dim row = Db.Query("SELECT * FROM Orphans WHERE OrphanID=@id", Db.P("@id", SqlDbType.Int, recordId)).Rows(0)
        code.Text = CStr(row("OrphanCode")) : fullName.Text = CStr(row("FullName"))
        gender.Text = If(row("Gender") Is DBNull.Value, "", CStr(row("Gender")))
        If row("DateOfBirth") IsNot DBNull.Value Then dob.Value = CDate(row("DateOfBirth")) Else dob.Checked = False
        nationalId.Text = If(row("NationalID") Is DBNull.Value, "", CStr(row("NationalID")))
        governorate.Text = If(row("Governorate") Is DBNull.Value, "", CStr(row("Governorate")))
        district.Text = If(row("District") Is DBNull.Value, "", CStr(row("District")))
        address.Text = If(row("Address") Is DBNull.Value, "", CStr(row("Address")))
    End Sub

    Private Sub SaveRecord(sender As Object, e As EventArgs)
        If String.IsNullOrWhiteSpace(code.Text) OrElse String.IsNullOrWhiteSpace(fullName.Text) Then MessageBox.Show("Orphan code and full name are required.", "Validation") : Return
        Try
            Dim dobValue As Object = If(dob.Checked, dob.Value.Date, Nothing)
            If recordId = 0 Then
                Const sql = "INSERT INTO Orphans(OrphanCode,FullName,Gender,DateOfBirth,NationalID,Governorate,District,Address,CreatedBy) VALUES(@c,@n,@g,@b,@i,@v,@d,@a,@u); SELECT CAST(SCOPE_IDENTITY() AS int)"
                Dim id = Db.Scalar(Of Integer)(sql, Params(dobValue))
                Db.Audit("CREATE", "Orphans", id, fullName.Text.Trim())
            Else
                Const sql = "UPDATE Orphans SET OrphanCode=@c,FullName=@n,Gender=@g,DateOfBirth=@b,NationalID=@i,Governorate=@v,District=@d,Address=@a,ModifiedDate=SYSDATETIME() WHERE OrphanID=@id"
                Dim list = Params(dobValue).Where(Function(parameter) parameter.ParameterName <> "@u").ToList()
                list.Add(Db.P("@id", SqlDbType.Int, recordId))
                Db.Execute(sql, list.ToArray())
                Db.Audit("UPDATE", "Orphans", recordId, fullName.Text.Trim())
            End If
            DialogResult = DialogResult.OK
        Catch ex As Microsoft.Data.SqlClient.SqlException When ex.Number = 2601 OrElse ex.Number = 2627
            MessageBox.Show("The orphan code or national ID already exists.", "Duplicate record")
        Catch ex As Exception
            MessageBox.Show("The record could not be saved: " & ex.Message, "Error")
        End Try
    End Sub

    Private Function Params(dobValue As Object) As Microsoft.Data.SqlClient.SqlParameter()
        Return {Db.P("@c", SqlDbType.NVarChar, code.Text.Trim(), 30), Db.P("@n", SqlDbType.NVarChar, fullName.Text.Trim(), 150), Db.P("@g", SqlDbType.NVarChar, NullIfEmpty(gender.Text), 10), Db.P("@b", SqlDbType.Date, dobValue), Db.P("@i", SqlDbType.NVarChar, NullIfEmpty(nationalId.Text), 50), Db.P("@v", SqlDbType.NVarChar, NullIfEmpty(governorate.Text), 100), Db.P("@d", SqlDbType.NVarChar, NullIfEmpty(district.Text), 100), Db.P("@a", SqlDbType.NVarChar, NullIfEmpty(address.Text), 250), Db.P("@u", SqlDbType.Int, AppSession.UserId)}
    End Function

    Private Shared Function NullIfEmpty(value As String) As Object
        If String.IsNullOrWhiteSpace(value) Then Return Nothing
        Return value.Trim()
    End Function
End Class
