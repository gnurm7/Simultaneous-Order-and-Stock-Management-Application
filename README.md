# Proje Gereksinimleri  

## Müşteri Bilgileri  

### Müşteri Türleri:  
- **CustomerID**: Müşteri Tanımlayıcı.  
- **CustomerName**: Müşteri Adı.  
- **Budget**: Müşteri Bütçesi (500 - 3000 TL arasında rastgele atanmalı).  
- **CustomerType**: Müşteri Türü (Premium/Standard).  
- **TotalSpent**: Toplam Harcama.  

### Müşteri Oluşturma:  
- Müşteri sayısı 5-10 arasında olmalıdır.  
- En az 2 adet premium müşteri başlangıçta bulunmalıdır.  

### Müşteri İşlem Sırası:  
- Premium müşterilerin işlemleri önce; Normal müşterilerin işlemleri ise Premium işlemler tamamlandıktan sonra işlenir.  
- Aynı ürün için birden fazla müşteri başvurduğunda, uygun bir işlem sırası sağlanmalıdır.  
- Bir müşteri, her ürün çeşidinden en fazla 5 adet satın alabilir.  

## Admin  
- **Yönetim Fonksiyonları**:  
  - Ürün ekleme, silme ve stok güncelleme işlemleri yapar.  
  - Admin işlemleri müşteri işlemleriyle paralel gerçekleştirilir ve admin, ürün bilgilerine erişirken diğer işlemleri beklemeye alır.  

## Stok Yönetimi  
- Her ürün başlangıçta sabit bir stok miktarıyla tanımlanır.  
- Başlangıçta 5 farklı ürün bulunmaktadır:  

| ProductID | ProductName | Stock | Price (TL) |  
|-----------|-------------|-------|------------|  
| 1         | Product1   | 500   | 100        |  
| 2         | Product2   | 10    | 50         |  
| 3         | Product3   | 200   | 45         |  
| 4         | Product4   | 75    | 75         |  
| 5         | Product5   | 0     | 500        |  

- Admin yeni ürünler ekleyebilir veya mevcut ürünleri silebilir, stok miktarlarını artırabilir veya azaltabilir.  

### Stok İşlemleri:  
- Ürün stoğu yeterli değilse işlem reddedilir.  
- Ürün satın alındığında stoklar hemen güncellenmelidir.  

## Bütçe Yönetimi  
- Müşterilerin bir bakiye hesabı vardır ve ödemeler bu bakiyeden yapılır.  
- Yetersiz bakiye durumunda işlem reddedilir.  

## Dinamik Öncelik Sistemi  
- **Öncelik Dinamikliği**: İşlem süresine göre güncellenir.  
- Premium müşteriler varsayılan olarak yüksek önceliğe sahiptir.  
- Normal müşterilerin önceliği bekleme süresi arttıkça yükseltilir.  
- Müşteri türü değişmekle birlikte, mevcut işlem sırasına dokunulmaz.  

### Dinamik Öncelik Hesaplama:  
- **Öncelik Skoru**:   
  - Formül: `ÖncelikSkoru = TemelÖncelikSkoru + (BeklemeSüresi × BeklemeSüresiAğırlığı)`  
  - **Temel Öncelik Skoru**: Premium müşteriler için 15; Normal müşteriler için 10.  
  - **Bekleme Süresi**: Siparişin onaylanmasına kadar geçen süre (saniye).  
  - **Bekleme Süresi Ağırlığı**: 0.5 puan.  

## Veritabanı  
### Minimum Tablolar:  
- **Customers**: CustomerID, CustomerName, Budget, CustomerType, TotalSpent  
- **Products**: ProductID, ProductName, Stock, Price  
- **Orders**: OrderID, CustomerID, ProductID, Quantity, TotalPrice, OrderDate, OrderStatus  
- **Logs**: LogID, CustomerID, OrderID, LogDate, LogType, LogDetails  

## Loglama ve İzleme  
- İşlem başladığında log kaydı tutulur.  
- Log kayıtları aşağıdaki bilgileri içerir:  
  - **Log ID**  
  - **Müşteri ID**  
  - **Log Türü**: “Hata”, “Uyarı”, “Bilgilendirme”  
  - **Müşteri Türü**: Premium veya Standard  
  - **Ürün ve Miktar**  
  - **İşlem Zamanı**  
  - **İşlem Sonucu**  

