# M6 — İstasyon doğuş programı ve zombi türleri

Güncelleme: 2026-10-08. Hedef sahne `Assets/Game/Scenes/TrainIntegration.unity`; fiziksel oranlar, iki taraflı 4–6 kapı ve cam/metal kuralları [MODEL_CONTRACT.md](MODEL_CONTRACT.md) ile aynıdır. Bu sistem görsel model veya hareketli çevre kökünü doğuş kaynağı olarak kullanmaz.

## Ayarlama

`Assets/Game/Content/StationPrograms` içindeki varlıklar Inspector'dan düzenlenir. Sahnedeki `Bounded station spawner` program listesini taşır. En büyük `firstStation <= mevcut istasyon` olan program seçilir: Station01 → 1–2, Station03 → 3–5, Station06 → 6 ve sonrası. Son program tekrar eder; sonsuza kadar otomatik artan sayılar eklenmedi. Bunlar ilk test değerleridir, nihai denge değildir.

Her `SpawnBand`: zombi profili, vagon indeksi (`-1`: tüm vagonlar), adet, savunmanın başlangıcına göre ilk saniye ve doğuş aralığıdır. **Battle:** adet vagon başınadır; D27 ile canlı savunucusu olmayan vagonun akışı o istasyon için tamamen kapatılır. **M8s fiziksel Solo:** yalnız `wagon=-1` bantları tek mobil bütçe oluşturur; her yeni istek o anda insanın bulunduğu vagona yönelir. Vagon değiştirmek programı, yayılmış adetleri veya sıradaki isteği sıfırlamaz; vagon-indeksli bantlar Solo'ya çoğaltılmaz. En az bir mobil bant zorunludur. Her iki modda başarılı doğuş veya sınırlı denemeden sonra iptal edilen istek bütçeyi tüketir; savunma bittiğinde yetişmemiş doğuşlar sonraki istasyona taşınmaz.

İç zombi açık ara kapıdan başka vagona yürüdüğünde doğduğu havuzda kalır. Kapasite/ödül/kayıt sahipliği bu havuza bağlıdır; fiziksel oda üyeliği Solo topolojisinden okunur. Zombi sırf insan başka vagona yürüdü diye silinmez. Geçişte toplam canlı/havuz sınırı korunur, ek bir havuz veya sınırsız yeniden spawn eklenmez.

## D27 — Canlı savunucu uygunluğu

`WagonRuntime.Defender` ve `HasLivingDefender` kamera görünürlüğünden bağımsızdır. Şimdilik yalnız oyuncunun atandığı vagonda savunucu vardır; bot uygulaması henüz yoktur. Boş veya savunucusu ölmüş vagona yeni zombi üretilmez. Mevcut zombiler silinmez; kalkışta dışarıdakiler normal şekilde temizlenir, içeridekiler yolcu olarak kalır ve global/vagon canlı limitlerini kullanır.

`StationSpawner` uygunluğu kapasite/nav denemesinden önce kontrol eder. `SpawnResult.Unoccupied`, `SpawnSchedule` içinde ilgili akışın kalanını kapatır; `SuppressedStreams` sayacı artar. Konum hatası/uyarı veya tekrar deneme borcu oluşmaz; diğer vagonların üretimi sürer. Yeni istasyon yeni çizelge açar. İstasyon ortasında yeniden savunucu eklenen kapatılmış akış o istasyonda açılmaz; mevcut atama karanlık geçiştedir. Eski lab fallback'i de boş vagonları atlar.

İleride Mod/Bot her canlı katılımcıyı doğru vagona kaydeder; bu kayıt tek başına bot hareketi veya zombi-bot hedefleme uygulaması değildir. Yalnız turret bulunan boş vagonun yeni saldırı çekmesi ayrıca seçilmemiştir; mevcut kural canlı katılımcı ister. Görünmeyen vagon simülasyonu bu düzeltmenin parçası değildir; havuz ve kalan bedenler hâlâ mevcut bütçelerle çalışır.

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

Play: **F11**, `Logs/SpawnAcceptance.txt`. Üç profilin aynı nesnede dokuz kez yeniden kullanımı; eski yaşam hasarı, geçersiz nav, ölümle kapasite açılması, gerçek global/vagon/kare sınırları ve hedef kayıtları denetlenir. D27 ek kontrolü normal oyuncu vagonunun üretimini, boş/boşaltılan/elenen vagonlarda üretimin durmasını ve mevcut bedenlerin korunmasını denetler. Global stres için diğer vagonlara yalnız can bileşeni taşıyan test savunucuları kaydedilir; bunlar bot değildir ve üretim kuralını atlatmaz. 30 hızlandırılmış istasyon döngüsünde beş orijinal içerideki zombi korunur; D28 korumasında hasar reddi ve görünür oynanış zamanında süre dolması kontrol edilir. Stres programı geçicidir (global 9/vagon 3/kuyruk 4/kare 2); test normal ayarlara temiz Restart ile döner. Son doğrulama zamanı geliştirme kaydına yazılır.

