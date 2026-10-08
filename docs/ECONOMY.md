# M7a — Koşu parası ve mağaza

2026-10-08: TrainIntegration beş-vagon sahnesinde PC teknik kabulü geçti. Bu aşama turret/dron, fiziksel para toplama, disk/bulut kaydı veya reklam/IAP servisi içermez.

## Oyuncu kuralları

- Koşu tabanca ve 0 para ile başlar. Oyuncuya ait öldürmeler parayı otomatik cüzdana ekler. Başka sahibin öldürmesi veya istasyon/havuz temizliği ödül değildir.
- Mağaza savaşta ve görünür yolculukta açıktır; oyun, ateş ve yakınlık onarımı devam eder. Karanlık geçişte/ölümde alışveriş kapanır; vagon ataması açık paneli kapatır.
- İyileştirme `max(mevcut can × %50, 30)`, maksimum canla sınırlı: 10→40, 50→80, 80→100. Tam canda para harcanmaz.
- Toplu onarım yalnız işlem anındaki vagonun hasarlı/kırık kapılarını tam cana getirir. Aynı kapı API'si kullanılır; eşikte beden varsa fiziksel kapanışın güvenlik kuralı sürer. Ücretsiz yakınlık onarımı değişmez.
- İlk silah satın alma silahı açar, şarjör/yedeğini doldurur ve seçer. Tekrar satın alma eksik mermiyi aynı fiyatla tamamlar. Doluyken ücret alınmaz; refill devam eden reload'u bitirir ama atış bekleme süresini sıfırlamaz.
- Para, açılmış silahlar, seçili silah ve mermi vagon atamasında korunur. Restart yeni koşudur: para ve envanter başlangıca döner. Uygulama kapanınca devam M8 işidir.

## Ayarların yeri

`Assets/Game/Content/Economy/RunShop.asset` Inspector'da düzenlenir. `ShopContentBuilder` yalnız eksik asseti üretir; mevcut fiyatları yeniden yazmaz. Mevcut beş ürünün geçici fiyatları:

| Ürün | Para |
| --- | ---: |
| Can yenileme | 30 |
| Mevcut vagonun bütün kapılarını onar | 40 |
| SMG / mermi tamamlama | 60 |
| Rifle / mermi tamamlama | 100 |
| Shotgun / mermi tamamlama | 90 |

Başlangıç parası 0, başarılı alımlar arasında 0,35 sn bekleme vardır. Normal/hızlı/dayanıklı zombi ödülü 8/10/20; `Content/StationPrograms` altındaki ilgili `EnemyProfile.reward` alanıdır. Bunlar nihai ekonomi dengesi değildir. Fiyat/ödül/silah değerleri birbirinden ayrı veridir.

## Sorumluluklar ve genişletme

- Saf `WaitYourTurn.Economy` assembly: `Wallet` tamsayı ve rezervasyon; `ShopService` doğrulama/işlem; `KillRewards` ölüm ve sahiplik kontrolü. Unity, UI, spawn veya sahne bağımlılığı yoktur; yalnız ortak Combat kimlik/ölüm verisini bilir.
- `EnemyPool.Killed` gerçek `Health.Died` sinyalini profil ödülüyle iletir. Her havuz bedeni oluşturulurken bir kez abone olunur, havuz yok edilirken bırakılır. Enemy/Combat mağaza veya cüzdan bilmez.
- `RunEconomy` Run katmanındaki bağlayıcıdır: bütün vagon havuzlarını dinler, koşu/atama bağlamını takip eder, mevcut oyuncu/kapı/silah API'lerini çağırır. `RunDriver` içine ekonomi kuralları taşınmaz.
- `ShopHud` Sandbox katmanında değiştirilebilir test arayüzüdür. UI kapatıldığında ödül ve satın alma çekirdeği çalışır. Safe area ile ölçeklenir; panel dokunuşu hareket joystick'i başlatmaz. Bu beş ürünlük panel nihai mobil mağaza tasarımı değildir.
- Turret/dron öldürmesinde `DamageContext.RewardOwner` oyuncunun güncel kimlik ve yaşamını taşımak zorundadır; kaynak savunma kimliği ayrı kalır. M7a testinde bu sahiplik verisiyle ödül yolu sınandı; gerçek turret/dron henüz yoktur.

Ödül tekrarı için tüm geçmiş öldürmeler tutulmaz: her havuz bedeninin yalnız son görülen yaşam kuşağı saklanır. Beş vagonun sabit 60 bedeni kadar kayıt oluşabilir; yeni koşuda temizlenir. Aynı/eski yaşam tekrar ödül vermez, başka sahibin ölümü sonradan sahiplenilemez.

Satın alma, geçerli katalog ürünü + güncel atama bağlamı + monoton istek kimliği ister. Eski bağlam/tekrar istek/uygulanamayan ürün/yetersiz para reddedilir. Başarılı yakın alımlar arasında kısa bekleme ikinci dokunuşu sınırlar. Para önce rezerve edilir, senkron uygulama başarılı olursa düşer; başarısızlık/istisnada rezervasyon bırakılır. `IShopEffects.TryApply` sözleşmesinde false hiçbir etki uygulanmadığı anlamına gelir; kısmen değiştirilmiş hatalı bir adaptöre genel rollback sağlanmaz. Gelecekte asenkron reklam/IAP teslimi bu senkron API'ye doğrudan callback bağlayamaz; ayrı doğrulanmış teslim tasarımı gerekir.

## Doğrulama ve sınırlar

- EditMode `Wait Your Turn → Economy → Run Economy Tests` / **Ctrl+Shift+F12**: 11 ekonomi testi; negatif/aşım/rezervasyon, başarısız ve iç içe işlem, eski bağlam/istek, sahiplik/kuşak ve 10.000 yeniden kullanımda tek kayıt.
- **Ctrl+Shift+F10** silah testi: 15 vaka; mevcut 13 kurala ek kilit/açma/refill ve reload/atış temposu korunması.
- Play **F9**: gerçek tabanca öldürmesi, tek ödül/havuz yeniden kullanımı, savunma sahipliği, ödülsüz temizlik; iyileşme, yerel toplu onarım, satın alma/refill, 10 hızlandırılmış atama, karanlık/ölüm/eski panel reddi ve Restart sıfırlaması. Test UI'yi kapatarak çekirdeği doğrular; sonunda normal koşuya döner.
- Play **F10**: önceki kapı/cam/metal/beden/onarım ve 10 istasyon/60 beden kabulü.

M7a raporları ve UTC zamanları [generated/m7a-pc-acceptance.txt](generated/m7a-pc-acceptance.txt) içinde. 16:9 PC görünümü kontrol edildi. Android dokunmatik ergonomisi, uzun oturum, nihai fiyat dengesi veya reklam/IAP denenmedi. M7a değerlendirmesinde kullanıcı reklam/IAP fikirlerini iletti; araştırma ve sade ekonomi seçenekleri [MONETIZATION.md](MONETIZATION.md) içinde, henüz servis/kalıcı ürün uygulanmadı. D27/D28 boş-vagon/güvenli atama kabulü ve 17:32:15 UTC F9 mağaza regresyonu PASS; [kanıt](generated/occupancy-arrival-pc-acceptance.txt). Sıradaki M7b sabit turretler, ardından M7c dron ve Mod/Bot aşamasıdır.
