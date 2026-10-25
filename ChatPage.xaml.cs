using System.Collections.ObjectModel;using Microsoft.AspNetCore.SignalR.Client;
namespace Assitech.App;
[QueryProperty(nameof(TicketId),"id")]
public partial class ChatPage:ContentPage{readonly Api api;readonly ObservableCollection<MsgDto> items=[];HubConnection? hub;
 public string TicketId{get;set;}="";
 public ChatPage(Api a){InitializeComponent();api=a;Msgs.ItemsSource=items;}
 protected override async void OnAppearing(){base.OnAppearing();
  foreach(var m in await api.Messages(TicketId))items.Add(m);
  hub=new HubConnectionBuilder().WithUrl(Api.Base+"/hubs/chat",o=>o.AccessTokenProvider=()=>Task.FromResult(api.Token)).WithAutomaticReconnect().Build();
  hub.On<MsgDto>("msg",m=>MainThread.BeginInvokeOnMainThread(()=>items.Add(m)));
  await hub.StartAsync();await hub.InvokeAsync("Join",TicketId);}
 protected override async void OnDisappearing(){base.OnDisappearing();if(hub!=null)await hub.DisposeAsync();}
 async void OnSend(object s,EventArgs e){if(string.IsNullOrWhiteSpace(Box.Text)||hub==null)return;await hub.InvokeAsync("Send",TicketId,Box.Text);Box.Text="";}}
