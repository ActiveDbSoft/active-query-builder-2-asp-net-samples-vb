Imports System.Collections.Generic
Imports System.ComponentModel.DataAnnotations

Namespace Models
	Public Class ExternalLoginConfirmationViewModel
		<Required> _
		<Display(Name := "Email")> _
		Public Property Email() As String
			Get
				Return m_Email
			End Get
			Set
				m_Email = Value
			End Set
		End Property
		Private m_Email As String
	End Class

	Public Class ExternalLoginListViewModel
		Public Property ReturnUrl() As String
			Get
				Return m_ReturnUrl
			End Get
			Set
				m_ReturnUrl = Value
			End Set
		End Property
		Private m_ReturnUrl As String
	End Class

	Public Class SendCodeViewModel
		Public Property SelectedProvider() As String
			Get
				Return m_SelectedProvider
			End Get
			Set
				m_SelectedProvider = Value
			End Set
		End Property
		Private m_SelectedProvider As String
		Public Property Providers() As ICollection(Of System.Web.Mvc.SelectListItem)
			Get
				Return m_Providers
			End Get
			Set
				m_Providers = Value
			End Set
		End Property
		Private m_Providers As ICollection(Of System.Web.Mvc.SelectListItem)
		Public Property ReturnUrl() As String
			Get
				Return m_ReturnUrl
			End Get
			Set
				m_ReturnUrl = Value
			End Set
		End Property
		Private m_ReturnUrl As String
		Public Property RememberMe() As Boolean
			Get
				Return m_RememberMe
			End Get
			Set
				m_RememberMe = Value
			End Set
		End Property
		Private m_RememberMe As Boolean
	End Class

	Public Class VerifyCodeViewModel
		<Required> _
		Public Property Provider() As String
			Get
				Return m_Provider
			End Get
			Set
				m_Provider = Value
			End Set
		End Property
		Private m_Provider As String

		<Required> _
		<Display(Name := "Code")> _
		Public Property Code() As String
			Get
				Return m_Code
			End Get
			Set
				m_Code = Value
			End Set
		End Property
		Private m_Code As String
		Public Property ReturnUrl() As String
			Get
				Return m_ReturnUrl
			End Get
			Set
				m_ReturnUrl = Value
			End Set
		End Property
		Private m_ReturnUrl As String

		<Display(Name := "Remember this browser?")> _
		Public Property RememberBrowser() As Boolean
			Get
				Return m_RememberBrowser
			End Get
			Set
				m_RememberBrowser = Value
			End Set
		End Property
		Private m_RememberBrowser As Boolean

		Public Property RememberMe() As Boolean
			Get
				Return m_RememberMe
			End Get
			Set
				m_RememberMe = Value
			End Set
		End Property
		Private m_RememberMe As Boolean
	End Class

	Public Class ForgotViewModel
		<Required> _
		<Display(Name := "Email")> _
		Public Property Email() As String
			Get
				Return m_Email
			End Get
			Set
				m_Email = Value
			End Set
		End Property
		Private m_Email As String
	End Class

	Public Class LoginViewModel
		<Required> _
		<Display(Name := "Email")> _
		<EmailAddress> _
		Public Property Email() As String
			Get
				Return m_Email
			End Get
			Set
				m_Email = Value
			End Set
		End Property
		Private m_Email As String

		<Required> _
		<DataType(DataType.Password)> _
		<Display(Name := "Password")> _
		Public Property Password() As String
			Get
				Return m_Password
			End Get
			Set
				m_Password = Value
			End Set
		End Property
		Private m_Password As String

		<Display(Name := "Remember me?")> _
		Public Property RememberMe() As Boolean
			Get
				Return m_RememberMe
			End Get
			Set
				m_RememberMe = Value
			End Set
		End Property
		Private m_RememberMe As Boolean
	End Class

	Public Class RegisterViewModel
		<Required> _
		<EmailAddress> _
		<Display(Name := "Email")> _
		Public Property Email() As String
			Get
				Return m_Email
			End Get
			Set
				m_Email = Value
			End Set
		End Property
		Private m_Email As String

		<Required> _
		<StringLength(100, ErrorMessage := "The {0} must be at least {2} characters long.", MinimumLength := 6)> _
		<DataType(DataType.Password)> _
		<Display(Name := "Password")> _
		Public Property Password() As String
			Get
				Return m_Password
			End Get
			Set
				m_Password = Value
			End Set
		End Property
		Private m_Password As String

		<DataType(DataType.Password)> _
		<Display(Name := "Confirm password")> _
		<Compare("Password", ErrorMessage := "The password and confirmation password do not match.")> _
		Public Property ConfirmPassword() As String
			Get
				Return m_ConfirmPassword
			End Get
			Set
				m_ConfirmPassword = Value
			End Set
		End Property
		Private m_ConfirmPassword As String
	End Class

	Public Class ResetPasswordViewModel
		<Required> _
		<EmailAddress> _
		<Display(Name := "Email")> _
		Public Property Email() As String
			Get
				Return m_Email
			End Get
			Set
				m_Email = Value
			End Set
		End Property
		Private m_Email As String

		<Required> _
		<StringLength(100, ErrorMessage := "The {0} must be at least {2} characters long.", MinimumLength := 6)> _
		<DataType(DataType.Password)> _
		<Display(Name := "Password")> _
		Public Property Password() As String
			Get
				Return m_Password
			End Get
			Set
				m_Password = Value
			End Set
		End Property
		Private m_Password As String

		<DataType(DataType.Password)> _
		<Display(Name := "Confirm password")> _
		<Compare("Password", ErrorMessage := "The password and confirmation password do not match.")> _
		Public Property ConfirmPassword() As String
			Get
				Return m_ConfirmPassword
			End Get
			Set
				m_ConfirmPassword = Value
			End Set
		End Property
		Private m_ConfirmPassword As String

		Public Property Code() As String
			Get
				Return m_Code
			End Get
			Set
				m_Code = Value
			End Set
		End Property
		Private m_Code As String
	End Class

	Public Class ForgotPasswordViewModel
		<Required> _
		<EmailAddress> _
		<Display(Name := "Email")> _
		Public Property Email() As String
			Get
				Return m_Email
			End Get
			Set
				m_Email = Value
			End Set
		End Property
		Private m_Email As String
	End Class
End Namespace

