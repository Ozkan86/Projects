using Microsoft.EntityFrameworkCore;
using Eventify.Data;
using Eventify.Models;
using Eventify.DTOs;
using Eventify.Helpers;

namespace Eventify.Services
{
    // Attendance verilerini yöneten servis sınıfı
    public class AttendanceService : IAttendanceService
    {
        private readonly AppDbContext _context;

        public AttendanceService(AppDbContext context)
        {
            _context = context; // DbContext bağımlılığı enjekte edilir
        }

        public async Task<PagedResultDto<SimpleAttendanceDto>> GetAllAttendancesPagedAsync(string? sortBy, bool isDescending, int page, int pageSize)
        {
            var query = _context.Attendances //eager loading ile ilişkili olduğu event ve user tabloları bağlanıyor
                .Include(a => a.Event)
                .Include(a => a.User)
                .AsQueryable(); // IQueryable ile sorgu oluşturulur, böylece veritabanına sorgu gönderilene kadar sorgu çalıştırılmaz

            query = sortBy?.ToLower() switch // Dinamik sıralama işlemi yapılır
            {
                "id" => isDescending ? query.OrderByDescending(a => a.Id) : query.OrderBy(a => a.Id), // ID'ye göre isDescanding true ise azalan, false ise artan sıralama yapılır
                "event" => isDescending ? query.OrderByDescending(a => a.Event.Name) : query.OrderBy(a => a.Event.Name), // Etkinliğe göre sıralama isDescanding true ise azalan, false ise artan sıralama yapılır
                "user" => isDescending ? query.OrderByDescending(a => a.User.Username) : query.OrderBy(a => a.User.Username), // Kullanıcıya göre sıralama isDescanding true ise azalan, false ise artan sıralama yapılır
                "isattending" => isDescending ? query.OrderByDescending(a => a.IsAttending) : query.OrderBy(a => a.IsAttending), // Katılım durumuna göre sıralama isDescanding true ise azalan, false ise artan sıralama yapılır
                _ => query.OrderBy(a => a.Id) // Varsayılan sıralama (sortBy boşsa Id'ye göre artan sırala)
            };

            var totalCount = await query.CountAsync(); // Attendances tablosundaki toplam kayıt sayısını alır

            var attendances = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(); // Sayfalama işlemi yapılır, page - 1 sayfa sayısı * pageSize kadar kayıt atlanır ve pageSize kadar kayıt alınır

            var items = attendances.Select(attendance => new SimpleAttendanceDto // attendances projeksiyon ile SimpleAttendanceDto'ya dönüştürülür (map işlemi yapılır)
            {
                Id = attendance.Id, // Attendance ID'si simpleAttendanceDto'daki id'ye aktarılır
                IsAttending = attendance.IsAttending, // Katılım durumu simpleAttendanceDto'daki IsAttending'e aktarılır
                Event = new SimpleEventDto // İlişkili Event bilgileri SimpleEventDto'ya aktarılır
                {
                    Id = attendance.Event.Id, // Event ID'si simpleEventDto'daki id'ye aktarılır
                    Name = attendance.Event.Name, // Etkinlik adı simpleEventDto'daki Name'e aktarılır
                    Location = attendance.Event.Location, // Etkinlik konumu simpleEventDto'daki Location'a aktarılır
                    Date = attendance.Event.Date // Etkinlik tarihi simpleEventDto'daki Date'e aktarılır
                },
                User = new SimpleUserDto // İlişkili User bilgileri SimpleUserDto'ya aktarılır
                {
                    Id = attendance.User.Id, // User ID'si simpleUserDto'daki id'ye aktarılır 
                    Username = attendance.User.Username, // Kullanıcı adı simpleUserDto'daki Username'e aktarılır
                    Email = attendance.User.Email, // Kullanıcı e-postası simpleUserDto'daki Email'e aktarılır 
                    Role = attendance.User.Role // Kullanıcı rolü simpleUserDto'daki Role'a aktarılır
                },
            });

            return new PagedResultDto<SimpleAttendanceDto> // PagedResultDto ile sonuç döndürülür
            {
                Items = items.ToList(), // Listeye dönüştürülür
                TotalCount = totalCount, // Toplam kayıt sayısı
                Page = page,   // Geçerli sayfa numarası
                PageSize = pageSize, // Sayfa başına kayıt sayısı
                HasMore = page * pageSize < totalCount // Daha fazla kayıt var mı? Kontrol edilir, eğer page * pageSize toplam kayıt sayısından küçükse daha fazla kayıt var demektir
            };
        }
        

