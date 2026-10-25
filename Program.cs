using System.IdentityModel.Tokens.Jwt;using System.Security.Claims;using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;using Microsoft.AspNetCore.Authorization;using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;using Microsoft.IdentityModel.Tokens;
var b=WebApplication.CreateBuilder(args);
b.Services.AddDbContext<Db>(o=>o.UseNpgsql(b.Configuration.GetConnectionString("Default")));
var key=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(b.Configuration["Jwt:Key"]!));
b.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o=>{
 o.TokenValidationParameters=new(){IssuerSigningKey=key,ValidateIssuer=false,ValidateAudience=false};
 o.Events=new(){OnMessageReceived=c=>{var t=c.Request.Query["access_token"];if(!string.IsNullOrEmpty(t)&&c.Request.Path.StartsWithSegments("/hubs"))c.Token=t;return Task.CompletedTask;}};});
b.Services.AddAuthorization();b.Services.AddSignalR();
var app=b.Build();app.UseAuthentication();app.UseAuthorization();
using(var s=app.Services.CreateScope())s.ServiceProvider.GetRequiredService<Db>().Database.EnsureCreated();
int Uid(ClaimsPrincipal u)=>int.Parse(u.FindFirstValue(ClaimTypes.NameIdentifier)!);
string Jwt(User u)=>new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(claims:[new(ClaimTypes.NameIdentifier,u.Id.ToString()),new(ClaimTypes.Role,u.Role)],expires:DateTime.UtcNow.AddMinutes(30),signingCredentials:new(key,SecurityAlgorithms.HmacSha256)));
var pw=new System.Text.RegularExpressions.Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$");
app.MapPost("/auth/register",async(Reg r,Db db)=>{
 if(!pw.IsMatch(r.Password))return Results.BadRequest("Mot de passe trop faible");
 if(await db.Users.AnyAsync(u=>u.Email==r.Email))return Results.Conflict("E-mail déjà utilisé");
 var u=new User{Email=r.Email,Hash=BCrypt.Net.BCrypt.HashPassword(r.Password),Prenom=r.Prenom,Nom=r.Nom,Tel=r.Tel};
 db.Users.Add(u);await db.SaveChangesAsync();return Results.Ok(new{token=Jwt(u)});});
app.MapPost("/auth/login",async(Login l,Db db)=>{
 var u=await db.Users.FirstOrDefaultAsync(x=>x.Email==l.Email);
 return u!=null&&BCrypt.Net.BCrypt.Verify(l.Password,u.Hash)?Results.Ok(new{token=Jwt(u)}):Results.Unauthorized();});
var a=app.MapGroup("").RequireAuthorization();
a.MapGet("/me",async(ClaimsPrincipal p,Db db)=>await db.Users.Where(u=>u.Id==Uid(p)).Select(u=>new{u.Email,u.Prenom,u.Nom,u.Tel}).FirstAsync());
a.MapDelete("/me",async(ClaimsPrincipal p,Db db)=>{var id=Uid(p);
 db.Messages.RemoveRange(db.Messages.Where(m=>m.SenderId==id));db.Tickets.RemoveRange(db.Tickets.Where(t=>t.UserId==id));
 db.Notifs.RemoveRange(db.Notifs.Where(n=>n.UserId==id));db.Users.RemoveRange(db.Users.Where(u=>u.Id==id));await db.SaveChangesAsync();return Results.NoContent();});
a.MapGet("/tickets",async(ClaimsPrincipal p,Db db,int page=0)=>await db.Tickets.Where(t=>t.UserId==Uid(p)).OrderByDescending(t=>t.CreatedAt).Skip(page*20).Take(20).ToListAsync());
a.MapGet("/tickets/{id}",async(string id,ClaimsPrincipal p,Db db)=>await db.Tickets.FirstOrDefaultAsync(t=>t.Id==id&&t.UserId==Uid(p)) is{}t?Results.Ok(t):Results.NotFound());
a.MapPost("/tickets",async(NewTicket n,ClaimsPrincipal p,Db db)=>{
 if(string.IsNullOrWhiteSpace(n.Sujet)||string.IsNullOrWhiteSpace(n.Description))return Results.BadRequest();
 var t=new Ticket{Id="KT"+Random.Shared.Next(10000,99999)+(char)('A'+Random.Shared.Next(26))+(char)('A'+Random.Shared.Next(26)),UserId=Uid(p),Sujet=n.Sujet,Description=n.Description,DateConstat=n.DateConstat,DateEcheance=n.DateEcheance};
 db.Tickets.Add(t);db.Notifs.Add(new(){UserId=t.UserId,Text=$"Ticket {t.Id} créé"});await db.SaveChangesAsync();return Results.Created($"/tickets/{t.Id}",t);});
a.MapPatch("/tickets/{id}/cancel",async(string id,ClaimsPrincipal p,Db db)=>{
 var t=await db.Tickets.FirstOrDefaultAsync(x=>x.Id==id&&x.UserId==Uid(p));if(t==null)return Results.NotFound();
 if(t.Statut!="En cours")return Results.BadRequest("Statut non modifiable");t.Statut="Annulé";await db.SaveChangesAsync();return Results.Ok(t);});
a.MapGet("/tickets/{id}/messages",async(string id,ClaimsPrincipal p,Db db)=>await db.Tickets.AnyAsync(t=>t.Id==id&&t.UserId==Uid(p))?Results.Ok(await db.Messages.Where(m=>m.TicketId==id).OrderBy(m=>m.At).ToListAsync()):Results.NotFound());
a.MapGet("/notifications",async(ClaimsPrincipal p,Db db)=>await db.Notifs.Where(n=>n.UserId==Uid(p)).OrderByDescending(n=>n.At).Take(100).ToListAsync());
app.MapHub<ChatHub>("/hubs/chat");
app.Run();
record Reg(string Email,string Password,string Prenom,string Nom,string Tel);record Login(string Email,string Password);record NewTicket(string Sujet,string Description,DateTime? DateConstat,DateTime? DateEcheance);
[Authorize]public class ChatHub(Db db):Hub{
 int Me=>int.Parse(Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!);
 async Task<bool> Allowed(string t)=>Context.User!.IsInRole("expert")||await db.Tickets.AnyAsync(x=>x.Id==t&&x.UserId==Me);
 public async Task Join(string ticketId){if(await Allowed(ticketId))await Groups.AddToGroupAsync(Context.ConnectionId,ticketId);}
 public async Task Send(string ticketId,string text){if(string.IsNullOrWhiteSpace(text)||!await Allowed(ticketId))return;
  var m=new Message{TicketId=ticketId,SenderId=Me,Text=text};db.Messages.Add(m);await db.SaveChangesAsync();await Clients.Group(ticketId).SendAsync("msg",m);}}
