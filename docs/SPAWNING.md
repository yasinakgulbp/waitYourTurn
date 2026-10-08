# M6 — İstasyon doğuş programı ve zombi türleri

Güncelleme: 2026-10-08. Hedef sahne `Assets/Game/Scenes/TrainIntegration.unity`; fiziksel oranlar, iki taraflı 4–6 kapı ve cam/metal kuralları [MODEL_CONTRACT.md](MODEL_CONTRACT.md) ile aynıdır. Bu sistem görsel model veya hareketli çevre kökünü doğuş kaynağı olarak kullanmaz.

## Ayarlama

`Assets/Game/Content/StationPrograms` içindeki varlıklar Inspector'dan düzenlenir. Sahnedeki `Bounded station spawner` program listesini taşır. En büyük `firstStation <= mevcut istasyon` olan program seçilir: Station01 → 1–2, Station03 → 3–5, Station06 → 6 ve sonrası. Son program tekrar eder; sonsuza kadar otomatik artan sayılar eklenmedi. Bunlar ilk test değerleridir, nihai denge değildir.

Her `SpawnBand`: zombi profili, vagon indeksi (`-1`: tüm vagonlar), adet, savunmanın başlangıcına göre ilk saniye ve doğuş aralığıdır. Adet vagon başınadır; sadece başarılı doğuş veya sınırlı denemeden sonra iptal edilen istek bütçeyi tüketir. Savunma bittiğinde yetişmemiş doğuşlar sonraki istasyona taşınmaz.

| Profil | Can | Hız | Vuruş | Saldırı aralığı | Blokout rengi |
| --- | --- | --- | --- | --- | --- |
| Normal | 24 | 2,1 | 2 | 1 sn | Kahverengi |
| Fast | 16 | 3,0 | 1 | 0,75 sn | Açık yeşil |
| Tough | 70 | 1,45 | 5 | 1,4 sn | Mor |

Hızda küçük 0–0,16 varyasyon vardır. Üçü aynı `EnemyBrain`, `AgentMotor`, `MeleeAttack` ve can sistemini kullanır; saldırı ön hazırlığı 0,25 sn. Renk yalnız sunumdur. Kapsül/agent boyutu tüm türlerde 1,7 × 0,6 kalır; büyük gövdeli özel tür için yalnız modeli büyütmek yeterli değildir, agent ve geçit kabulü ayrıca gerekir.

## Bütçe ve yaşam döngüsü

- Normal programlarda tren toplamı 40 aktif, vagon başına 12 aktif; fiziksel havuz 12 × 5 = 60 nesnede sabit. Bunlar cihaz ölçümüyle kesinleşmiş optimum sayılar değildir.
- `SpawnSchedule` saf program/zaman kuralıdır. En fazla 32 band × 32 vagon, 256 kuyruk yeri kabul edilir; mevcut veride 15 akış/32 kuyruk yeri. Her akışın en fazla bir bekleyen isteği bulunur. Enqueue/işleme bütçesi mevcut programda kare başına 2; uzun karede sınırsız telafi yapılmaz.
- Kapasite dolunca istek kuyruk yerini bırakıp kendi akışında 0,25 sn ertelenir. Böylece dolu bir vagon diğerlerini kilitlemez. Bu erteleme geçersiz konum denemesi sayılmaz; savunma bitişiyle sona erer.
- NavMesh, beden/duvar çakışması veya erişilebilir kendi-vagon kapısı başarısızsa başka bir yazılmış doğuş çapası denenir. İstek en fazla 3 başarısız konum denemesinden sonra iptal edilir; ilk iptal istasyon başına bir uyarı üretir. Beden çakışma sorgusu tekrar kullanılan buffer ile yapılır; herhangi bir doluluk güvenli biçimde reddedilir.
- Canlı sayısı bütün vagon havuzlarının `Active` listelerinden alınır; içeride taşınanlar da dahildir. Ölmüş olup LateUpdate iadesini bekleyen beden o karede korumacı olarak hâlâ yer tutar. Daha düşük bir limite geçildiğinde mevcut yolcular silinmez; sayı düşene kadar yeni doğuş engellenir.
- `EnemyPool.WarmOne` kontrollü ısınır: başlangıçta kare başına en fazla 4 nesne, seçilen programda `warmupPerFrame`. İlk can modeli ve sunum bufferı ısınmada hazırlanır. `TrySpawn` hiçbir nesne yaratmaz ve havuzu büyütmez. Eski izole lablar kendi eski toplu ısınmasını ve küçük doğuş çizelgesini korur.
- Havuzdan her çıkışta can/takım/yaşam kuşağı, profil, hız, saldırı değerleri/zamanları, hedef, kapı/geometri, vagon ve doğuş istasyonu yeniden atanır. İadede hedef kaydı, kapı saldırı slotu, path ve bekleyen saldırı bırakılır. `Health.LifeVersion` eski hedef yaşamına ait hasarı reddeder.
- AI kararları 0,15 sn aralıklıdır; slot fazı doğuş, atama ve karanlıktan dönüşte korunur. Motor hedef 0,4 birim değişince veya ilgili portal sürümü değişince path yeniler. Her kare bütün sahnede hedef araması yapılmaz.
- Defense dışında doğuş yoktur. Görünür yolculukta içerideki savaş mevcut kuralla sürer; karanlık geçişte oynanış durur. Kalkışta dışarıdakiler ödülsüz havuza döner, içeridekiler aynı nesne/yaşam olarak kalır. Restart yeni program/zaman çizelgesi açar.

