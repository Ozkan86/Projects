using System;
using System.Collections.Generic;

namespace Eventify.Models;

public class User
{
    public int Id { get; set; } // User ID

    public string Username { get; set; } = null!; // Kullanıcı adı

    public string Email { get; set; } = null!; // E-posta adresi

    public string Password { get; set; } = null!; // Şifre

    public string Role { get; set; } = null!; // Kullanıcı rolü (Admin, User)
    public  ICollection<Attendance> Attendances { get; } = new List<Attendance>(); //navigasyon özelliği, Attendance sınıfına referans
}
