' 02/14/2007

Imports System.Xml
Imports System.Data.SqlClient
Imports Tva.Security.Application

Namespace Services

    '<WebService(Namespace:="http://troweb/DataServices/")> _
    '<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
    '<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
    Public Class BaseWebService
        Inherits System.Web.Services.WebService
        Implements IBusinessObjectsAdapter

        Private m_boAdapter As IBusinessObjectsAdapter
        Protected DllName As String
        Protected FullyQualifiedClassName As String
        Protected tva_credentials As AuthenticationSoapHeader

        Public Sub New()
            MyBase.New()

        End Sub

        Public Function UserHasAccessToData(ByVal roleName As String) As Boolean

            With tva_credentials
                Return AuthenticateUser(.UserName, .Password, roleName, .Server, .PassThroughAuthentication)
            End With

        End Function

        Public Function BuildMessage() As String Implements IBusinessObjectsAdapter.BuildMessage

            Return m_boAdapter.BuildMessage()
        End Function

        Public Sub Initialize(ByVal ParamArray itemList() As Object) Implements IBusinessObjectsAdapter.Initialize

            Dim a As System.Reflection.Assembly = System.Reflection.Assembly.LoadFrom(Server.MapPath(DllName)) '"C:\Documents and Settings\sjohn\My Documents\Visual Studio 2005\Projects\abc\abc\bin\Release\abc.dll")
            Dim t As System.Type = a.GetType(FullyQualifiedClassName, True)
            m_boAdapter = Activator.CreateInstance(t)
            m_boAdapter.Initialize(itemList)

        End Sub

        Public Function ConvertToXMLDoc(ByVal xmlData As String)

            Dim xmlDoc As XmlDocument = New XmlDocument()
            Dim xmlReader As System.Xml.XmlTextReader
            xmlReader = New System.Xml.XmlTextReader(xmlData, System.Xml.XmlNodeType.Document, Nothing)
            xmlReader.ReadOuterXml()
            xmlDoc.Load(xmlReader)

            Return xmlDoc

        End Function

        Public Shared Function AuthenticateUser(ByVal userID As String, ByVal password As String, ByVal roleName As String, ByVal server As SecurityServer, ByVal passThroughAuthentication As Boolean) As Boolean

            Dim connectionString As String

            With System.Configuration.ConfigurationManager.AppSettings
                Select Case server
                    Case SecurityServer.Development
                        connectionString = .Item("DevelopmentSecurityServer")
                    Case SecurityServer.Acceptance
                        connectionString = .Item("AcceptanceSecurityServer")
                    Case SecurityServer.Production
                        connectionString = .Item("ProductionSecurityServer")
                    Case Else
                        connectionString = .Item("DevelopmentSecurityServer")
                End Select
            End With

            Try
                Dim uObject As User = New User(userID, password, New SqlConnection(connectionString))
                With uObject
                    ' When not using pass through authentication, web service validates user name and password
                    ' otherwise only user name is used to verify user is in role and it becomes the responsibility
                    ' of the owning application to handle user authentication...
                    If Not passThroughAuthentication AndAlso Not .IsAuthenticated() Then Return False
                    If .FindRole(roleName) IsNot Nothing Then Return True
                End With
            Catch ex As Exception
            End Try


            Return False

        End Function
    End Class

End Namespace