# E-Commerce Microservices API 

Bu proje, modern yazılım mimarisi prensipleri (Onion Architecture, CQRS, Event-Driven) ve 12-Factor App metodolojisi göz önünde bulundurularak geliştirilmiş bir E-Ticaret Mikroservis backend uygulamasıdır. Backend geliştirici mülakat süreçleri için bir **Case Study (Vaka Çalışması)** olarak tasarlanmıştır.

---

## Mimari ve Teknolojiler

Projede birbirinden bağımsız, kendi sorumluluklarına sahip mikroservisler kullanılmış olup modern kurumsal standartlar benimsenmiştir:

- **.NET 8 Web API:** Core framework.
- **Onion (Clean) Architecture:** Domain, Application, Infrastructure ve API katmanlarıyla birbirinden izole edilmiş, test edilebilir yapı.
- **CQRS Pattern:** `MediatR` kütüphanesi kullanılarak Command ve Query sorumluluklarının ayrıştırılması.
- **Event-Driven Architecture:** `RabbitMQ` ve `MassTransit` kullanılarak servisler arası asenkron, olay güdümlü iletişim.
- **API Gateway:** `YARP` ile Rate Limiting ve merkezi Routing.
- **Merkezi Loglama:** `Serilog` ve `Seq` kullanılarak asenkron, yapılandırılmış (Structured) loglama altyapısı.
- **Caching:** `Redis Distributed Cache` kullanılarak listeleme operasyonlarının optimize edilmesi.
* **Cache Invalidation (Redis):** Okuma performansını maksimize etmek için ürün listesi Redis üzerinden sunulur (CQRS - Query). Ancak veri tutarlılığını (Data Consistency) sağlamak adına; ürün ekleme, güncelleme veya silme işlemlerinde (CQRS - Command) Redis üzerindeki ilgili önbellek (`all_products`) anında silinir (**Cache Invalidation**). Böylece kullanıcıların eski (stale) veriyi görmesi engellenmiştir.
- **Kimlik Doğrulama:** `ASP.NET Core Identity` ve `JWT` kullanılarak Refresh Token destekli güvenli yetkilendirme.

---

## Servisler ve Portlar

| Servis Adı | Port | Açıklama |
| :--- | :--- | :--- |
| **Gateway.API** | `5123` | Tüm servislere tek noktadan erişim ve Rate Limiting. |
| **Auth.API** | `5259` | Kullanıcı kaydı, girişi, JWT üretimi ve Refresh Token yönetimi. |
| **Product.API** | `5127` | Ürün Ekleme/Güncelleme/Silme ve Redis Cache destekli listeleme. |
| **Log.API** | `5295` | RabbitMQ kuyruğunu dinleyip Serilog üzerinden Seq'e log basan Background Worker. |

---

## Proje Klasör Yapısı

```
ecommerce-api/
├── docker-compose.yml
└── src/
    ├── Services/
    |   ├── Gateway.API/
    │   ├── Auth/
    │   │   ├── Auth.API/
    │   │   ├── Auth.Application/
    │   │   ├── Auth.Domain/
    │   │   └── Auth.Infrastructure/
    │   └── Product/
    │       ├── Product.API/
    │       ├── Product.Application/
    │       ├── Product.Domain/
    │       └── Product.Infrastructure/
    |       ├── Log.API/
    └── Shared/
    
```

---

## Kurulum ve Çalıştırma

> **⚠️**
> Projenin incelenmesi ve test edilmesi sırasında kolaylık sağlamak amacıyla, lokal geliştirme ortamına ait veritabanı şifreleri, RabbitMQ bilgileri ve JWT anahtarları `appsettings.json` dosyalarına bilinçli olarak dahil edilmiştir. **Ekstra bir `.env` yapılandırması gerekmez.**

### Ön Koşullar

Aşağıdaki araçların sisteminizde kurulu olması gerekmektedir:

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (v24+)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [EF Core CLI](https://learn.microsoft.com/en-us/ef/core/cli/dotnet): `dotnet tool install --global dotnet-ef`

---

### 1. Projeyi Klonlayın

```bash
git clone https://github.com/Cihanyuksel/ecommerce-api.git
cd ecommerce-api
```

### 2. Altyapıyı Ayağa Kaldırın (Docker Compose)

SQL Server, RabbitMQ, Redis ve Seq servislerini başlatmak için:

```bash
docker-compose up -d
```

Tüm container'ların ayağa kalktığını doğrulamak için:

```bash
docker-compose ps
```

### 3. Veritabanı Migration'larını Uygulayın

**Auth Servisi:**
```bash
dotnet ef database update \
  --project src/Services/Auth/Auth.Infrastructure \
  --startup-project src/Services/Auth/Auth.API
```

> Auth servisi başarıyla migrate edildiğinde `cihan@admin.com` / `M12*117go` bilgileriyle otomatik bir admin kullanıcısı oluşturulur.

**Product Servisi:**
```bash
dotnet ef database update \
  --project src/Services/Product/Product.Infrastructure \
  --startup-project src/Services/Product/Product.API
```

### 4. Servisleri Çalıştırın

**IDE (Visual Studio):**

Solution Properties → Multiple Startup Projects bölümünden aşağıdaki projeleri `Start` olarak işaretleyin:

- `Gateway.API`
- `Auth.API`
- `Product.API`
- `Log.API`

**Terminal / CLI Üzerinden Servisleri Çalıştırma**

```bash
dotnet run --project src/Services/Gateway.API
dotnet run --project src/Services/Auth/Auth.API
dotnet run --project src/Services/Product/Product.API
dotnet run --project src/Services/Log.API
```

---

## Sistemi Test Etme

### 1. Giriş Yapın ve Token Alın

```
POST http://localhost:5123/api/auth/login
Content-Type: application/json

{
  "email": "cihan@admin.com",
  "password": "M12*117go"
}
```

Dönen `accessToken` değerini kopyalayın.

### 2. Yeni Ürün Ekleyin

```
POST http://localhost:5123/api/products
Authorization: Bearer <accessToken>
Content-Type: application/json

{
  "name": "Test Ürünü",
  "price": 199.99,
  "stock": 50
}
```

### 3. Logları Doğrulayın

Ürün başarıyla eklendiğinde `Product.API`, RabbitMQ'ya bir event fırlatır ve `Log.API` bu event'i tüketerek Seq'e yazar.

Seq arayüzüne (şifre: M12*117go) erişmek için: **http://localhost:5341**

---

## Yönetim Panelleri

| Servis | URL | Kimlik Bilgisi |
| :--- | :--- | :--- |
| **Seq (Loglama)** | http://localhost:5341 | `admin` / `M12*117go` |
| **RabbitMQ Yönetim** | http://localhost:15672 | `guest` / `guest` |
| **RedisInsight** | http://localhost:8001 | Şifresiz |
| **Swagger – Auth** | http://localhost:5259/swagger | — |
| **Swagger – Product** | http://localhost:5127/swagger | — |

---