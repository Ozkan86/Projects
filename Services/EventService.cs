using Microsoft.EntityFrameworkCore;
using Eventify.Data;
using Eventify.Models;
using Eventify.DTOs;

namespace Eventify.Services
{
    public class EventService : IEventService //EventService IEventService arayüzünü kullanıyor
    {
        private readonly AppDbContext _context;

        public EventService(AppDbContext context) //kurucu enjeksiyonu
        {
            _context = context;
        }

        public async Task<PagedResultDto<EventDto>> GetAllEventsWithPagingAsync(int pageNumber, int pageSize, string? sortBy, bool isDescending)
        {
            var query = _context.Events //Eager loading ile Event ve ilişkili olduğu Attendances ve User tablolarını bağlanıyor
                .Include(e => e.Attendances)
                    .ThenInclude(a => a.User)
                .AsQueryable(); //AsQueryable ile sorgu oluşturuluyor ve IQueryable olarak döndürülüyor

            // Toplam kayıt sayısı
            var totalCount = await query.CountAsync(); // Events tablosundaki toplam kayıt sayısını alır

            // Sıralama
            query = sortBy?.ToLower() switch // sortBy parametresine göre sıralama yapılıyor
            {
                "name" => isDescending ? query.OrderByDescending(e => e.Name) : query.OrderBy(e => e.Name), // isDescending doğruysa isme göre azalan sırada değilse artan sırada sırala
                "date" => isDescending ? query.OrderByDescending(e => e.Date) : query.OrderBy(e => e.Date), // isDescending doğruysa tarihe göre azalan sırada değilse artan sırada sırala
                "location" => isDescending ? query.OrderByDescending(e => e.Location) : query.OrderBy(e => e.Location), // isDescending doğruysa yere göre azalan sırada değilse artan sırada sırala
                _ => query.OrderBy(e => e.Id) // varsayılan olarak Id'ye göre artan sırada sırala
            };

            // Sayfalama
            query = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize); // geride kalan sayfa sayısı * sayfa boyutu kadar kayıtı atla ve son noktadan itibaren sayfa boyutu kadar kaydı al

            var events = await query.ToListAsync(); // sorguyu çalıştır ve sonuçları liste olarak al

            var eventDtos = events.Select(e => new EventDto // events'in içinden projeksiyonla EventDto 'ya özellikleri aktar (map)
            {
                Id = e.Id,
                Name = e.Name,
                Location = e.Location,
                Date = e.Date,
                Attendances = e.Attendances.Select(a => new AttendanceDto
                {
                    Id = a.Id,
                    IsAttending = a.IsAttending,
                    User = new SimpleUserDto
                    {
                        Id = a.User.Id,
                        Username = a.User.Username,
                        Email = a.User.Email,
                        Role = a.User.Role
                    }
                }).ToList() 
            }).ToList();  // events listesindeki her bir Event nesnesini EventDto nesnesine dönüştür ve yeni bir liste oluştur

            return new PagedResultDto<EventDto>
            {
                Items = eventDtos, // EventDto listesini Items olarak ata
                Page = pageNumber, // Geçerli sayfa numarasını ata 
                PageSize = pageSize, // Sayfa boyutunu ata
                TotalCount = totalCount, // Toplam kayıt sayısını ata
                HasMore = pageNumber * pageSize < totalCount // Daha fazla kayıt var mı? Kontrolü yapılıyor
            };
        }
        public async Task<IEnumerable<EventDto>> GetAllEventsAsync(int pageNumber, int pageSize, string? sortBy, bool isDescending)
        {
            var query = _context.Events      //eager loading ile event ve ilişkili olduğu diğer tablo bağlanıyor
                .Include(e => e.Attendances)
                    .ThenInclude(a => a.User)  //ThenInclude ile aatendances altına user bağlanıyor (eagerloading)
                .AsQueryable();

            // Sıralama
            if (!string.IsNullOrEmpty(sortBy))
            {
                switch (sortBy.ToLower())
                {
                    case "name": //sıralama parametresi isim mi
                        query = isDescending ? query.OrderByDescending(e => e.Name) : query.OrderBy(e => e.Name); //isDescending doğruysa isme göre azalan sırada değilse artan sırada sırala
                        break;
                    case "date": //sıralama parametresi tarih mi
                        query = isDescending ? query.OrderByDescending(e => e.Date) : query.OrderBy(e => e.Date); //isDescending doğruysa tarihe göre azalan sırada değilse artan sırada sırala
                        break;
                    case "location": //sıralama parametresi yer mi
                        query = isDescending ? query.OrderByDescending(e => e.Location) : query.OrderBy(e => e.Location); //isDescending doğruysa yere göre azalan sırada değilse artan sırada sırala
                        break;
                    default: // hiçbiri değilse Id'ye göre sırala (artan şekilde)
                        query = query.OrderBy(e => e.Id); // varsayılan
                        break;
                }
            }
            else
            {
                query = query.OrderBy(e => e.Id); // boş bırakıldıysa varsayılan sıralama
            }

            // Sayfalama
            query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize); // geride kalan sayfa sayısı * sayfa boyutu kadar kayıtı atla ve son noktadan itibaren sayfa boyutu kadar kaydı al