        public async Task<IEnumerable<SimpleAttendanceDto>> GetAllAttendancesAsync(string? sortBy, bool isDescending, int page, int pageSize)
        {
            var query = _context.Attendances
                .Include(a => a.Event) //eager loading ile ilişkili olduğu event tablosu bağlanıyor
                .Include(a => a.User) //eager loading ile ilişkili olduğu user tablosu bağlanıyor
                .AsQueryable();

            // Dinamik sıralama
            query = sortBy?.ToLower() switch
            {
                "id" => isDescending ? query.OrderByDescending(a => a.Id) : query.OrderBy(a => a.Id), // ID'ye göre sıralama
                "event" => isDescending ? query.OrderByDescending(a => a.Event.Name) : query.OrderBy(a => a.Event.Name), // Etkinliğe göre sıralama
                "user" => isDescending ? query.OrderByDescending(a => a.User.Username) : query.OrderBy(a => a.User.Username), // Kullanıcıya göre sıralama
                "isattending" => isDescending ? query.OrderByDescending(a => a.IsAttending) : query.OrderBy(a => a.IsAttending), // Katılım durumuna göre sıralama
                _ => query.OrderBy(a => a.Id) // Varsayılan sıralama (sortBy boşsa Id'ye göre sırala)
            };

            // Sayfalama
            query = query.Skip((page - 1) * pageSize).Take(pageSize); // geride kalan sayfa sayısı * sayfa boyutu kadar kayıtı atla ve son noktadan itibaren sayfa boyutu kadar kaydı al

            var attendances = await query.ToListAsync();

            return attendances.Select(a => new SimpleAttendanceDto //projeksiyon ile SimpleAttendanceDto'ya özellikleri aktar (map)
            {
                Id = a.Id,
                IsAttending = a.IsAttending,
                Event = new SimpleEventDto
                {
                    Id = a.Event.Id,
                    Name = a.Event.Name,
                    Location = a.Event.Location,
                    Date = a.Event.Date
                },
                User = new SimpleUserDto
                {
                    Id = a.User.Id,
                    Username = a.User.Username,
                    Email = a.User.Email,
                    Role = a.User.Role
                }
            }); // yeni eklediğimiz kaydı yanıt olarak döndürüyoruz
        }


        public async Task<AttendanceDetailsDto> CreateAttendanceAsync(CreateAttendanceDto createDto)
        {
           
            var attendance = new Attendance //CreateAttendanceDto'dan gelen verilerle yeni bir Attendance nesnesi oluştur
            {
                EventId = createDto.EventId,
                UserId = createDto.UserId,
                IsAttending = createDto.IsAttending
            };

            // Attendance nesnesine ait Event ve User bilgilerini yükle
            _context.Attendances.Add(attendance); // Yeni katılımı ekle
            await _context.SaveChangesAsync(); // Veritabanına kaydet

            // Event ve User nesnelerinin yüklendiğinden emin ol
            var createdAttendance = await _context.Attendances //eagerloaing
                .Include(a => a.Event)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == attendance.Id); // FirstOrDefaultAsync ile id'ye göre eşleşen ilk kaydı al eşleşen kayıt yoksa default olanı döndür

            if (createdAttendance == null || createdAttendance.Event == null || createdAttendance.User == null) // Eğer katılım, etkinlik veya kullanıcı bulunamazsa hata fırlat
            {
                throw new Exception("Attendance, Event, or User not found.");
            }

            // AttendanceDetailsDto nesnesini oluştur
            var attendanceDetailsDto = new AttendanceDetailsDto
            {
                Id = createdAttendance.Id,
                EventId = createdAttendance.EventId,
                UserId = createdAttendance.UserId,
                IsAttending = createdAttendance.IsAttending,
                Event = new EventDto
                {
                    Id = createdAttendance.Event.Id,
                    Name = createdAttendance.Event.Name,
                    Location = createdAttendance.Event.Location,
                    Date = createdAttendance.Event.Date
                },
                User = new SimpleUserDto
                {
                    Id = createdAttendance.User.Id,
                    Username = createdAttendance.User.Username,
                    Email = createdAttendance.User.Email,
                    Role = createdAttendance.User.Role // Kullanıcının rolünü ekle
                }
            };

            return attendanceDetailsDto;
        }

        public async Task<SimpleAttendanceDto?> GetAttendanceByIdAsync(int attendanceId)
        {
            var attendance = await _context.Attendances //eager loading ile attendance ve ilişkili olduğu diğer tablo bağlanıyor
                .Include(a => a.Event)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == attendanceId); //FirstOrDefaultAsync ile id'ye göre eşleşen ilk kaydı al eşleşen kayıt yoksa default olanı döndür

            if (attendance == null) return null; // attendance boşsa geriye null döndür

