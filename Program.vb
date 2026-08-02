Imports System.Windows.Forms

Friend Module Program
    <STAThread>
    Public Sub Main()
        ApplicationConfiguration.Initialize()
        Application.Run(New FrmLogin())
    End Sub
End Module
