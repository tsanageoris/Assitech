using Microsoft.EntityFrameworkCore;
public class User{public int Id{get;set;}public string Email{get;set;}="";public string Hash{get;set;}="";public string Prenom{get;set;}="";public string Nom{get;set;}="";public string Tel{get;set;}="";public string Role{get;set;}="client";}
public class Ticket{public string Id{get;set;}="";public int UserId{get;set;}public string Sujet{get;set;}="";public string Description{get;set;}="";public string Statut{get;set;}="En cours";public DateTime? DateConstat{get;set;}public DateTime? DateEcheance{get;set;}public DateTime CreatedAt{get;set;}=DateTime.UtcNow;}
public class Message{public int Id{get;set;}public string TicketId{get;set;}="";public int SenderId{get;set;}public string Text{get;set;}="";public DateTime At{get;set;}=DateTime.UtcNow;}
public class Notif{public int Id{get;set;}public int UserId{get;set;}public string Text{get;set;}="";public bool Read{get;set;}public DateTime At{get;set;}=DateTime.UtcNow;}
public class Db(DbContextOptions<Db> o):DbContext(o){public DbSet<User> Users=>Set<User>();public DbSet<Ticket> Tickets=>Set<Ticket>();public DbSet<Message> Messages=>Set<Message>();public DbSet<Notif> Notifs=>Set<Notif>();}