            return new SimpleAttendanceDto
            {
                Id = attendance.Id,
                IsAttending = attendance.IsAttending,
                Event = new SimpleEventDto
                {
                    Id = attendance.Event.Id,
                    Name = attendance.Event.Name,
                    Location = attendance.Event.Location,
                    Date = attendance.Event.Date
                },
                User = new SimpleUserDto
                {
                    Id = attendance.User.Id,
                    Username = attendance.User.Username,
                    Email = attendance.User.Email,
                    Role = attendance.User.Role // Kullanıcının rolünü ekle
                }
            };
        }
        public async Task<AttendanceDetailsDto> UpdateAttendanceAsync(int id, UpdateAttendanceDto updateDto)
        {
            
            var attendance = await _context.Attendances //eager loading ile attendance ve ilişkili olduğu diğer tablolar bağlanıyor
                .Include(a => a.Event)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id); //FirstOrDefaultAsync ile id'ye göre eşleşen ilk kaydı al eşleşen kayıt yoksa default olanı döndür

          
            if (attendance == null) // Eğer attendance bulunamazsa, null döndürüyoruz
            {
                return null;
            }

            
            attendance.IsAttending = updateDto.IsAttending; // Gelen dto'dan alınan verilerle attendance'ı güncelliyoruz
            attendance.EventId = updateDto.EventId; // Etkinlik ID'sini güncelliyoruz
            attendance.UserId = updateDto.UserId; // Kullanıcı ID'sini güncelliyoruz

            await _context.SaveChangesAsync(); // Veritabanında değişiklikleri kaydediyoruz

            attendance = await _context.Attendances //Güncel attendance allllllll
                .Include(a => a.Event)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id);


            var updatedAttendanceDto = new AttendanceDetailsDto // Güncellenmiş attendance'ı DTO formatına dönüştürüp geri döndürüyoruz
            {
                Id = attendance.Id,
                EventId = attendance.EventId,
                UserId = attendance.UserId,
                IsAttending = attendance.IsAttending,
                Event = new EventDto
                {
                    Id = attendance.Event.Id,
                    Name = attendance.Event.Name,
                    Location = attendance.Event.Location,
                    Date = attendance.Event.Date
                },
                User = new SimpleUserDto
                {
                    Id = attendance.User.Id,
                    Username = attendance.User.Username,
                    Email = attendance.User.Email,
                    Role = attendance.User.Role // Kullanıcının rolünü ekle
                }
            };

            return updatedAttendanceDto;
        }


        public async Task DeleteAttendanceAsync(int id) 
        {
            //var totalCount = await _context.Attendances.CountAsync(); // Toplam attendance sayısını al
            /*if (id < 0 || id > totalCount)
            {
                throw new ArgumentOutOfRangeException("ID is out of range"); // ID aralığı dışındaysa hata fırlat
            }*/
            var attendance = await _context.Attendances.FindAsync(id); // ID'ye göre attendance kaydını bul

            if (attendance == null) // Eğer attendance bulunamazsa, hata fırlat
            {
                throw new ArgumentException("Attendance not found");
            }

            _context.Attendances.Remove(attendance); // Attendance'ı sil
            await _context.SaveChangesAsync(); // Değişiklikleri kaydet
        }

    }
}


/*public async Task<PagedResultDto<AttendanceLinkDto>> GetAllAttendancesPagedAsync(string? sortBy, bool isDescending, int page, int pageSize, string baseUrl)
        {
            var query = _context.Attendances
                .Include(a => a.Event)
                .Include(a => a.User)
                .AsQueryable();

            query = sortBy?.ToLower() switch
            {
                "id" => isDescending ? query.OrderByDescending(a => a.Id) : query.OrderBy(a => a.Id),
                "event" => isDescending ? query.OrderByDescending(a => a.Event.Name) : query.OrderBy(a => a.Event.Name),
                "user" => isDescending ? query.OrderByDescending(a => a.User.Username) : query.OrderBy(a => a.User.Username),
                "isattending" => isDescending ? query.OrderByDescending(a => a.IsAttending) : query.OrderBy(a => a.IsAttending),
                _ => query.OrderBy(a => a.Id)
            };

            var totalCount = await query.CountAsync();

            var attendances = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var items = attendances.Select(attendance => new AttendanceLinkDto
            {
                Id = attendance.Id,
                IsAttending = attendance.IsAttending,
                Event = new SimpleEventDto
                {
                    Id = attendance.Event.Id,
                    Name = attendance.Event.Name,
                    Location = attendance.Event.Location,
                    Date = attendance.Event.Date
                },
                User = new SimpleUserDto
                {
                    Id = attendance.User.Id,
                    Username = attendance.User.Username,
                    Email = attendance.User.Email,
                    Role = attendance.User.Role
                },
                Links = AttendanceLinkHelper.GenerateLinks(attendance.Id, baseUrl)
            });

            return new PagedResultDto<AttendanceLinkDto>
            {
                Items = items.ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                HasMore = page * pageSize < totalCount
            };
        }*/
