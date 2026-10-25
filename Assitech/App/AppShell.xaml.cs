namespace Assitech.App;
public partial class AppShell:Shell{public AppShell(){InitializeComponent();Routing.RegisterRoute("newticket",typeof(NewTicketPage));Routing.RegisterRoute("chat",typeof(ChatPage));}}