## Kontroller ve kanıt

EditMode: **Ctrl+Shift+F11**, `Wait Your Turn → Run → Run Spawn Tests`; rapor `Logs/SpawnAndJourneyTests.xml`. Spawn kuralları ile mevcut yolculuk/kamera grubunu birlikte çalıştırır. Sonuç 26/26 PASS (2026-10-08 15:33:53 UTC); kuyruk adaleti testinin ilk sürümde yakaladığı dolu-vagon kilitlenmesi düzeltildi.

Play: **F11**, `Logs/SpawnAcceptance.txt`. Üç profilin aynı nesnede dokuz kez yeniden kullanımı; eski yaşam hasarı, geçersiz nav, ölümle kapasite açılması, gerçek global/vagon/kare sınırları ve hedef kayıtları denetlenir. 30 hızlandırılmış istasyon döngüsünde beş orijinal içerideki zombi korunur. Stres programı geçicidir (global 9/vagon 3/kuyruk 4/kare 2); test normal ayarlara temiz Restart ile döner. Son doğrulama zamanı geliştirme kaydına yazılır.

Play **F10** mevcut kapı/yan cam/dört silah/oyuncu sınırı/onarım ve 10 geçiş kabulüdür; 60 eşzamanlı bedenlik havuz da doldurulur. Bu kontrol yeni director kapalıyken önceki çekirdeğin korunmasını ölçer; F11 director entegrasyonunu ayrıca ölçer.

PC kontrolleri Android FPS/ısı/bellek profilinin yerine geçmez. Güncel program ve çok kapılı geometri A54'te henüz ölçülmedi; kullanıcının rutin PC test tercihi korunur. Açık kapıya yeniden yönelme, görsel modeller, özel saldırı stratejileri, gerçek botlar ve ekonomik denge bu aşamanın kanıtına dahil değildir. Sıradaki uygulama M7a cüzdan/ödül/mağaza; turret ve dron onu izler.

Son M6 Play kanıtı: 2026-10-08 15:35:13 UTC PASS; üç profil/dokuz nesne yeniden kullanımı, 30 istasyon, beş kalıcı yolcu, global 9/vagon 3/kuyruk 4/kare 2 ve sabit 60 nesne. Console Play sonunda 0 hata/0 uyarı.

Son çekirdek regresyonu: 2026-10-08 15:36:24 UTC F10 PASS; 46 yan pencere/dört silah, 4/5/6 kapı varyantları, 10 vagon geçişinde kalıcılık ve 60 bedenlik sabit havuz. Kısa kanıt kopyası [generated/m6-pc-acceptance.txt](generated/m6-pc-acceptance.txt).