Play **F10** mevcut kapı/yan cam/dört silah/oyuncu sınırı/onarım ve 10 geçiş kabulüdür; 60 eşzamanlı bedenlik havuz da doldurulur. Bu kontrol yeni director kapalıyken önceki çekirdeğin korunmasını ölçer; F11 director entegrasyonunu ayrıca ölçer.

PC kontrolleri Android FPS/ısı/bellek profilinin yerine geçmez. Güncel program ve çok kapılı geometri A54'te henüz ölçülmedi; kullanıcının rutin PC test tercihi korunur. Açık kapıya yeniden yönelme, görsel modeller, özel saldırı stratejileri, gerçek botlar ve ekonomik denge bu aşamanın kanıtına dahil değildir. M7a cüzdan/ödül/mağaza bağlandı; turret ve dron onu izler.

M7a: profillerde `reward` alanı normal/hızlı/dayanıklı için 8/10/20 başlangıç değeridir. `EnemyPool.Killed` gerçek ölümün `DeathNotice` verisini ve profil ödülünü iletir; `RunEconomy` sahipliği/yaşam kuşağını denetler. Temizlik, başarısız doğuş veya havuza iade ödül oluşturmaz. Havuz nesnesine ölüm aboneliği yalnız ısınmada eklenir, iadede çoğaltılmaz. Spawn programı/canlı bütçesi/geometri değişmedi; bağlama ve test kanıtı [ECONOMY.md](ECONOMY.md).

Son M6 Play kanıtı: 2026-10-08 15:35:13 UTC PASS; üç profil/dokuz nesne yeniden kullanımı, 30 istasyon, beş kalıcı yolcu, global 9/vagon 3/kuyruk 4/kare 2 ve sabit 60 nesne. Console Play sonunda 0 hata/0 uyarı.

Son çekirdek regresyonu: 2026-10-08 15:36:24 UTC F10 PASS; 46 yan pencere/dört silah, 4/5/6 kapı varyantları, 10 vagon geçişinde kalıcılık ve 60 bedenlik sabit havuz. Kısa kanıt kopyası [generated/m6-pc-acceptance.txt](generated/m6-pc-acceptance.txt).

D27/D28 güncel PC doğrulaması: 2026-10-08 17:30:01 UTC 28 spawn/yolculuk ve 17:30:11 UTC 40 can/onarım testi PASS. 17:31:13 UTC F11 boş/boşaltılan/elenmiş vagonlarda yeni üretimin durması, mevcut bedenlerin korunması, test savunucularıyla global 9/vagon 3/kuyruk 4/kare 2, dokuz profil yeniden kullanımı, 30 geçişte beş yolcu ve koruma hasar reddi/süre dolması PASS. 17:32:15 UTC F9 mağaza, 17:33:02 UTC F10 güncel 26 cam/dört silah/kapı/10 geçiş/60 beden PASS; Console 0 hata/0 uyarı. [Kısa kanıt](generated/occupancy-arrival-pc-acceptance.txt). Hızlandırılmış döngü uzun oturum veya Android performans kanıtı değildir.

## Başlangıç dengesi — turret denemesi, 2026-10-08

Kullanıcı ilk istasyonda alışverişe yetişemeden öldüğünü bildirdi. Station01 (istasyon 1–2) ayrı Intro profili kullanır: can 12, hız 1,65 m/sn (+mevcut küçük varyasyon), hasar 1, saldırı aralığı 1,2 sn, ödül 12. Yalnız bu profilden vagon başına 10 zombi; ilk doğuş 4 sn, aralık 4,5 sn, vagon canlı sınırı 4. Hızlı/dayanıklı bu ilk programdan çıkarıldı; Station03/06 ve normal/hızlı/dayanıklı profiller değişmedi. Normal turret 90: hiç para harcanmadan 8 gerçek öldürme = 96; bütün 10 öldürme 120 para. Ücretsiz para/ölümsüzlük eklenmedi; bu sayılar nihai denge değildir. Yeni profil aynı ortak brain/pool/hasar yolunu kullanır.

PC Play kontrolü: normal yeni koşuda başlangıç merkezinde otomatik tabancayla, test fixture/ölümsüzlük/para takviyesi kullanılmadan ilk dalgada 120 para ve HP100 görüldü (Defense 12,1 sn kaldı, canlı düşman 0). Console 0 hata/0 uyarı. Bu kısa kontrol kullanıcının hareket/alışveriş deneyimi veya nihai denge kabulü yerine geçmez; runtime mekanik kodu değiştirilmedi.
