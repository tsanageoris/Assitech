namespace Assitech.App;
public partial class NewTicketPage:ContentPage{readonly Api api;
 public NewTicketPage(Api a){InitializeComponent();api=a;}
 void OnChanged(object s,TextChangedEventArgs e)=>Go.IsEnabled=!string.IsNullOrWhiteSpace(Sujet.Text)&&!string.IsNullOrWhiteSpace(Desc.Text);
 async void OnCreate(object s,EventArgs e){if(await api.Create(Sujet.Text,Desc.Text))await Shell.Current.GoToAsync("..");else await DisplayAlert("Erreur","Création impossible.","OK");}}
