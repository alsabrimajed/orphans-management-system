Public Module AppSession
    Public Property UserId As Integer
    Public Property Username As String = String.Empty
    Public Property DisplayName As String = String.Empty
    Public Property RoleName As String = String.Empty

    Public Sub Clear()
        UserId = 0
        Username = String.Empty
        DisplayName = String.Empty
        RoleName = String.Empty
    End Sub
End Module
