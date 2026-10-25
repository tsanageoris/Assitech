using System.Net.Http.Json;
namespace Assitech.App;
public record TicketDto(string Id,string Sujet,string Description,string Statut,DateTime CreatedAt);
public record MsgDto(int SenderId,string Text);
public class Api{
 public const string Base="http://10.0.2.2:5000"; // émulateur Android -> machine hôte
 readonly HttpClient h=new(){BaseAddress=new(Base)};
 public string? Token{get;private set;}
 record Tok(string token);
 async Task<bool> Auth(string path,object body){var r=await h.PostAsJsonAsync(path,body);if(!r.IsSuccessStatusCode)return false;
  Token=(await r.Content.ReadFromJsonAsync<Tok>())!.token;h.DefaultRequestHeaders.Authorization=new("Bearer",Token);
  await SecureStorage.SetAsync("jwt",Token);return true;}
 public Task<bool> Login(string e,string p)=>Auth("auth/login",new{email=e,password=p});
 public Task<bool> Register(string e,string p,string pr,string n,string t)=>Auth("auth/register",new{email=e,password=p,prenom=pr,nom=n,tel=t});
 public async Task<List<TicketDto>> Tickets()=>await h.GetFromJsonAsync<List<TicketDto>>("tickets")??[];
 public async Task<bool> Create(string s,string d)=>(await h.PostAsJsonAsync("tickets",new{sujet=s,description=d})).IsSuccessStatusCode;
 public async Task<bool> Cancel(string id)=>(await h.PatchAsync($"tickets/{id}/cancel",null)).IsSuccessStatusCode;
 public async Task<List<MsgDto>> Messages(string id)=>await h.GetFromJsonAsync<List<MsgDto>>($"tickets/{id}/messages")??[];
}
