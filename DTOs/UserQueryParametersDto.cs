namespace Eventify.DTOs
{
    public class UserQueryParametersDto
    {
        public int Page { get; set; } = 1; // Sayfa numarası (varsayılan: 1)
        public int PageSize { get; set; } = 10; // Sayfa başına gösterilecek kayıt sayısı (varsayılan: 10)
        public string SortBy { get; set; } = "Id"; // Sıralama yapılacak alan (varsayılan: Id)
        public bool Descending { get; set; } = false; // Azalan sıralama mı? (false: artan, true: azalan)
    }
}
