public class PagedResultDto<T>
{
    public List<T> Items { get; set; } // Liste içeriği, generic tip T ile tanımlanır
    public int Page { get; set; } // Geçerli sayfa numarası
    public int PageSize { get; set; } // Sayfa boyutu
    public bool HasMore { get; set; } // Daha fazla kayıt var mı?
    public int TotalCount { get; set; } // Toplam kayıt sayısı, veritabanındaki toplam öğe sayısını temsil eder
}
