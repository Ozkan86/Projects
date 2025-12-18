using System;
using System.Collections.Generic;


namespace Eventify.Models;

public class Attendance
{
    public int Id { get; set; } // Attendance ID

    public int EventId { get; set; } //foreign key, Event sınıfına referans

    public int UserId { get; set; } //foreign key, User sınıfına referans

    public bool IsAttending { get; set; } //katılım durumu

    public  Event Event { get; set; } = null!; //navigasyon özelliği, Event sınıfına referans

    public  User User { get; set; } = null!; //navigasyon özelliği, User sınıfına referans
}
