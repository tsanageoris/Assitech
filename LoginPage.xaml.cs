namespace Assitech.App;
public partial class LoginPage:ContentPage{readonly Api api;
 public LoginPage(Api a){InitializeComponent();api=a;}
 async void OnLogin(object s,EventArgs e){if(await api.Login(Email.Text??"",Pwd.Text??""))await Shell.Current.GoToAsync("//main");else Err.Text="Identifiants invalides.";}}