            var events = await query.ToListAsync();
            if (events == null) return null; // events boşsa geriye null döndür

            return events.Select(e => new EventDto     // events'in içinden projeksiyonla EventDto 'ya özellikleri aktar (map)
            {
                Id = e.Id,
                Name = e.Name,
                Location = e.Location,
                Date = e.Date,
                Attendances = e.Attendances.Select(a => new AttendanceDto  // events'in içindeki Attendances'i yine projeksiyonla AttendanceDto'ya aktar (attendances yukarıda eager loading ile dolduruldu)
                {
                    Id = a.Id,
                    IsAttending = a.IsAttending,
                    User = new SimpleUserDto
                    {
                        Id = a.User.Id,
                        Username = a.User.Username,
                        Email = a.User.Email,
                        Role = a.User.Role
                    }
                }).ToList()
            }).ToList();
        }

        
        public async Task<EventDto?> GetEventByIdAsync(int id)
        {
            var e = await _context.Events         //eagerloaing
                .Include(e => e.Attendances)
                    .ThenInclude(a => a.User)
                .FirstOrDefaultAsync(e => e.Id == id);  //FirstOrDefaultAsync ile id'ye göre eşleşen ilk kaydı al eşleşen kayıt yoksa default olanı döndür

            if (e == null) return null;

            return new EventDto
            {
                Id = e.Id,
                Name = e.Name,
                Location = e.Location,
                Date = e.Date,
                Attendances = e.Attendances.Select(a => new AttendanceDto //.Select ile projeksiyon yapılıyor
                {
                    Id = a.Id,
                    IsAttending = a.IsAttending,
                    User = new SimpleUserDto
                    {
                        Id = a.User.Id,
                        Username = a.User.Username,
                        Email = a.User.Email,
                        Role = a.User.Role
                    }
                }).ToList()
            };
        }
        

        public async Task<EventDto> CreateEventAsync(CreateEventDto dto)
        {
            // CreateEventDto'yu Event modeline dönüştür
            var eventEntity = new Event
            {
                Name = dto.Name,
                Location = dto.Location,
                Date = dto.Date
            };

            // Event'i veritabanına ekle
            await _context.Events.AddAsync(eventEntity); // takip edilen nesnenin durumu added olur
            await _context.SaveChangesAsync(); // yeni event veritabanına kaydedilir

            // Veritabanından kaydedilen event'i almak
            var createdEvent = await _context.Events //eagerloading
                .Include(e => e.Attendances)
                    .ThenInclude(a => a.User)
                .FirstOrDefaultAsync(e => e.Id == eventEntity.Id);  //veritabanına eklenen event'i alıyoruz

            // EventDto'ya dönüştür
            return new EventDto
            {
                Id = createdEvent.Id,
                Name = createdEvent.Name,
                Location = createdEvent.Location,
                Date = createdEvent.Date,
                Attendances = createdEvent.Attendances.Select(a => new AttendanceDto
                {
                    Id = a.Id,
                    IsAttending = a.IsAttending,
                    User = new SimpleUserDto
                    {
                        Id = a.User.Id,
                        Username = a.User.Username,
                        Email = a.User.Email,
                        Role = a.User.Role
                    }
                }).ToList() // yeni eklediğimiz kaydı yanıt olarak döndürüyoruz
            };
        }

        public async Task<bool> UpdateEventAsync(UpdateEventDto updateDto,int id)
        {
            var existingEvent = await _context.Events.FindAsync(id); //FirstOrDefaultAsync ile id'ye göre eşleşen ilk kaydı al eşleşen kayıt yoksa default olanı döndür

            if (existingEvent == null) //etkinlik bulunamazsa false döndür
            {
                return false;
            }

            existingEvent.Name = updateDto.Name; // var olan etkinliğin ismini updateDto'ya güncelle
            existingEvent.Location = updateDto.Location; // var olan etkinliğin yerini updateDto'ya güncelle
            existingEvent.Date = updateDto.Date; // var olan etkinliğin tarihini updateDto'ya güncelle (etkinlik updated durumunda)

            await _context.SaveChangesAsync(); // değişiklikleri kaydet

            return true;
        }


        public async Task DeleteEventAsync(int id)
        {
            var eventEntity = await _context.Events.FindAsync(id);

            if (eventEntity == null)
            {
                // Etkinlik bulunamadığında, exception fırlatmak ya da kontrol eklemek faydalı olabilir.
                throw new KeyNotFoundException($"Event with id {id} not found.");
            }

            _context.Events.Remove(eventEntity); //durum deleted olur
            await _context.SaveChangesAsync(); // değişiklikleri kaydet
        }

    }
}