### Hata Mesajları:  
- "Ürün stoğu yetersiz"  
- "Zaman aşımı"  
- "Müşteri bakiyesi yetersiz"  
- "Veritabanı Hatası"  

## UI Entegrasyonu  

### Müşteri Paneli  
- **Müşteri Listesi**:  
  - Tabloda şu bilgileri gösterir:  
    - CustomerID  
    - Ad  
    - Tür (Premium/Normal)  
    - Bütçe  
    - Bekleme Süresi  
    - Öncelik Skoru  

- **Sipariş Oluşturma Formu**:  
  - Müşterinin ürün seçimi yapmasına, adet girişi yapmasına ve "Sipariş Ver" butonuna tıklayarak işlem gerçekleştirmesine olanak tanır.  

- **Bekleme Durumu**:  
  - Her müşterinin sipariş sırasındaki durumu (bekliyor, işleniyor, tamamlandı) renklendirme ile görselleştirilir.  

### Ürün Stok Durumu Paneli  
- **Ürün Tablosu**:  
  - Ürün adı, stok miktarı ve fiyat gibi bilgiler tablo halinde gösterilir.  
  - Stok miktarları her işlem sonrası güncellenmelidir.  

- **Grafik Temsili**:  
  - Bar veya dairesel grafik ile stok durumu görselleştirilebilir.  
  - Stok kritik seviyeye geldiğinde, grafik renk değiştiren bir görsel uyarı içerebilir.  

### Log Paneli  
- Gerçek zamanlı loglama ile her işlemin sonucunu listeler:  
  - Örneğin: "Müşteri 1, Product3'ten 2 adet aldı. İşlem Başarılı."  
  - Hatalı işlemler için: "Müşteri 2, Product5'ten 3 adet almak istedi. Yetersiz Stok."  

- Loglar, işlem sırasına göre kayar bir liste şeklinde gösterilmelidir ve işlemler gerçekleşirken eş zamanlı güncellenmelidir.  

### Dinamik Öncelik ve Bekleme Paneli  
- Bekleme süresi ve öncelik skoru bir tabloda gösterilir, her işlemde güncellenmelidir.  
- Müşteri sırası animasyonla gösterilerek güncellemeler görsel bir şekilde sağlanır. Örneğin, sipariş sırası değiştiğinde listeler yukarı/aşağı hareket eder.  

### Sipariş İşleme Animasyonu  
- İşlemde olan siparişler için animasyon göstergeleri sunulur:  
  - Örneğin: "Müşteri 1’in siparişi işleniyor" şeklinde bir yükleme çubuğu veya hareketli bir simge.  

## Genel İş Akışı  
1. **Müşteri Kaydı**: Sistem rastgele müşteri sayısı ve türü oluşturur.  
2. **Sipariş Verme**: Müşteri, ürünleri seçerek sipariş verir.  
3. **Log Kaydı**: Her işlem başlangıcı için bir log kaydı tutulur.  
4. **İşlem Sırası ve Öncelik**: Premium müşterilerin işlemleri öncelikli olarak işlenir. Bekleme süresine göre Normal müşterilerin önceliği artar.  
5. **Stok ve Bütçe Kontrolü**: Ürün stok durumu kontrol edilir. Yetersiz stok veya bütçe durumunda işlem reddedilir.  
6. **Veritabanı Güncellemeleri**: Başarılı işlemlerde veritabanı güncellenir.  
7. **Gerçek Zamanlı İzleme**: Loglar ve müşteri bekleme durumları anlık olarak güncellenir ve kullanıcı arayüzünde görselleştirilir.  

## Kullanıcı Rolleri  
- **Müşteri**: Ürün satın alabilir, mevcut siparişleri takip edebilir, bütçesini yönetebilir.  
- **Admin**: Ürünleri yönetebilir, stok güncellemeleri yapabilir, müşteri işlemlerini onaylayabilir veya reddedebilir.
