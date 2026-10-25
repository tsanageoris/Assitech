namespace Assitech.App;
public partial class TicketsPage:ContentPage{readonly Api api;
 public TicketsPage(Api a){InitializeComponent();api=a;}
 protected override async void OnAppearing(){base.OnAppearing();List.ItemsSource=await api.Tickets();}
 async void OnNew(object s,EventArgs e)=>await Shell.Current.GoToAsync("newticket");
 async void OnSelect(object s,SelectionChangedEventArgs e){if(e.CurrentSelection.FirstOrDefault() is TicketDto t){List.SelectedItem=null;await Shell.Current.GoToAsync($"chat?id={t.Id}");}}}
