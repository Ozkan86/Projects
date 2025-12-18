using System;
using System.Collections.Generic;


namespace Eventify.Models;

public class Event
{
    public int Id { get; set; } // Event ID

    public string Name { get; set; } = null!; // Event name

    public string Location { get; set; } = null!; // Event location

    public DateTime Date { get; set; } // olay tarihi

    public  ICollection<Attendance> Attendances { get; set; } = new List<Attendance>(); //navigasyon özelliği, Attendance sınıfına referans

}
