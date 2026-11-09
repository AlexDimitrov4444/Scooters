namespace Scooters.Models
{
    // Статуси на поръчката
    public enum OrderStatus
    {
        Pending,      // Изчакваща
        Processing,   // В обработка
        Shipped,      // Изпратена
        Delivered,    // Доставена
        Cancelled     // Отказана
    }
}
