Imports System.Configuration
Imports System.Data
Imports Microsoft.Data.SqlClient

Public Module Db
    Private ReadOnly Property ConnectionString As String
        Get
            Dim setting = ConfigurationManager.ConnectionStrings("OrphansDb")
            If setting Is Nothing OrElse String.IsNullOrWhiteSpace(setting.ConnectionString) Then
                Throw New InvalidOperationException("The OrphansDb connection string is missing from App.config.")
            End If
            Return setting.ConnectionString
        End Get
    End Property

    Public Function OpenConnection() As SqlConnection
        Dim connection As New SqlConnection(ConnectionString)
        connection.Open()
        Return connection
    End Function

    Public Function Query(sql As String, ParamArray parameters() As SqlParameter) As DataTable
        Using connection = OpenConnection(), command As New SqlCommand(sql, connection)
            command.Parameters.AddRange(parameters)
            Using adapter As New SqlDataAdapter(command)
                Dim table As New DataTable()
                adapter.Fill(table)
                Return table
            End Using
        End Using
    End Function

    Public Function Scalar(Of T)(sql As String, ParamArray parameters() As SqlParameter) As T
        Using connection = OpenConnection(), command As New SqlCommand(sql, connection)
            command.Parameters.AddRange(parameters)
            Dim value = command.ExecuteScalar()
            If value Is Nothing OrElse value Is DBNull.Value Then Return Nothing
            Return CType(Convert.ChangeType(value, GetType(T)), T)
        End Using
    End Function

    Public Function Execute(sql As String, ParamArray parameters() As SqlParameter) As Integer
        Using connection = OpenConnection(), command As New SqlCommand(sql, connection)
            command.Parameters.AddRange(parameters)
            Return command.ExecuteNonQuery()
        End Using
    End Function

    Public Function P(name As String, type As SqlDbType, value As Object, Optional size As Integer = 0) As SqlParameter
        Dim parameter As New SqlParameter(name, type)
        If size > 0 Then parameter.Size = size
        parameter.Value = If(value, DBNull.Value)
        Return parameter
    End Function

    Public Sub Audit(actionName As String, entityName As String, entityId As Integer, details As String)
        Const sql = "INSERT INTO AuditLogs(UserID, ActionName, EntityName, EntityID, Details) VALUES(@u,@a,@e,@id,@d)"
        Execute(sql, P("@u", SqlDbType.Int, If(AppSession.UserId = 0, Nothing, AppSession.UserId)),
                P("@a", SqlDbType.NVarChar, actionName, 80), P("@e", SqlDbType.NVarChar, entityName, 80),
                P("@id", SqlDbType.Int, If(entityId = 0, Nothing, entityId)), P("@d", SqlDbType.NVarChar, details, 1000))
    End Sub
End Module
